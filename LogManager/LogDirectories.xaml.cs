using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public LogDirectories() {
            InitializeComponent();
            RefreshLogDirList();
        }

        private void btnAddNewLogDir_Click(object sender, RoutedEventArgs e) {
            AddNewLogDir();
            RefreshLogDirList();
        }

        private void AddNewLogDir() {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog()) {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath)) {
                    AppController.GetInstance().AddNewDirectory(fbd.SelectedPath);
                }
            }
        }

        private void btnRefreshLogDirList_Click(object sender, RoutedEventArgs e) {
            RefreshLogDirList();
        }

        private void RefreshLogDirList() {
            this.spLogDirs.Children.Clear();

            var dirs = AppController.GetInstance().GetDirectories();

            foreach (var d in dirs) {
                DockPanel dockPanel = new DockPanel();

                Label lblPathValue = new Label();
                lblPathValue.Content = d["path"];

                dockPanel.Children.Add(lblPathValue);

                this.spLogDirs.Children.Add(dockPanel);
            }
        }
    }
}
