using NAudio.Lame;
using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;

namespace FlacSquisher
{
    public partial class MainWindow : Window
    {
        public void InitGUI()
        {
            StringBuilder sb = new StringBuilder();
            Version myVersion = Assembly.GetExecutingAssembly().GetName().Version;
            sb.Append(Assembly.GetExecutingAssembly().GetName().Name).Append(" v").Append(myVersion.Major.ToString()).Append(".").Append(myVersion.Minor.ToString()).Append(".").Append(myVersion.Revision.ToString());
            this.Title = sb.ToString();
            sb.Clear();
            //UserControls
            UserC_MP3.Visibility = Visibility.Hidden;
            Enum.GetValues(typeof(Encode.MP3.Bitrates)).OfType<Encode.MP3.Bitrates>().All((x) => { UserC_MP3.CMB_MP3_Bitrate.Items.Add(x.GetEnumDescription()); return true; });
            UserC_MP3.CMB_MP3_Bitrate.SelectedIndex = (int)FSConfig.Config.MP3Settings.LastMP3Bitrate;
            Enum.GetNames(typeof(MPEGMode)).All(x => { UserC_MP3.CMB_MP3_Mode.Items.Add(x); return true; });
            UserC_MP3.CMB_MP3_Mode.SelectedItem = Enum.GetName(typeof(MPEGMode), FSConfig.Config.MP3Settings.LastMP3Mode);
            //# ### #
            Enum.GetValues(typeof(Encode.AudioEncoders)).OfType<Encode.AudioEncoders>().All((x) => { CMB_Encoder.Items.Add(x.GetEnumDescription()); return true; });
            CMB_Encoder.SelectedIndex = (int)FSConfig.Config.LastEncoder;
            TXT_FLACDirectory.Text = FSConfig.Config.LastInputDirectory;
            TXT_OutputDirectory.Text = FSConfig.Config.LastOutputDirectory;
#if DEBUG
            TXT_FLACDirectory.Text = "C:\\Users\\SpReeD\\Desktop\\fTest\\";
            TXT_OutputDirectory.Text = "C:\\Users\\SpReeD\\Desktop\\fTest\\out\\";
#endif
        }
    }
}
