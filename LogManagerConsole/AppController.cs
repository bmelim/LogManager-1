using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SevenZipNET;

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

        public T GetConfigvalue<T>(string name) {
            try {
                ReadConfig();

                if (!this.Config.ContainsKey(name)) {
                    throw new Exception("Key " + name + " is not present in config file");
                }

                object value = this.Config[name];

                return (T)Convert.ChangeType(value, typeof(T));
            } catch (Exception e) {
            }

            return (T)Convert.ChangeType(new object(), typeof(T));
        }

        public bool AddNewDirectory(string path) {
            try {

                ReadConfig();

                bool found = false;
                var dirs = ((JArray)this.Config["directories"]);

                for (int i = 0; i < dirs.Count; ++i) {
                    if (path == dirs[i].Value<string>("path")) {
                        found = true;
                    }
                }

                if (!found) {
                    var entry = new Dictionary<string, string>();
                    entry.Add("path", path);
                    ((JArray)this.Config["directories"]).Add(JToken.FromObject(entry));

                    WriteConfig();
                }
            } catch (Exception e) {
            }

            return true;
        }

        public bool RemoveDirectory(string path) {
            try {

                ReadConfig();

                var dirs = ((JArray)this.Config["directories"]);

                for (int i = 0; i < dirs.Count; ++i) {
                    if (path == dirs[i].Value<string>("path")) {
                        ((JArray)this.Config["directories"]).RemoveAt(i);
                    }
                }

                WriteConfig();
            } catch (Exception e) {
            }

            return true;
        }

        public List<Dictionary<string, string>> GetDirectories() {
            ReadConfig();
            try {
                List<Dictionary<string, string>> dirs = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(JsonConvert.SerializeObject(this.Config["directories"]));
                return dirs;
            } catch (Exception e) {
            }

            return new List<Dictionary<string, string>>();
        }

        private Dictionary<string, Dictionary<DateTime, List<string>>> GetFilesToArchive() {
            Dictionary<string, Dictionary<DateTime, List<string>>> files = new Dictionary<string, Dictionary<DateTime, List<string>>>();

            foreach (var dir in JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(((JArray)this.Config["directories"]).ToString())) {
                if (!files.ContainsKey(dir["path"])) {
                    files.Add(dir["path"], new Dictionary<DateTime, List<string>>());
                }

                if (Directory.Exists(dir["path"])) {
                    foreach (var file in Directory.GetFiles(dir["path"])) {
                        if (Path.GetExtension(file) != ".7z") {
                            DateTime fdate = File.GetLastWriteTime(file);

                            if ((DateTime.Now - fdate).TotalDays > GetConfigvalue<int>("timeToLive")) {
                                DateTime keydate = new DateTime(fdate.Year, fdate.Month, fdate.Day);

                                if (!files[dir["path"]].ContainsKey(keydate)) {
                                    files[dir["path"]].Add(keydate, new List<string>());
                                }

                                Console.WriteLine("Add file: " + file + " to archive queue");
                                files[dir["path"]][keydate].Add(file);
                            }
                        }
                    }
                }
            }

            return files;
        }

        private void ArchiveFiles(DateTime date, string dirPath, List<string> filesPath) {
            StringBuilder sbZipFile = new StringBuilder();
            sbZipFile.Append(dirPath);
            sbZipFile.Append("\\");
            sbZipFile.Append(date.Year);
            sbZipFile.Append(".");
            sbZipFile.Append(date.Month);
            sbZipFile.Append(".");
            sbZipFile.Append(date.Day);
            sbZipFile.Append("_logs.7z");


            SevenZipCompressor zip = new SevenZipCompressor(sbZipFile.ToString());

            string tmpDir = Path.GetTempPath() + "logs";
            Directory.CreateDirectory(tmpDir);

            foreach (var f in filesPath) {
                string p = tmpDir + "\\" + f.Substring(f.LastIndexOf("\\") + 1);
                Console.WriteLine("Moving file: " + f + " to " + p);
                File.Move(f, p);
            }

            Console.WriteLine("Compressing " + tmpDir + "...");
            zip.CompressDirectory(tmpDir, dirPath, SevenZipNET.CompressionLevel.Normal);
            Console.WriteLine("Done compressing " + tmpDir + "!");

            Directory.Delete(tmpDir, true);

            foreach (var f in filesPath) {
                Console.WriteLine("Deleting file " + f);
                File.Delete(f);
            }
        }

        public void ArchiveLogDirectories() {
            try {

                ReadConfig();

                var dirs = GetFilesToArchive();

                foreach (var dir in dirs) {
                    foreach (var files in dir.Value) {
                        if (dir.Value.Count > 0) {
                            // IISManager.GetInstance().StopService();
                            ArchiveFiles(files.Key, dir.Key, files.Value);
                        }
                    }
                }

                // IISManager.GetInstance().StartService();
            } catch (Exception e) {
            }
        }

        public void RemoveOldLog() {
            try {

                ReadConfig();

                foreach (var dir in JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(((JArray)this.Config["directories"]).ToString())) {
                    if (Directory.Exists(dir["path"])) {
                        foreach (var file in Directory.GetFiles(dir["path"])) {
                            if (Path.GetExtension(file) == ".7z") {
                                if ((DateTime.Now - File.GetLastWriteTime(file)).TotalDays > GetConfigvalue<int>("timeToArchive")) {
                                    // IISManager.GetInstance().StopService();
                                    File.Delete(file);
                                }
                            }
                        }
                    }
                }

                // IISManager.GetInstance().StartService();
            } catch (Exception e) {
            }
        }
    }
}
