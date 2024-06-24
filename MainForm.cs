using Azure.Core;
using Azure;
using InputPattern.Database;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Diagnostics;

namespace InputPattern
{
    public partial class MainForm : Form
    {
        private List<string> filePaths;
        private List<string> fileNames;
        private string baseDirectory;

        private string connectionString = "Server=localhost;Database=Slot_Hacksaw;persist security info=True;MultipleActiveResultSets=True;User ID=sa;Password=sqlpassword123!@#;TrustServerCertificate=True";

        public MainForm()
        {
            InitializeComponent();
        }

        private void btn_Open_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Data files (*.data)|*.data|All files (*.*)|*.*";
                openFileDialog.Multiselect = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePaths = openFileDialog.FileNames.ToList();
                    fileNames = filePaths.Select(System.IO.Path.GetFileName).ToList();
                    FileTextBox.Text = string.Join(", ", fileNames);
                    baseDirectory = Path.GetDirectoryName(filePaths[0]);
                }
            }
        }

        private void btn_Start_Click(object sender, EventArgs e)
        {
            if (filePaths != null && filePaths.Count > 0)
            {
                foreach (var filePath in filePaths)
                {
                    string fileContent = File.ReadAllText(filePath);
                    List<PatWantedDeadOrAWildHacksaw> records = ParseData(fileContent, System.IO.Path.GetFileName(filePath));
                    InsertDataIntoDatabase(records);
                }
                MessageBox.Show("Data inserted successfully!");
            }
            else
            {
                MessageBox.Show("Please select files first.");
            }
        }

        private List<PatWantedDeadOrAWildHacksaw> ParseData(string data, string fileName)
        {
            var records = new List<PatWantedDeadOrAWildHacksaw>();
            var jsonEntries = data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var entry in jsonEntries)
            {
                var parsedEntry = JsonConvert.DeserializeObject<Dictionary<string, string>>(entry);
                if (parsedEntry != null)
                {
                    var request = JsonConvert.DeserializeObject<Request>(parsedEntry["Request"]);
                    var response = JsonConvert.DeserializeObject<Response>(parsedEntry["Response"]);

                    string gameCode = fileName.Split('.')[0];
                    string gameName = fileName.Split('.')[1];
                    string pType = GetPType(request, response);
                    string type = request.bets[0].buyBonus != null ? "free" : "base";
                    byte gameDone = (byte)(response.round.status == "completed" ? 0 : 1);
                    int totalWin = GetTotalWin(request, response);
                    int win = totalWin;
                    int totalBet = int.Parse(request.bets[0].betAmount);
                    double rtpDouble = ((double)totalWin / totalBet) * 100;
                    int rtp = (int)rtpDouble;

                    var record = new PatWantedDeadOrAWildHacksaw
                    {
                        gameCode = gameCode,
                        gameName = gameName,
                        pType = pType,
                        type = type,
                        gameDone = gameDone,
                        idx = "",
                        small = 1,
                        win = win,
                        totalWin = totalWin,
                        totalBet = totalBet,
                        virtualBet = totalBet,
                        rtp = rtp,
                        balance = int.Parse(response.accountBalance.balance),
                        pattern = JsonConvert.SerializeObject(response.round),
                        createdAt = response.serverTime,
                        updatedAt = response.serverTime
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

            if (request.bets[0].buyBonus != null)
            {
                pType = request.bets[0].buyBonus.ToString();
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

        private int GetTotalWin(Request request, Response response)
        {
            int totalWin = 0;

            if (request.bets.ElementAt<Bet>(0).buyBonus != null)
            {
                totalWin = int.Parse(response.round.events.Last<Event>().awa);
            }
            else
            {
                totalWin = int.Parse(response.round.events.First<Event>().awa);
            };

            return totalWin;
        }

        private void InsertDataIntoDatabase(List<PatWantedDeadOrAWildHacksaw> records)
        {
            string hashCodesFilePath = Path.Combine(baseDirectory, @$"..\..\HashCodes\{record.Split('.')[1]}");
            // Check and generate initial hash code file if necessary
            if (!File.Exists(hashCodesFilePath))
            {
                GenerateInitialHashCodeFile(records[0]);
            }

            // Load existing hash codes from file
            var existingHashCodes = File.ReadAllLines(hashCodesFilePath)
                                        .Select(line => line.Split(':'))
                                        .ToDictionary(parts => parts[0], parts => parts[1]);

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                foreach (var record in records)
                {
                    int id = GetLastId(record, connectionString);
                    int lastBig = GetLastBig(record, connectionString);

                    string eventHashCode = CalculateEventHashCodeFromPattern(record.pattern);

                    // Check if hash code already exists in the hash codes file
                    if (IsEventHashCodeAlreadyExists(eventHashCode, hashCodesFilePath))
                    {
                        // Skip insertion if record with same eventHashCode exists
                        continue;
                    }

                    string query = $@"
                        INSERT INTO pattern.pat_{record.gameName.ToLower()}_hacksaw 
                        (id, gameCode, pType, type, gameDone, idx, big, small, win, totalWin, totalBet, virtualBet, rtp, balance, pattern, createdAt, updatedAt)
                        VALUES
                        (@id, @gameCode, @pType, @type, @gameDone, @idx, @big, @small, @win, @totalWin, @totalBet, @virtualBet, @rtp, @balance, @pattern, @createdAt, @updatedAt)";
                    
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", ++id);
                        command.Parameters.AddWithValue("@gameCode", record.gameCode ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@pType", record.pType);
                        command.Parameters.AddWithValue("@type", record.type);
                        command.Parameters.AddWithValue("@gameDone", record.gameDone);
                        command.Parameters.AddWithValue("@idx", record.idx);
                        command.Parameters.AddWithValue("@big", ++lastBig);
                        command.Parameters.AddWithValue("@small", record.small);
                        command.Parameters.AddWithValue("@win", record.win);
                        command.Parameters.AddWithValue("@totalWin", record.totalWin);
                        command.Parameters.AddWithValue("@totalBet", record.totalBet);
                        command.Parameters.AddWithValue("@virtualBet", record.virtualBet);
                        command.Parameters.AddWithValue("@rtp", record.rtp);
                        command.Parameters.AddWithValue("@balance", record.balance ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@pattern", record.pattern ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@createdAt", record.createdAt);
                        command.Parameters.AddWithValue("@updatedAt", record.updatedAt);

                        command.ExecuteNonQuery();
                    }

                    UpdateHashCodesFile(record.createdAt.ToString(), eventHashCode, hashCodesFilePath);
                }
            }
        }

        private void GenerateInitialHashCodeFile(PatWantedDeadOrAWildHacksaw record)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                // Query to fetch existing hash codes from database
                string query = $"SELECT createdAt, pattern FROM pattern.pat_{record.gameName.ToLower()}_hacksaw";
                Dictionary<string, string> existingHashCodes = new Dictionary<string, string>();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string time = reader["createdAt"].ToString();
                            string pattern = reader["pattern"].ToString();

                            // Calculate hash code for the pattern field (you can adjust this logic)
                            string eventHashCode = CalculateEventHashCodeFromPattern(pattern);

                            // Store hash code in dictionary
                            existingHashCodes[time] = eventHashCode;
                        }
                    }
                }

                // Save hash codes to file
                SaveHashCodesToFile(existingHashCodes, hashCodesFilePath);
            }
        }

        private string CalculateEventHashCodeFromPattern(string pattern)
        {
            // Example hash code calculation logic (adjust as per your requirements)
            return pattern.GetHashCode().ToString();
        }

        private void SaveHashCodesToFile(Dictionary<string, string> hashCodes, string filePath)
        {
            // Save hash codes to a text file
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var kvp in hashCodes)
                {
                    writer.WriteLine($"{kvp.Key}:{kvp.Value}");
                }
            }
        }

        private bool IsEventHashCodeAlreadyExists(string eventHashCode, string hashCodesFilePath)
        {
            // Check if the given hash code exists in the hash codes file
            if (!File.Exists(hashCodesFilePath))
            {
                // If hash codes file doesn't exist, create it and return false
                return false;
            }

            // Read existing hash codes from file
            var existingHashCodes = File.ReadAllLines(hashCodesFilePath)
                                        .Select(line => line.Split(':'))
                                        .ToDictionary(parts => parts[0], parts => parts[1]);

            // Check if eventHashCode exists in the dictionary
            return existingHashCodes.ContainsValue(eventHashCode);
        }

        private void UpdateHashCodesFile(string time, string eventHashCode, string hashCodesFilePath)
        {
            // Append new hash code to the existing hash codes file
            using (StreamWriter writer = File.AppendText(hashCodesFilePath))
            {
                writer.WriteLine($"{time}:{eventHashCode}");
            }
        }

        private int GetLastId(PatWantedDeadOrAWildHacksaw record, string connectionString)
        {
            int lastId = 0;
            string query = $"SELECT ISNULL(MAX(id), 0) FROM pattern.pat_{record.gameName.ToLower()}_hacksaw";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    lastId = (int)command.ExecuteScalar();
                }
            }

            return lastId;
        }

        private int GetLastBig(PatWantedDeadOrAWildHacksaw record, string connectionString)
        {
            int lastBig = 0;
            string query = $"SELECT ISNULL(MAX(big), 0) FROM pattern.pat_{record.gameName.ToLower()}_hacksaw";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    lastBig = (int)command.ExecuteScalar();
                }
            }

            return lastBig;
        }
    }

    #region Models
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
        public int id { get; set; }
        public string gameCode { get; set; }
        public string gameName { get; set; }
        public string pType { get; set; }
        public string type { get; set; }
        public byte gameDone { get; set; }
        public string idx { get; set; }
        public int big { get; set; }
        public int small { get; set; }
        public int win { get; set; }
        public int totalWin { get; set; }
        public int totalBet { get; set; }
        public int virtualBet { get; set; }
        public int rtp { get; set; }
        public int? balance { get; set; }
        public string pattern { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
    #endregion
}
