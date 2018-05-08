using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LogManager {
    public class AppController {
        #region Singleton
        private static AppController Instance { get; set; }
        public static AppController GetInstance() {
            if (Instance == null) {
                Instance = new AppController("LogManagerConfig.json");
            }

            return Instance;
        }
        #endregion

        private string ConfigPath { get; set; }
        private Dictionary<string, object> Config { get; set; }

        private AppController(string configPath) {
            this.ConfigPath = configPath;

            ReadConfig();

            if (!this.Config.ContainsKey("directories")) {
                this.Config = new Dictionary<string, object>();
            }
        }

        private void ReadConfig() {
            this.Config = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(this.ConfigPath));
        }

        private void WriteConfig() {
            File.WriteAllText(this.ConfigPath, JsonConvert.SerializeObject(this.Config));
        }

        public bool AddNewDirectory(string path) {
            ReadConfig();

            var entry = new Dictionary<string, string>();
            entry.Add("path", path);
            ((JArray)this.Config["directories"]).Add(JToken.FromObject(entry));

            WriteConfig();

            return true;
        }

        public List<Dictionary<string, string>> GetDirectories() {
            ReadConfig();

            List<Dictionary<string, string>> dirs = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(JsonConvert.SerializeObject(this.Config["directories"]));

            return dirs;
        }
    }
}
