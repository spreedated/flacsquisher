using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using FlacSquisher;

namespace FlacSquisher.Statics
{
    static public class Threads
    {
        public static List<ComboBoxItem> Items
        {
            get
            {
                List<ComboBoxItem> acc = new List<ComboBoxItem>();
                for (int i = 0; i < Environment.ProcessorCount; i++)
                {
                    acc.Add(new ComboBoxItem() { Content = (i + 1).ToString() });
                }
                return acc;
            }
        }
    }
}
