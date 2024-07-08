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
using Newtonsoft.Json.Linq;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace InputPattern
{
    public partial class MainForm : Form
    {
        private List<string> filePaths;
        private List<string> fileNames;
        private bool isFirstInsert;

        List<string> patternHashCodes;

        private string connectionString = "Server=localhost;Database=Slot_AvatarUX;persist security info=True;MultipleActiveResultSets=True;User ID=sa;Password=sqlpassword123!@#;TrustServerCertificate=True";

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

        private async void btn_Start_Click(object sender, EventArgs e)
        {
            if (filePaths != null && filePaths.Count > 0)
            {
                foreach (var filePath in filePaths)
                {
                    var fileContent = await File.ReadAllTextAsync(filePath);
                    ParseData(fileContent);
                    //InsertDataIntoDatabase(records);
                }
                MessageBox.Show("Data inserted successfully!");
            }
            else
            {
                MessageBox.Show("Please select files first.");
            }
        }

        private void ParseData(string fileContent)
        {
            var records = new List<Pattern>();
            //var jsonEntries = JsonConvert.DeserializeObject<JObject>(fileContent);
            var stringEntries = fileContent.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var jsonEntries = new List<JObject>();
            foreach (var stringEntry in stringEntries)
            {
                var jsonEntry = JObject.Parse(stringEntry);
                jsonEntries.Add(jsonEntry);
            }

            foreach (var entry in jsonEntries)
            {
                try
                {
                    var request = JsonConvert.DeserializeObject<JObject>((string)entry["Request"]);
                    var response = JsonConvert.DeserializeObject<JObject>((string)entry["Response"]);

                    if (!response.ContainsKey("roundId") || JsonConvert.SerializeObject(response["wager"]["next"]) == "null")
                    {
                        if (request["action"] != null)
                        {
                            string action = (string)request["action"];

                            string gameCode = (string)request["game"];
                            // byte gameDone = (byte)(response.round.status == "completed" ? 0 : 1);
                            string idx = action == "main" ? "" : action;
                            int totalWin = (int)((double)response["wager"]["win"] * 100);
                            int win = totalWin;
                            string pType = action == "main" ? (win == 0 ? "base-zero" : "base-win") : "free";
                            string type = pType == "free" ? "free" : "base";
                            int totalBet = (int)((double)response["wager"]["state"]["bet"] * 100);
                            double rtpDouble = ((double)totalWin / totalBet) * 100;
                            int rtp = (int)rtpDouble;

                            var record = new Pattern
                            {
                                gameCode = gameCode,
                                pType = pType,
                                type = type,
                                gameDone = (byte)0,
                                idx = idx,
                                small = 1,
                                win = win,
                                totalWin = totalWin,
                                totalBet = totalBet,
                                virtualBet = totalBet,
                                rtp = rtp,
                                balance = 0,
                                pattern = JsonConvert.SerializeObject(response["wager"]["data"]),
                                createdAt = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                                updatedAt = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"))
                            };
                            records.Add(record);
                        }
                        else if (request["action"] == null)
                        {
                            string gameCode = (string)request["game"];
                            string roundId = (string)request["roundId"];
                            int balance = (int)((double)response["balance"] * 100);
                            
                            records.Last().balance= balance;
                            records.Last().gameDone = (byte)1;
                        }
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }

            InsertDataIntoDatabase(records);
        }

        private void InsertDataIntoDatabase(List<Pattern> records)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string sanitizedTableName = $"pat_{records[0].gameCode.Replace("-", "_")}";
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
                        INSERT INTO pattern.{sanitizedTableName} 
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

        private void UpdateBalance(string gameCode, string roundId, int balance)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string tableName = $"pat_{gameCode.Replace("-", "_")}";
                string query = $@"
                    USE Slot_AvatarUX;
                    UPDATE pattern.{tableName}
                    SET balance = {balance},
                        gameDone = 1
                    WHERE pattern LIKE '%' + {roundId} + '%'
                      AND gameDone = 0
                      AND balance = 0;";

                using (SqlCommand checkTableCommand = new SqlCommand(query, connection))
                {
                    checkTableCommand.ExecuteNonQuery();
                }
            }
        }

    #region Assist_Functions
        //private string GetPType(JToken request, JToken response)
        //{
        //    string pType = "";
        //    try
        //    {
        //        if (response["round"] == null)
        //        {
        //            return "";
        //        }

        //        if(response.round.status == "completed")
        //        {
        //            pType = "base-zero";
        //            return pType;
        //        }
        //        else
        //        {
        //            pType = "base-win";
        //            foreach (var evnt in response.round.events)
        //            {
        //                if (evnt.etn == "feature_enter" || request.bets[0].buyBonus != null)
        //                {
        //                    pType = "free";
        //                    break;
        //                }
        //                else
        //                {
        //                    continue;
        //                }
        //            }
        //            return pType;
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        return pType;
        //    }
        //}

        //private string GetIdx(JToken request, JToken response)
        //{
        //    string idx = "";
        //    try
        //    {
        //        if (request["bet"] != null || request["action"] == null || response["wager"]["next"] != null)
        //        {
        //            return "";
        //        }

        //        if ((string)request["action"] == "main")
        //        {
        //            idx = "base";
        //        }
        //        else idx = "free";
        //        return idx;
        //    }
        //    catch (Exception e)
        //    {
        //        return idx;
        //    }
        //}

        //private int GetTotalWin(Request request, Response response)
        //{

        //    int totalWin = int.Parse(response.round.events.Last<Event>().awa);

        //    return totalWin;
        //}

        private string CalculateEventHashCodeFromPattern(string pattern)
        {
            // Example hash code calculation logic (adjust as per your requirements)
            return pattern.GetHashCode().ToString();
        }

        private int GetLastId(Pattern record, string connectionString)
        {
            int lastId = 0;
            string query = $"SELECT ISNULL(MAX(id), 0) FROM pattern.pat_{record.gameCode.Replace("-", "_")}";

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

        private int GetLastBig(Pattern record, string connectionString)
        {
            int lastBig = 0;
            string query = $"SELECT ISNULL(MAX(big), 0) FROM pattern.pat_{record.gameCode.Replace("-", "_")}";

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

    public class Pattern
    {
        public int id { get; set; }
        public string gameCode { get; set; }
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
