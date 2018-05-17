using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LogManager {
    /// <summary>
    /// Logique d'interaction pour App.xaml
    /// </summary>
    public partial class App : Application {
        public static bool IsSilent = false;

        public App() {
            this.Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e) {
            for (int i = 0; i < e.Args.Count(); ++i) {
                switch (e.Args[i]) {
                    case "-silent":
                        App.IsSilent = true;
                        break;

                    case "-add":
                        if (i + 1 < e.Args.Count()) {
                            if (Directory.Exists(e.Args[i + 1])) {
                                AppController.GetInstance().AddNewDirectory(e.Args[i + 1]);
                            }
                        }
                        break;

                    case "-remove":
                        if (i + 1 < e.Args.Count()) {
                            if (Directory.Exists(e.Args[i + 1])) {
                                AppController.GetInstance().RemoveDirectory(e.Args[i + 1]);
                            }
                        }
                        break;
                }
            }
        }
    }
}
