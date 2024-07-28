//using Azure.Core;
//using Azure;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json.Linq;

namespace InputPattern
{
    public partial class MainForm : Form
    {
        private List<string> filePaths;
        private List<string> fileNames;
        private bool isFirstInsert;
        private int roundCount = 0;

        List<string> patternHashCodes;

        private string connectionString = "Server=localhost;Database=Slot_3Oaks;persist security info=True;MultipleActiveResultSets=True;User ID=sa;Password=sqlpassword123!@#;TrustServerCertificate=True";

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
            roundCount = 0;
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
                try
                {
                    var jsonEntry = JObject.Parse(stringEntry);
                    jsonEntries.Add(jsonEntry);
                }
                catch { 
                    continue;
                }
            }

            List<JObject> roundSpinList = new List<JObject>();

            var request = new JObject();
            var response = new JObject();
            List<string> nextActionList = new List<string>();

            foreach (var entry in jsonEntries)
            {
                try
                {
                    request = JsonConvert.DeserializeObject<JObject>((string)entry["Request"]);
                    response = JsonConvert.DeserializeObject<JObject>((string)entry["Response"]);

                    if (response["status"]["code"].ToString() != "OK") continue;

                    nextActionList = JsonConvert.DeserializeObject<List<string>>(response["context"]["actions"].ToString());

                    switch (request["action"]["name"].ToString())
                    {
                        case "spin":
                            roundSpinList.Clear();
                            roundSpinList.Add(entry);
                            if (nextActionList.Contains("spin"))
                            {
                                ProcessEntry(roundSpinList);
                                roundSpinList.Clear() ;
                            }
                            break;
                        case "buy_spin":
                            roundSpinList.Clear();
                            roundSpinList.Add(entry);
                            break;
                        case "respin":
                            if (roundSpinList.Any() && (nextActionList.Contains("respin") || nextActionList.Contains("bonus_spins_stop")))
                            {
                                roundSpinList.Add(entry);
                            }
                            else
                            {
                                roundSpinList.Clear();
                            }
                            break;
                        case "freespin":
                            if (roundSpinList.Any() && (nextActionList.Contains("freespin") || nextActionList.Contains("freespin_stop")))
                            {
                                roundSpinList.Add(entry);
                            }
                            else
                            {
                                roundSpinList.Clear();
                            }
                            break;
                        case "bonus_spins_stop":
                            if (roundSpinList.Any())
                            {
                                roundSpinList.Add(entry);
                                ProcessEntry(roundSpinList);
                            }
                            break;
                        case "freespin_stop":
                            if (roundSpinList.Any())
                            {
                                roundSpinList.Add(entry);
                                ProcessEntry(roundSpinList);
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
        }

        private void ProcessEntry(List<JObject> entryList)
        {
            var records = new List<Pattern>();

            var request = new JObject();
            var response = new JObject();

            int small = 0;

            foreach (JObject entry in entryList)
            {
                request = JsonConvert.DeserializeObject<JObject>((string)entry["Request"]);
                response = JsonConvert.DeserializeObject<JObject>((string)entry["Response"]);

                string action = (string)request["action"];

                string gameCode = fileNames[0].Split('.')[0];
                byte gameDone = (byte)(entryList.Count == 1 ? 1 : (action == "bonus_spins_stop" || action == "freespin_stop" ? 1 : 0));
                double totalWin = IsNullOrEmpty(response["context"]["spins"]["total_win"]) ? 0 : 
                    (double)(response["context"]["spins"]["total_win"]) / 100;
                double win = totalWin;
                string pType = entryList.Count > 1 ? (action == "spin" || action == "buy_spin" ? "free-start" : action == "bonus_spins_stop" || action == "freespin_stop" ? "free-end" : "free") : (totalWin > 0 ? "base-win" : "base-zero");
                string type = pType.Contains("free") ? "free" : "base";
                string idx = IsNullOrEmpty(response["context"]["spins"]["selected_mode"]) ? "" : response["context"]["spins"]["selected_mode"].ToString();
                double virtualBet = (double)(response["context"]["spins"]["round_bet"]) / 100;
                double totalBet = type == "free" ? (action == "spin" ? virtualBet : 0) : virtualBet;
                int rtp = (int)Math.Round(totalWin / virtualBet * 100);
                string balance = ((double)(response["user"]["balance"]) / 100).ToString();

                var record = new Pattern
                {
                    gameCode = gameCode,
                    pType = pType,
                    type = type,
                    gameDone = gameDone,
                    idx = idx,
                    small = ++small,
                    win = win,
                    totalWin = totalWin,
                    totalBet = totalBet,
                    virtualBet = virtualBet,
                    rtp = rtp,
                    balance = balance,
                    pattern = JsonConvert.SerializeObject(response),
                    createdAt = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                    updatedAt = DateTime.Parse(DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ"))
                };
                records.Add(record);
            }
            InsertDataIntoDatabase(records);
        }

        private void InsertDataIntoDatabase(List<Pattern> records)
        {
            roundCount++;
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
                                         $"win FLOAT, " +
                                         $"totalWin FLOAT, " +
                                         $"totalBet FLOAT, " +
                                         $"virtualBet FLOAT, " +
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
                        command.Parameters.AddWithValue("@big", roundCount);
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
        public static bool IsNullOrEmpty(JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
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
        public double win { get; set; }
        public double totalWin { get; set; }
        public double totalBet { get; set; }
        public double virtualBet { get; set; }
        public int rtp { get; set; }
        public string balance { get; set; }
        public string pattern { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime updatedAt { get; set; }
    }
}
