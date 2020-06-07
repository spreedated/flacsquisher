using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using FlacSquisher.UserControls;
using NAudio.Lame;
using Serilog;
using Serilog.Core;

namespace FlacSquisher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Log.Logger = new LoggerConfiguration().WriteTo.Debug().CreateLogger();
            new FConfig(); //Initialize Config
            InitGUI(); //Initialize GUI
        }
        private void InitGUI()
        {
            StringBuilder sb = new StringBuilder();
            Version myVersion = Assembly.GetExecutingAssembly().GetName().Version;
            sb.Append(Assembly.GetExecutingAssembly().GetName().Name).Append(" v").Append(myVersion.Major.ToString()).Append(".").Append(myVersion.Minor.ToString()).Append(".").Append(myVersion.Revision.ToString());
            this.Title = sb.ToString();
            sb.Clear();
            Enum.GetValues(typeof(Encode.AudioEncoders)).OfType<Encode.AudioEncoders>().All((x)=> { CMB_Encoder.Items.Add(x.GetEnumDescription()); return true; });
            Enum.GetValues(typeof(Encode.MP3.Bitrates)).OfType<Encode.MP3.Bitrates>().All((x) => { UserC_MP3.CMB_MP3_Bitrate.Items.Add(x.GetEnumDescription()); return true; });
            CMB_Encoder.SelectedIndex = (int)FSConfig.Config.LastEncoder;
            //UserControls
            UserC_MP3.Visibility = Visibility.Hidden;
            UserC_MP3.CMB_MP3_Bitrate.SelectedIndex = (int)FSConfig.Config.MP3Settings.LastMP3Bitrate;
            //# ### #
            TXT_FLACDirectory.Text = FSConfig.Config.LastInputDirectory;
            TXT_OutputDirectory.Text = FSConfig.Config.LastOutputDirectory;
            Enum.GetNames(typeof(MPEGMode)).All(x => { UserC_MP3.CMB_MP3_Mode.Items.Add(x); return true; });
            UserC_MP3.CMB_MP3_Mode.SelectedIndex = (int)FSConfig.Config.MP3Settings.LastMP3Mode;
#if DEBUG
            TXT_FLACDirectory.Text = "C:\\Users\\SpReeD\\Desktop\\fTest\\";
            TXT_OutputDirectory.Text = "C:\\Users\\SpReeD\\Desktop\\fTest\\out\\";
#endif
        }

        #region "Buttons"
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

            Stopwatch sWatch = new Stopwatch();
            Log.Information("[MainWindow][BTN_Encode_Click] Starting processing files...");
            sWatch.Start();
            switch (selectedEncoder)
            {
                case Encode.AudioEncoders.MP3:
                    Encode.MP3.Bitrates selectedBitrate = Enum.GetValues(typeof(Encode.MP3.Bitrates)).OfType<Encode.MP3.Bitrates>().Where((x) => { return x.GetEnumDescription().Equals(UserC_MP3.CMB_MP3_Bitrate.SelectedItem.ToString()); }).FirstOrDefault();
                    Encode.MP3 k = new Encode.MP3(TXT_FLACDirectory.Text, TXT_OutputDirectory.Text, selectedBitrate, (MPEGMode)UserC_MP3.CMB_MP3_Mode.SelectedItem);
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
            Log.Information(Microsoft.VisualBasic.Strings.StrDup(25,'+'));
            Log.Information("[MainWindow][BTN_Encode_Click] Entire processing finished - Duration: " + sWatch.Elapsed.ToString(@"hh\:mm\:ss\:fffffff"));
            Log.Information(Microsoft.VisualBasic.Strings.StrDup(25, '+'));

            CMB_Encoder.IsEnabled = true;
            UserC_MP3.IsEnabled = true;
            BTN_Encode.IsEnabled = true;
            BTN_Exit.IsEnabled = true;
        }
        
        private void BTN_Change_Directory_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            TextBox txtBox = btn.Name.Contains("FLAC")? TXT_FLACDirectory:TXT_OutputDirectory;

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
                    acc = acc.Substring(0,acc.LastIndexOf(System.IO.Path.DirectorySeparatorChar));
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

        private void CMB_Encoder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ToggleAllUserControls(false);

            switch (GetSelectedEncoder())
            {
                case Encode.AudioEncoders.MP3:
                    UserC_MP3.Visibility = Visibility.Visible;
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
        }
        
    }

}
