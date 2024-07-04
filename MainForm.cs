//using Azure.Core;
//using Azure;
using InputPattern.Database;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Diagnostics;
using InputPattern.Models;
using Azure.Identity;
using System.IO;
using Microsoft.Web.WebView2.Core;

namespace InputPattern
{
    public partial class MainForm : Form
    {
        private List<string> filePaths;
        private List<string> fileNames;
        private bool isFirstInsert;

        List<string> patternHashCodes;

        private string connectionString = "Server=localhost;Database=Slot_Hacksaw;persist security info=True;MultipleActiveResultSets=True;User ID=sa;Password=sqlpassword123!@#;TrustServerCertificate=True";

        public MainForm()
        {
            InitializeComponent();
            patternHashCodes = new List<string>();
            isFirstInsert = true;
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
                    
                    fileList.Items.Clear();
                    foreach (var fileName in fileNames)
                    {
                        fileList.Items.Add(fileName);
                    }
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
                    List<HacksawPattern> records = ParseData(fileContent, System.IO.Path.GetFileName(filePath));
                    InsertDataIntoDatabase(records);
                }
                MessageBox.Show("Data inserted successfully!");
            }
            else
            {
                MessageBox.Show("Please select files first.");
            }
        }

        private List<HacksawPattern> ParseData(string data, string fileName)
        {
            var records = new List<HacksawPattern>();
            var jsonEntries = data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var parsedEntry = new Dictionary<string, string>();

            foreach (var entry in jsonEntries)
            {
                try
                {
                    parsedEntry = JsonConvert.DeserializeObject<Dictionary<string, string>>(entry);
                }
                catch (Exception e)
                {
                    continue;
                }
                
                if (parsedEntry != null)
                {
                    try
                    {
                        var req = JsonConvert.DeserializeObject<Request>(parsedEntry["Request"]);
                        var res = JsonConvert.DeserializeObject<Response>(parsedEntry["Response"]);
                    }
                    catch (Exception e)
                    {
                        continue;
                    }
                    var request = JsonConvert.DeserializeObject<Request>(parsedEntry["Request"]);
                    var response = JsonConvert.DeserializeObject<Response>(parsedEntry["Response"]);

                    if (response == null || response.round == null || response.accountBalance == null || response.accountBalance.balance == null)
                    {
                        continue;
                    }

                    string gameCode = fileName.Split('.')[0];
                    string gameName = fileName.Split('.')[1];
                    string pType = GetPType(request, response);
                    string type = pType == "free" ? "free" : "base";
                    //byte gameDone = (byte)(response.round.status == "completed" ? 0 : 1);
                    string idx = GetIdx(request, response);
                    int totalWin = GetTotalWin(request, response);
                    int win = totalWin;
                    int totalBet = int.Parse(request.bets[0].betAmount);
                    double rtpDouble = ((double)totalWin / totalBet) * 100;
                    int rtp = (int)rtpDouble;

                    var record = new HacksawPattern
                    {
                        gameCode = gameCode,
                        gameName = gameName,
                        pType = pType,
                        type = type,
                        gameDone = (byte)1,
                        idx = idx,
                        small = 1,
                        win = win,
                        totalWin = totalWin,
                        totalBet = totalBet,
                        virtualBet = totalBet,
                        rtp = rtp,
                        balance = int.Parse(response.accountBalance.balance),
                        pattern = JsonConvert.SerializeObject(response.round.events),
                        //pattern = JsonConvert.SerializeObject(response.round.events),
                        createdAt = response.serverTime,
                        updatedAt = response.serverTime
                    };

                    records.Add(record);
                }
            }

            return records;
        }

        private void InsertDataIntoDatabase(List<HacksawPattern> records)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sanitizedTableName = $"pat_{records[0].gameName.ToLower().Replace("'", "").Replace("!", "").Replace("_&_", "_")}";
                string checkTableQuery = $"IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'pattern' AND TABLE_NAME = '{sanitizedTableName}') " +
                                         $"BEGIN " +
                                         $"CREATE TABLE pattern.{sanitizedTableName} " +
                                         $"( " +
                                         $"id INT PRIMARY KEY, " +
                                         $"gameCode NVARCHAR(50), " +
                                         $"pType NVARCHAR(30), " +
                                         $"type NVARCHAR(30), " +
                                         $"gameDone TINYINT, " +
                                         $"idx NVARCHAR(22), " +
                                         $"big INT, " +
                                         $"small INT, " +
                                         $"win INT, " +
                                         $"totalWin INT, " +
                                         $"totalBet INT, " +
                                         $"virtualBet INT, " +
                                         $"rtp INT, " +
                                         $"balance NVARCHAR(100), " +
                                         $"pattern NVARCHAR(MAX), " +
                                         $"createdAt DATETIME, " +
                                         $"updatedAt DATETIME " +
                                         $") " +
                                         $"END";

                using (SqlCommand checkTableCommand = new SqlCommand(checkTableQuery, connection))
                {
                    checkTableCommand.ExecuteNonQuery();
                }

                if (isFirstInsert)
                {
                    string getPatternsQuery = $"SELECT pattern FROM pattern.{sanitizedTableName}";

                    using (SqlCommand command = new SqlCommand(getPatternsQuery, connection))
                    {
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string pattern = reader["pattern"].ToString();

                                // Calculate hash code for the pattern field (you can adjust this logic)
                                string eventHashCode = CalculateEventHashCodeFromPattern(pattern);

                                // Store hash code in dictionary
                                patternHashCodes.Add(eventHashCode);
                            }
                        }
                    }
                    isFirstInsert = false;
                }

                foreach (var record in records)
                {
                    int id = GetLastId(record, connectionString);
                    int lastBig = GetLastBig(record, connectionString);
                    string eventHashCode = CalculateEventHashCodeFromPattern(record.pattern);

                    // Check if hash code already exists in the hash codes file
                    if (patternHashCodes.Contains(eventHashCode))
                    {
                        continue;
                    }

                    string query = $@"
                        INSERT INTO pattern.pat_{records[0].gameName.ToLower().Replace("'", "").Replace("!", "").Replace("_&_", "_")} 
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

                    patternHashCodes.Add(eventHashCode);
                }
            }
        }

    #region Assist_Functions
        private string GetPType(Request request, Response response)
        {
            string pType = "";
            try
            {
                if (response.round == null)
                {
                    return "";
                }

                if(response.round.status == "completed")
                {
                    pType = "base-zero";
                    return pType;
                }
                else
                {
                    pType = "base-win";
                    foreach (var evnt in response.round.events)
                    {
                        if (evnt.etn == "feature_enter" || request.bets[0].buyBonus != null)
                        {
                            pType = "free";
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }
                    return pType;
                }
            }
            catch (Exception e)
            {
                return pType;
            }
        }

        private string GetIdx(Request request, Response response)
        {
            string idx = "";
            try
            {
                if (response.round == null)
                {
                    return "";
                }

                if (request.bets[0].buyBonus != null)
                {
                    idx = request.bets[0].buyBonus.ToString();
                }
                return idx;
            }
            catch (Exception e)
            {
                return idx;
            }
        }

        private int GetTotalWin(Request request, Response response)
        {
            int totalWin = int.Parse(response.round.events.Last<Event>().awa);

            return totalWin;
        }

        private string CalculateEventHashCodeFromPattern(string pattern)
        {
            // Example hash code calculation logic (adjust as per your requirements)
            return pattern.GetHashCode().ToString();
        }

        private int GetLastId(HacksawPattern record, string connectionString)
        {
            int lastId = 0;
            string query = $"SELECT ISNULL(MAX(id), 0) FROM pattern.pat_{record.gameName.ToLower().Replace("'", "").Replace("!", "").Replace("_&_", "_").Replace("boys�", "boys")}";

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

        private int GetLastBig(HacksawPattern record, string connectionString)
        {
            int lastBig = 0;
            string query = $"SELECT ISNULL(MAX(big), 0) FROM pattern.pat_{record.gameName.ToLower().Replace("'", "").Replace("!", "").Replace("_&_", "_")}";

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
    #endregion
    }

    public class HacksawPattern
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
}
