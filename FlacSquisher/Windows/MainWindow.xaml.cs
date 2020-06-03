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
            InitGUI();
            new FConfig(); //Initialize Config
        }
        private void InitGUI()
        {
            StringBuilder sb = new StringBuilder();
            Version myVersion = Assembly.GetExecutingAssembly().GetName().Version;
            sb.Append(Assembly.GetExecutingAssembly().GetName().Name).Append(" v").Append(myVersion.Major.ToString()).Append(".").Append(myVersion.Minor.ToString()).Append(".").Append(myVersion.Revision.ToString());
            this.Title = sb.ToString();
            sb.Clear();
            Enum.GetValues(typeof(Encode.AudioEncoders)).OfType<Encode.AudioEncoders>().All((x)=> { CMB_Encoder.Items.Add(x.GetEnumDescription()); return true; });
            Enum.GetValues(typeof(Encode.MP3.Bitrates)).OfType<Encode.MP3.Bitrates>().All((x) => { CMB_MP3_Bitrate.Items.Add(x.GetEnumDescription()); return true; });
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
            CMB_MP3_Bitrate.IsEnabled = false;
            BTN_Encode.IsEnabled = false;
            BTN_Exit.IsEnabled = false;

            Stopwatch sWatch = new Stopwatch();
            Log.Information("[MainWindow][BTN_Encode_Click] Starting processing files...");
            sWatch.Start();
            switch (selectedEncoder)
            {
                case Encode.AudioEncoders.MP3:
                    Encode.MP3.Bitrates selectedBitrate = Enum.GetValues(typeof(Encode.MP3.Bitrates)).OfType<Encode.MP3.Bitrates>().Where((x) => { return x.GetEnumDescription().Equals(CMB_MP3_Bitrate.SelectedItem.ToString()); }).FirstOrDefault();
                    Encode.MP3 k = new Encode.MP3(TXT_FLACDirectory.Text, TXT_OutputDirectory.Text, selectedBitrate);
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
            CMB_MP3_Bitrate.IsEnabled = true;
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
                    //TODO: Continue config write
                }
            }
        }
        #endregion

        private void CMB_Encoder_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GRP_LAME_Options.Visibility = Visibility.Hidden;

            switch (GetSelectedEncoder())
            {
                case Encode.AudioEncoders.MP3:
                    GRP_LAME_Options.Visibility = Visibility.Visible;
                    break;
                case Encode.AudioEncoders.OGG:
                    break;
                case Encode.AudioEncoders.WAVE:
                    break;
                default:
                    break;
            }
        }

        private Encode.AudioEncoders GetSelectedEncoder()
        {
            return Enum.GetValues(typeof(Encode.AudioEncoders)).OfType<Encode.AudioEncoders>().Where((x) => { return x.GetEnumDescription().Equals(CMB_Encoder.SelectedItem.ToString()); }).FirstOrDefault();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            FConfig.Save();
        }
    }

}
