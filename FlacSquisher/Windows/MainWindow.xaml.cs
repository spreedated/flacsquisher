using FlacSquisher.Windows;
using NAudio.Lame;
using Serilog;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace FlacSquisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
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
        public MainWindow()
        {
            InitializeComponent();
            Log.Logger = new LoggerConfiguration().WriteTo.Debug().CreateLogger();
            new FConfig(); //Initialize Config
            InitGUI(); //Initialize GUI
        }

        #region "Buttons"
        private void BTN_Update_Click(object sender, RoutedEventArgs e)
        {
            VersionCheck versionCheck = new VersionCheck();
            versionCheck.ShowDialog();
        }
        private void BTN_Options_Click(object sender, RoutedEventArgs e)
        {
            Options k = new Options();
            k.ShowDialog();
        }
        private void BTN_Exit_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void BTN_Encode_Click(object sender, RoutedEventArgs e)
        {
            if (!new DirectoryInfo(TXT_FLACDirectory.Text).Exists | !new DirectoryInfo(TXT_OutputDirectory.Text).Exists)
            {
                MessageBox.Show("Paths not valid!\nPlease check Input & Output paths.", "Invalid paths", MessageBoxButton.OK);
                return;
            }

            Encode.AudioEncoders selectedEncoder = GetSelectedEncoder();

            CMB_Encoder.IsEnabled = false;
            UserC_MP3.IsEnabled = false;
            BTN_Encode.IsEnabled = false;
            BTN_Exit.IsEnabled = false;

            DisplayStatus.Content = "Processing files...";

            Stopwatch sWatch = new Stopwatch();
            Log.Information("[MainWindow][BTN_Encode_Click] Starting processing files...");
            sWatch.Start();
            switch (selectedEncoder)
            {
                case Encode.AudioEncoders.MP3:
                    Encode.MP3.Bitrates selectedBitrate = Enum.GetValues(typeof(Encode.MP3.Bitrates)).OfType<Encode.MP3.Bitrates>().Where((x) => { return x.GetEnumDescription().Equals(UserC_MP3.CMB_MP3_Bitrate.SelectedItem.ToString()); }).FirstOrDefault();
                    Encode.MP3 k = new Encode.MP3(TXT_FLACDirectory.Text, TXT_OutputDirectory.Text, selectedBitrate, (MPEGMode)UserC_MP3.CMB_MP3_Mode.SelectedIndex, new Progress<Encode.EncodeProgress>((x) => { DisplayStatusPerc.Content = x.Percentage.ToString() + "%"; DisplayStatusProgressBar.Value = x.Percentage; }));
                    await k.Process();
                    break;
                case Encode.AudioEncoders.WAVE:
                    Encode.WAVE w = new Encode.WAVE(TXT_FLACDirectory.Text, TXT_OutputDirectory.Text);
                    await w.Process();
                    break;
                case Encode.AudioEncoders.OGG:
                    MessageBox.Show("Sorry, vorbis is not yet implemented, check for updates!", "Not yet implemented :(", MessageBoxButton.OK);
                    break;
                case Encode.AudioEncoders.OPUS:
                    MessageBox.Show("Sorry, vorbis is not yet implemented, check for updates!", "Not yet implemented :(", MessageBoxButton.OK);
                    break;
                default:
                    Debug.Print("Unknown Encoder selected");
                    break;
            }
            sWatch.Stop();
            Log.Information(Microsoft.VisualBasic.Strings.StrDup(25, '+'));
            Log.Information("[MainWindow][BTN_Encode_Click] Entire processing finished - Duration: " + sWatch.Elapsed.ToString(@"hh\:mm\:ss\:fffffff"));
            Log.Information(Microsoft.VisualBasic.Strings.StrDup(25, '+'));

            CMB_Encoder.IsEnabled = true;
            UserC_MP3.IsEnabled = true;
            BTN_Encode.IsEnabled = true;
            BTN_Exit.IsEnabled = true;

            DisplayStatus.Content = "Finished - Process duration: " + sWatch.Elapsed.ToString(@"hh\:mm\:ss\:ff");
            Task wD = new Task(() => { Thread.Sleep(10000); });
            wD.Start();
            await wD;
            DisplayStatus.Content = "Ready";
            DisplayStatusProgressBar.Value = 0;
            DisplayStatusPerc.Content = "";
        }
        private void BTN_Change_Directory_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            TextBox txtBox = btn.Name.Contains("FLAC") ? TXT_FLACDirectory : TXT_OutputDirectory;

            System.Windows.Forms.FolderBrowserDialog folderBrowser = new System.Windows.Forms.FolderBrowserDialog();
            if (Directory.Exists(txtBox.Text))
            {
                DirectoryInfo dirInfo = new DirectoryInfo(txtBox.Text);
                folderBrowser.SelectedPath = dirInfo.FullName;
            }
            if (folderBrowser.ShowDialog() != System.Windows.Forms.DialogResult.Cancel)
            {
                txtBox.Text = folderBrowser.SelectedPath;
                if (btn.Name.Contains("FLAC") && folderBrowser.SelectedPath.TrimEnd(System.IO.Path.PathSeparator).ToLower().EndsWith("flac"))
                {
                    string acc = folderBrowser.SelectedPath.TrimEnd(System.IO.Path.DirectorySeparatorChar);
                    acc = acc.Substring(0, acc.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
                    TXT_OutputDirectory.Text = acc;
                }
                if (btn.Name.Contains("FLAC"))
                {
                    FSConfig.Config.LastInputDirectory = folderBrowser.SelectedPath; //Write to global config
                }
                else
                {
                    FSConfig.Config.LastOutputDirectory = folderBrowser.SelectedPath; //Write to global config
                }
            }
        }
        #endregion

        #region "ComboBoxes"
        private void CMB_Encoder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ToggleAllUserControls(false);

            switch (GetSelectedEncoder())
            {
                case Encode.AudioEncoders.MP3:
                    UserC_MP3.Visibility = Visibility.Visible;
                    break;
                case Encode.AudioEncoders.WAVE:
                    UserC_WAVE.Visibility = Visibility.Visible;
                    break;
                case Encode.AudioEncoders.OGG:
                    //TODO: Implement OGG Options
                    break;
                case Encode.AudioEncoders.OPUS:
                    //TODO: Implement OPUS Options
                    break;
                default:
                    break;
            }
            FSConfig.Config.LastEncoder = GetSelectedEncoder(); //Write to global config
        }
        #endregion

        #region "Helper Functions"
        private Encode.AudioEncoders GetSelectedEncoder()
        {
            return Enum.GetValues(typeof(Encode.AudioEncoders)).OfType<Encode.AudioEncoders>().Where((x) => { return x.GetEnumDescription().Equals(CMB_Encoder.SelectedItem.ToString()); }).FirstOrDefault();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            FConfig.Save();
        }
        private void ToggleAllUserControls(bool UCVisibility)
        {
            UserC_MP3.Visibility = UCVisibility ? Visibility.Visible : Visibility.Hidden;
            UserC_WAVE.Visibility = UCVisibility ? Visibility.Visible : Visibility.Hidden;
        }
        #endregion

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }

}
