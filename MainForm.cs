using Azure.Core;
using Azure;
using InputPattern.Database;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace InputPattern
{
    public partial class MainForm : Form
    {
        private string filePath;
        private string fileName;

        public MainForm()
        {
            InitializeComponent();
        }

        private void btn_Open_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Data files (*.data)|*.data|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                    fileName = System.IO.Path.GetFileName(filePath);
                    FileTextBox.Text = filePath;
                }
            }
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(filePath))
            {
                string fileContent = File.ReadAllText(filePath);
                List<PatWantedDeadOrAWildHacksaw> records = ParseData(fileContent);
                InsertDataIntoDatabase(records);
                MessageBox.Show("Data inserted successfully!");
            }
            else
            {
                MessageBox.Show("Please select a file first.");
            }
        }

        private List<PatWantedDeadOrAWildHacksaw> ParseData(string data)
        {
            var records = new List<PatWantedDeadOrAWildHacksaw>();
            var jsonEntries = data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            int index = 0;
            foreach (var entry in jsonEntries)
            {

                var parsedEntry = JsonConvert.DeserializeObject<Dictionary<string, string>>(entry);
                if (parsedEntry != null)
                {
                    var request = JsonConvert.DeserializeObject<Request>(parsedEntry["Request"]);
                    var response = JsonConvert.DeserializeObject<Response>(parsedEntry["Response"]);

                    string gameCode = fileName.Split('.')[0];
                    string pType = GetPType(request, response);
                    string type = request.bets.ElementAt<Bet>(0).buyBonus != null ? "free" : "base";
                    byte gameDone = (byte)(response.round.status == "completed" ? 0 : 1);
                    int big = ++index;
                    int small = 1;
                    int totalWin = GetTotalWin(response);

                    var record = new PatWantedDeadOrAWildHacksaw
                    {
                        // Populate the properties based on the parsed JSON
                        GameCode = gameCode,
                        PType = pType,
                        Type = type,
                        GameDone = gameDone,
                        Idx = "",
                        Big = big,
                        Small = small,
                        //Win = response.accountBalance.balance,
                        //TotalWin = Convert.ToDouble(response.accountBalance.balance),
                        //TotalBet = request.bets[0].betAmount,
                        Balance = response.accountBalance.balance,
                        //Pattern = response.round.events[0].c.grid,
                        //CreatedAt = DateTime.UtcNow,
                        //UpdatedAt = DateTime.UtcNow
                    };

                    records.Add(record);
                }
            }

            return records;
        }

        private string GetPType(Request request, Response response)
        {
            string pType = "";
            if (response == null)
            {
                return null;
            }

            if (request.bets.ElementAt<Bet>(0).buyBonus != null)
            {
                pType = request.bets.ElementAt<Bet>(0).buyBonus.ToString();
            }
            else if (response.round.status == "completed")
            {
                pType = "base_normal";
            }
            else if(response.round.status == "wfwpc")
            {
                pType = "base_win";
            }
            return pType;
        }

        private int GetTotalWin(Response response)
        {
            int totalWin = 0;
            
            return totalWin;
        }

        private void InsertDataIntoDatabase(List<PatWantedDeadOrAWildHacksaw> records)
        {
            string connectionString = "Your SQL Server Connection String Here";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var record in records)
                {
                    string query = @"
                        INSERT INTO PatWantedDeadOrAWildHacksaw
                        (GameCode, PType, Type, GameDone, Win, TotalWin, TotalBet, Balance, Pattern, CreatedAt, UpdatedAt)
                        VALUES
                        (@GameCode, @PType, @Type, @GameDone, @Win, @TotalWin, @TotalBet, @Balance, @Pattern, @CreatedAt, @UpdatedAt)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@GameCode", record.GameCode ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@PType", record.PType);
                        command.Parameters.AddWithValue("@Type", record.Type);
                        command.Parameters.AddWithValue("@GameDone", record.GameDone);
                        command.Parameters.AddWithValue("@Win", record.Win);
                        command.Parameters.AddWithValue("@TotalWin", record.TotalWin);
                        command.Parameters.AddWithValue("@TotalBet", record.TotalBet);
                        command.Parameters.AddWithValue("@Balance", record.Balance ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Pattern", record.Pattern ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CreatedAt", record.CreatedAt);
                        command.Parameters.AddWithValue("@UpdatedAt", record.UpdatedAt);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }
    }

    public class Request
    {
        public bool autoplay { get; set; }
        public List<Bet> bets { get; set; }
        public object offerId { get; set; }
        public object promotionId { get; set; }
        public int seq { get; set; }
        public string sessionUuid { get; set; }
    }

    public class Bet
    {
        public string betAmount { get; set; }
        public object buyBonus { get; set; }
    }

    public class Response
    {
        public Round round { get; set; }
        public bool promotionNoLongerAvailable { get; set; }
        public object promotionWin { get; set; }
        public object offer { get; set; }
        public object freeRoundOffer { get; set; }
        public int statusCode { get; set; }
        public string statusMessage { get; set; }
        public AccountBalance accountBalance { get; set; }
        public object statusData { get; set; }
        public object dialog { get; set; }
        public object customData { get; set; }
        public DateTime serverTime { get; set; }
    }

    public class Round
    {
        public string status { get; set; }
        public object jackpotWin { get; set; }
        public string roundId { get; set; }
        public List<object> possibleActions { get; set; }
        public List<Event> events { get; set; }
    }

    public class Event
    {
        public int et { get; set; }
        public string etn { get; set; }
        public string en { get; set; }
        public string ba { get; set; }
        public string bc { get; set; }
        public string wa { get; set; }
        public string wc { get; set; }
        public string awa { get; set; }
        public string awc { get; set; }
        public C c { get; set; }
    }

    public class C
    {
        public List<Action> actions { get; set; }
        public string reelSet { get; set; }
        public List<string> stops { get; set; }
        public string grid { get; set; }
    }

    public class Action
    {
        public string at { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        public string winAmount { get; set; }
        public string symbol { get; set; }
        public string mask { get; set; }
        public string count { get; set; }
    }

    public class AccountBalance
    {
        public string currencyCode { get; set; }
        public string balance { get; set; }
        public object realBalance { get; set; }
        public object bonusBalance { get; set; }
    }

    public class PatWantedDeadOrAWildHacksaw
    {
        public int Id { get; set; }
        public string GameCode { get; set; }
        public string PType { get; set; }
        public string Type { get; set; }
        public byte GameDone { get; set; }
        public string Idx { get; set; }
        public int Big { get; set; }
        public int Small { get; set; }
        public int Win { get; set; }
        public int TotalWin { get; set; }
        public int TotalBet { get; set; }
        public int VirtualBet { get; set; }
        public int Rtp { get; set; }
        public string Balance { get; set; }
        public string Pattern { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
