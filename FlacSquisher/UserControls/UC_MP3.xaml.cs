using NAudio.Lame;
using System;
using System.Linq;
using System.Windows.Controls;

namespace FlacSquisher.UserControls
{
    /// <summary>
    /// Interaction logic for UC_MP3.xaml
    /// </summary>
    public partial class UC_MP3 : UserControl
    {
        public UC_MP3()
        {
            InitializeComponent();
        }
        private void CMB_MP3_Bitrate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FSConfig.Config.MP3Settings.LastMP3Bitrate = GetSelectedMP3Bitrate();
        }
        private Encode.MP3.Bitrates GetSelectedMP3Bitrate()
        {
            return Enum.GetValues(typeof(Encode.MP3.Bitrates)).OfType<Encode.MP3.Bitrates>().Where((x) => { return x.GetEnumDescription().Equals(CMB_MP3_Bitrate.SelectedItem.ToString()); }).FirstOrDefault();
        }

        private void CMB_MP3_Mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            FSConfig.Config.MP3Settings.LastMP3Mode = GetSelectedMP3Mode();
        }
        private MPEGMode GetSelectedMP3Mode()
        {
            return Enum.GetValues(typeof(MPEGMode)).OfType<MPEGMode>().Where((x) => { return x.GetEnumDescription().Equals(CMB_MP3_Mode.SelectedItem.ToString()); }).FirstOrDefault();
        }
    }
}
