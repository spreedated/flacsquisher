﻿using System;
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
        private MainWindow mainWref;
        public UC_MP3()
        {
            InitializeComponent();
            //this.mainWref = mainWref;
        }
        private void CMB_MP3_Bitrate_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //FSConfig.Config.MP3Settings.LastMP3Bitrate = mainWref.GetSelectedMP3Bitrate();
        }
    }
}
