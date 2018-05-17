using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LogManager {
    /// <summary>
    /// Logique d'interaction pour LogDirectories.xaml
    /// </summary>
    public partial class LogDirectories : Window {
        private Timer timer = null;
        private System.Windows.Forms.NotifyIcon notifyIcon = null;
        private ContextMenu notifyMenu = null;

        private Label SelectedLabel = null;

        public LogDirectories() {
            this.Initialized += LogDirectories_Initialized;
            this.Loaded += LogDirectories_Loaded;

            if (!App.IsSilent) {
                InitializeComponent();

                this.Visibility = Visibility.Visible;
                this.ShowInTaskbar = true;

                RefreshLogDirList();
            }

            AppController.GetInstance().RemoveOldLog();
            AppController.GetInstance().ArchiveLogDirectories();


            this.timer = new Timer(1000 * AppController.GetInstance().GetConfigvalue<int>("archivingInterval"));
            this.timer.Elapsed += Timer_Elapsed;
            this.timer.AutoReset = true;
            this.timer.Enabled = true;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e) {
            AppController.GetInstance().RemoveOldLog();
            AppController.GetInstance().ArchiveLogDirectories();
        }

        private void LogDirectories_Loaded(object sender, RoutedEventArgs e) {
            notifyIcon.Visible = true;
        }

        private void LogDirectories_Initialized(object sender, EventArgs e) {
            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Click += NotifyIcon_Click;
            notifyIcon.Icon = LogManager.Properties.Resources.icon;

            notifyMenu = new ContextMenu();
            MenuItem mi_ShowHideWindow = new MenuItem();
            mi_ShowHideWindow.Header = "Show/Hide Window";
            mi_ShowHideWindow.Click += Mi_ShowHideWindow_Click;
            notifyMenu.Items.Add(mi_ShowHideWindow);
            MenuItem mi_Quit = new MenuItem();
            mi_Quit.Header = "Quit";
            mi_Quit.Click += Mi_Quit_Click;
            notifyMenu.Items.Add(mi_Quit);
        }

        private void NotifyIcon_Click(object sender, EventArgs e) {
            notifyMenu.IsOpen = true;
        }

        private void Mi_Quit_Click(object sender, RoutedEventArgs e) {
            this.Close();
        }

        private void Mi_ShowHideWindow_Click(object sender, RoutedEventArgs e) {
            if (this.Visibility == Visibility.Visible) {
                this.Hide();
                this.ShowInTaskbar = false;
            } else {
                this.Show();
                this.ShowInTaskbar = true;
            }
        }

        private void btnAddNewLogDir_Click(object sender, RoutedEventArgs e) {
            AddNewLogDir();
            RefreshLogDirList();
        }

        private void AddNewLogDir() {
            try {
                using (var fbd = new System.Windows.Forms.FolderBrowserDialog()) {
                    System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                    if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                        AppController.GetInstance().AddNewDirectory(fbd.SelectedPath);
                        AppController.GetInstance().RemoveOldLog();
                        AppController.GetInstance().ArchiveLogDirectories();
                    }
                }
            }catch(Exception e) {
                Logger.GetInstance().LogError(e.ToString());
            }
        }

        private void btnRefreshLogDirList_Click(object sender, RoutedEventArgs e) {
            RefreshLogDirList();
            AppController.GetInstance().RemoveOldLog();
            AppController.GetInstance().ArchiveLogDirectories();
        }

        private void RefreshLogDirList() {
            this.spLogDirs.Children.Clear();

            var dirs = AppController.GetInstance().GetDirectories();

            foreach (var d in dirs) {
                DockPanel dockPanel = new DockPanel();

                Label lblPathValue = new Label();
                lblPathValue.Content = d["path"];
                lblPathValue.MouseDown += (object sender, MouseButtonEventArgs e) => {
                    this.SelectedLabel = (Label)sender;
                };

                dockPanel.Children.Add(lblPathValue);

                this.spLogDirs.Children.Add(dockPanel);
            }
        }

        private void btnRemoveLogDir_Click(object sender, RoutedEventArgs e) {
            if (this.SelectedLabel != null) {
                AppController.GetInstance().RemoveDirectory(this.SelectedLabel.Content.ToString());
                this.SelectedLabel = null;
                RefreshLogDirList();
            }
        }

        private void btnStartIIS_Click(object sender, RoutedEventArgs e) {
            IISManager.GetInstance().StartService();
        }

        private void btnStopIIS_Click(object sender, RoutedEventArgs e) {
            IISManager.GetInstance().StopService();
        }

        private void btnClean_Click(object sender, RoutedEventArgs e) {
            AppController.GetInstance().RemoveOldLog();
        }

        private void btnArchive_Click(object sender, RoutedEventArgs e) {
            AppController.GetInstance().ArchiveLogDirectories();
        }
    }
}
