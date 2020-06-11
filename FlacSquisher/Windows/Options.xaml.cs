using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

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
            FSConfig.Config.FSOptions.FilesInclude.All(x => { LSB_FileInclude.Items.Add(x); return true; });
            CHK_UpdateStartup.DataContext = FSConfig.Config.FSOptions;
        }
        #region "Buttons"
        private void BTN_Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
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
                int selItem = LSB_FileInclude.SelectedIndex;
                LSB_FileInclude.Items.Remove(LSB_FileInclude.SelectedItem);
                LSB_FileInclude.SelectedIndex = selItem;
            }
        }

        private void BTN_Save_Click(object sender, RoutedEventArgs e)
        {
            List<string> acc = new List<string>();
            LSB_FileInclude.Items.OfType<string>().All(x => { acc.Add(x); return true; });
            FSConfig.Config.FSOptions.FilesInclude = acc.ToList<string>();
            acc = null;

            FConfig.Save();
            this.Close();
        }

        private void BTN_Default_Click(object sender, RoutedEventArgs e)
        {
            LSB_FileInclude.Items.Clear();
            LSB_FileInclude.Items.Add("png");
            LSB_FileInclude.Items.Add("jpg");
            CHK_UpdateStartup.IsChecked = true;
        }

        private void BTN_Update_Click(object sender, RoutedEventArgs e)
        {
            VersionCheck versionCheck = new VersionCheck();
            versionCheck.ShowDialog();
        }
        #endregion

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}