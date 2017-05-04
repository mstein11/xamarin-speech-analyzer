using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using MySql.Data.MySqlClient;

namespace Happimeter.Server.Data.HappimeterDatabase
{
    public class DatabaseContext
    {
        public string DatabaseName => ConfigurationManager.AppSettings["Happymeter.Databasename"];
        public string Password => ConfigurationManager.AppSettings["Happymeter.Databasepw"];
        public string Username => ConfigurationManager.AppSettings["Happymeter.Databaseuser"];
        public string Server => ConfigurationManager.AppSettings["Happymeter.Databaseserver"];

        private MySqlConnection _connection;
        public MySqlConnection Connection => _connection;

        private static DatabaseContext _instance;
        public static DatabaseContext Instance()
        {
            return _instance ?? (_instance = new DatabaseContext());
        }

        public void GetGpsCoordsFromUser()
        {
            IsConnect();
            var cmd = _connection.CreateCommand();

            cmd.CommandText =
                //"SELECT * FROM sensor_data sd, mood_data md, user u WHERE u.id = sd.user_id AND sd.user_id = md.user_id AND DATE(md.timestamp) = DATE(sd.timestamp) AND DATE(md.timestamp) = CURDATE() AND u.mail = @mail";
                "SELECT u.id,u.mail, sd.timestamp, sd.avgbpm, sd.activity, sd.avglightlevel, sd.positionlat, sd.positionlon, sd.vmc, md.pleasant, md.activation FROM sensor_data sd, mood_data md, user u WHERE u.id = sd.user_id AND sd.user_id = md.user_id AND DATE(md.timestamp) = DATE(sd.timestamp) AND DATE(md.timestamp) = SUBDATE(CURDATE(),1)";
            cmd.Parameters.AddWithValue("mail", "pbudner@mit.edu");
            var reader = cmd.ExecuteReader();
            var resultList = new List<object>();
            while (reader.Read())
            {
                var tmp = new {Id = reader["id"]};
                resultList.Add(resultList);
            }
            Close();
        }

        public List<MoodData> GetMoodData(string userMail, DateTime referenceDate)
        {
            IsConnect();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT u.id,u.mail, md.pleasant, md.activation, md.timestamp FROM mood_data md, user u WHERE u.id = md.user_id AND DATE(md.timestamp) = DATE(@date) AND u.mail = @mail";
            cmd.Parameters.AddWithValue("mail", "pbudner@mit.edu");
            cmd.Parameters.AddWithValue("date", referenceDate.ToString("yyyy-MM-dd HH:mm:ss"));

            var reader = cmd.ExecuteReader();
            var resultList = new List<MoodData>();
            while (reader.Read())
            {
                var tmp = new MoodData
                {
                    Email = reader["mail"].ToString(),
                    Activation = int.Parse(reader["activation"].ToString()),
                    Pleasant = int.Parse(reader["pleasant"].ToString()),
                    Timestamp = DateTime.Parse(reader.GetString("timestamp"))
                };
                resultList.Add(tmp);
            }
            reader.Close();
            var cmd2 = _connection.CreateCommand();
            cmd2.CommandText =
                "SELECT u.id,u.mail, cp.pleasance, cp.activation, cp.predicted_at as timestamp FROM classifier_predictions cp, user u WHERE u.id = cp.user_id AND DATE(cp.predicted_at) = DATE(@date) AND u.mail = @mail";
            cmd2.Parameters.AddWithValue("mail", "pbudner@mit.edu");
            cmd2.Parameters.AddWithValue("date", referenceDate.ToString("yyyy-MM-dd HH:mm:ss"));
            var reader2 = cmd2.ExecuteReader();
            while (reader2.Read())
            {
                var tmp = new MoodData
                {
                    Email = reader2["mail"].ToString(),
                    Activation = int.Parse(reader2["activation"].ToString()),
                    Pleasant = int.Parse(reader2["pleasance"].ToString()),
                    Timestamp = DateTime.Parse(reader2.GetString("timestamp"))
                };
                resultList.Add(tmp);
            }
            Close();

            return resultList;
        }

        public List<SensorData> GetSensorData(string userMail, DateTime referenceDate)
        {
            IsConnect();
            var cmd = _connection.CreateCommand();
            cmd.CommandText = "SELECT u.id, u.mail, sd.timestamp, sd.avgbpm, sd.activity, sd.avglightlevel, sd.positionlat, sd.positionlon, sd.vmc FROM sensor_data sd, user u WHERE u.id = sd.user_id AND DATE(sd.timestamp) = DATE(@date) AND u.mail = @mail";
            cmd.Parameters.AddWithValue("mail", "pbudner@mit.edu");
            cmd.Parameters.AddWithValue("date", referenceDate.ToString("yyyy-MM-dd HH:mm:ss"));

            var reader = cmd.ExecuteReader();
            var resultList = new List<SensorData>();
            while (reader.Read())
            {
                var tmp = new SensorData()
                {
                    AvgBpm = int.Parse(reader["avgbpm"].ToString()),
                    LightLevel = int.Parse(reader["avglightlevel"].ToString()),
                    Vmc = long.Parse(reader["vmc"].ToString()),
                    Activity = int.Parse(reader["activity"].ToString()),
                    GeoLat = double.Parse(reader["positionlat"].ToString()),
                    GeoLng = double.Parse(reader["positionlon"].ToString()),
                    Timestamp = DateTime.Parse(reader.GetString("timestamp"))
                };
                resultList.Add(tmp);
            }
            Close();
            return resultList;
        }

        public bool IsConnect()
        {
            if (Connection != null)
                return true;

            string connstring = $"Server={Server}; database={DatabaseName}; UID={Username}; password={Password}";
            _connection = new MySqlConnection(connstring);
            try
            {
                _connection.Open();
            }
            catch (Exception e)
            {
                _connection = null;
                return false;
            }
            return true;
        }

        public void Close()
        {
            _connection.Close();
            _connection = null;
        }
    }
}