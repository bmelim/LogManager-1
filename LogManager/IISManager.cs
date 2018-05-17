using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LogManager {
    public class IISManager {
        #region Singleton
        private static IISManager Instance { get; set; }
        public static IISManager GetInstance() {
            if (Instance == null) {
                Instance = new IISManager("piweb.rollandinc.com");
            }

            return Instance;
        }
        #endregion

        private string MachineName { get; set; }
        private ServiceController Service { get; set; }

        private IISManager(string machineName) {
            this.MachineName = machineName;
        }

        private bool WaitForService() {
            try {
                this.Service = new ServiceController();
                this.Service.MachineName = this.MachineName;
                this.Service.ServiceName = "w3svc";

                if (this.Service != null) {
                    do {
                        this.Service.Refresh();
                    } while (
                    this.Service.Status == ServiceControllerStatus.ContinuePending ||
                    this.Service.Status == ServiceControllerStatus.PausePending ||
                    this.Service.Status == ServiceControllerStatus.StartPending ||
                    this.Service.Status == ServiceControllerStatus.StopPending);

                    return true;
                }
            } catch (Exception e) {
                Logger.GetInstance().LogError(JsonConvert.SerializeObject(e));
            }

            return false;
        }

        public bool StopService() {
            try {
                if (WaitForService()) {
                    if (this.Service.Status == ServiceControllerStatus.Running || this.Service.Status == ServiceControllerStatus.Paused) {
                        Logger.GetInstance().LogWarning("Stopping IIS on " + this.MachineName + "...");
                        this.Service.Stop();
                        this.Service.WaitForStatus(ServiceControllerStatus.Stopped);
                    }

                    this.Service.Close();

                    return true;
                }
            } catch (Exception e) {
                Logger.GetInstance().LogError(JsonConvert.SerializeObject(e));
            }

            return false;
        }

        public bool StartService() {
            try {
                if (WaitForService()) {
                    if (this.Service.Status == ServiceControllerStatus.Stopped) {
                        Logger.GetInstance().LogWarning("Starting IIS on " + this.MachineName + "...");
                        this.Service.Start();
                        this.Service.WaitForStatus(ServiceControllerStatus.Running);
                    } else if (this.Service.Status == ServiceControllerStatus.Paused) {
                        this.Service.Continue();
                        this.Service.WaitForStatus(ServiceControllerStatus.Running);
                    }

                    this.Service.Close();

                    return true;
                }
            } catch (Exception e) {
                Logger.GetInstance().LogError(JsonConvert.SerializeObject(e));
            }

            return false;
        }
    }
}
