using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Linq;

namespace FlacSquisher.Windows
{
    /// <summary>
    /// Interaction logic for Options.xaml
    /// </summary>
    public partial class Options : Window
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
        public Options()
        {
            InitializeComponent();
            FSConfig.Config.FSOptions.FilesExclude.All(x => { LSB_FileExclude.Items.Add(x); return true; });
            FSConfig.Config.FSOptions.FilesInclude.All(x => { LSB_FileInclude.Items.Add(x); return true; });
        }
        #region "Buttons"
        private void BTN_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
        private void BTN_FileExclude_Add_Click(object sender, RoutedEventArgs e)
        {
            if (TXT_Fileexts.Text != null && TXT_Fileexts.Text.Length > 0)
            {
                //Add Item to TOP of the list
                List<string> acc = new List<string>() { TXT_Fileexts.Text.TrimStart('*').TrimStart('.') };
                LSB_FileExclude.Items.OfType<string>().All(x => { acc.Add(x); return true; });
                LSB_FileExclude.Items.Clear();
                acc.OfType<string>().All(x => { LSB_FileExclude.Items.Add(x); return true; });
                acc = null;
                TXT_Fileexts.Text = null;
            }
        }
        private void BTN_FileExclude_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (LSB_FileExclude.SelectedIndex > -1)
            {
                LSB_FileExclude.Items.Remove(LSB_FileExclude.SelectedItem);
            }
        }

        private void BTN_FileInclude_Add_Click(object sender, RoutedEventArgs e)
        {
            if (TXT_Fileinc.Text != null && TXT_Fileinc.Text.Length > 0)
            {
                //Add Item to TOP of the list
                List<string> acc = new List<string>() { TXT_Fileinc.Text.TrimStart('*').TrimStart('.') };
                LSB_FileInclude.Items.OfType<string>().All(x => { acc.Add(x); return true; });
                LSB_FileInclude.Items.Clear();
                acc.OfType<string>().All(x => { LSB_FileInclude.Items.Add(x); return true; });
                acc = null;
                TXT_Fileinc.Text = null;
            }
        }

        private void BTN_FileInclude_Delete_Click(object sender, RoutedEventArgs e)
        {
            if (LSB_FileInclude.SelectedIndex > -1)
            {
                LSB_FileInclude.Items.Remove(LSB_FileInclude.SelectedItem);
            }
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            List<string> acc = new List<string>();
            LSB_FileExclude.Items.OfType<string>().All(x=> { acc.Add(x); return true; });
            FSConfig.Config.FSOptions.FilesExclude = acc.ToList<string>();
            acc.Clear();
            LSB_FileInclude.Items.OfType<string>().All(x => { acc.Add(x); return true; });
            FSConfig.Config.FSOptions.FilesInclude = acc.ToList<string>();
            acc = null;
            FConfig.Save();
        }
    }
}