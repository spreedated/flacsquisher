using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
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

        private readonly Update UpdateObj = new Update();
        public VersionCheck()
        {
            InitializeComponent();
            DoUpdateCheck();
        }
        private async void DoUpdateCheck()
        {
            GRP_Wait.Visibility = Visibility.Visible;
            GRP_Results.Visibility = Visibility.Hidden;
            Task t = new Task(() =>
            {
                UpdateObj.OnlineCheck();
            });
            t.Start();
            await t;

            if (UpdateObj.GitHubResponse == null)
            {
                TXB_SomeCoolText.Text = "Some error occurred!\n\nCheck your internet connection and firewall.";
                BTN_Download.IsEnabled = false;
                BTN_Cool.Content = "Uncool";
                GRP_Results.Visibility = Visibility.Hidden;
                GRP_Wait.Visibility = Visibility.Hidden;
                return;
            }

            if (Assembly.GetExecutingAssembly().GetName().Version < new Version(UpdateObj.GitHubResponse.Version.Major, UpdateObj.GitHubResponse.Version.Minor, UpdateObj.GitHubResponse.Version.Build, 0))
            {
                TXB_SomeCoolText.Text = "Good news!\nNew version available.";
            }
            else if (Assembly.GetExecutingAssembly().GetName().Version > new Version(UpdateObj.GitHubResponse.Version.Major, UpdateObj.GitHubResponse.Version.Minor, UpdateObj.GitHubResponse.Version.Build, 0))
            {
                TXB_SomeCoolText.Text = "Cool!\nYou have a newer version than online. You wizard!";
                BTN_Download.IsEnabled = false;
            }
            else
            {
                TXB_SomeCoolText.Text = "No newer version\nYou are on the latest " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
            LBL_CurrVers.Content = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            LBL_OnVers.Content = UpdateObj.GitHubResponse.Version.ToString();
            LBL_OnBranch.Content = UpdateObj.GitHubResponse.Branch;
            LBL_OnDate.Content = UpdateObj.GitHubResponse.PublishDate.ToString();
            LBL_OnDesc.Content = UpdateObj.GitHubResponse.Description;
            LBL_OnSize.Content = ((double)UpdateObj.GitHubResponse.Files[0].Bytes / 1024 / 1024).ToString("#.00", System.Globalization.CultureInfo.InvariantCulture) + " MiB";

            GRP_Wait.Visibility = Visibility.Hidden;
            GRP_Results.Visibility = Visibility.Visible;
        }

        private void BTN_Cool_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BTN_Download_Click(object sender, RoutedEventArgs e)
        {
            UpdateObj.DownloadZIP();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
