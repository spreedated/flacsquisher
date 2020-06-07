using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

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
    }
}
