using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Serilog;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace FlacSquisher.Windows
{
    /// <summary>
    /// Interaction logic for VersionCheck.xaml
    /// </summary>
    public partial class VersionCheck : Window
    {
        #region "Hide X Button"
        private const int GWL_STYLE = -16;
        private const int WS_SYSMENU = 0x80000;
        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            SetWindowLong(hwnd, GWL_STYLE, GetWindowLong(hwnd, GWL_STYLE) & ~WS_SYSMENU);
        }
        #endregion
        private string dlUrl = null;
        public VersionCheck()
        {
            InitializeComponent();
            Update acc = new Update();
            acc.OnlineCheck();
            if (acc.GitHubResponse == null)
            {
                TXB_SomeCoolText.Text = "Some error occurred!\n\nCheck your internet connection and firewall.";
                BTN_Download.IsEnabled = false;
                GRP_Results.Visibility = Visibility.Hidden;
                return;
            }

            if (Assembly.GetExecutingAssembly().GetName().Version < acc.GitHubResponse.Version)
            {
                TXB_SomeCoolText.Text = "Good news!\nNew version available.";
            }
            else if (Assembly.GetExecutingAssembly().GetName().Version > acc.GitHubResponse.Version)
            {
                TXB_SomeCoolText.Text = "Cool!\nYou have a newer version than online. You wizard!";
            }
            else
            {
                TXB_SomeCoolText.Text = "No newer version\nYou are on the latest " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            LBL_CurrVers.Content = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            LBL_OnVers.Content = acc.GitHubResponse.Version.ToString();
            LBL_OnBranch.Content = acc.GitHubResponse.Branch;
            LBL_OnDate.Content = acc.GitHubResponse.PublishDate.ToString();
            LBL_OnDesc.Content = acc.GitHubResponse.Description;
            LBL_OnSize.Content = ((double)acc.GitHubResponse.Files[0].Bytes / 1024 / 1024).ToString("#.00", System.Globalization.CultureInfo.InvariantCulture) + " MiB";
            dlUrl = acc.GitHubResponse.Files[0].DownloadURL;
        }

        private void BTN_Cool_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BTN_Download_Click(object sender, RoutedEventArgs e)
        {
            if (dlUrl != null)
            {
                Process.Start(Update.GitHubAPILink);
            }
        }
    }
}
