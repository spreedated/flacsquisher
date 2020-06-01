using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace FlacSquisher
{
    public static class Extensions
    {
        public static Task DoAll(this IEnumerable<Task> x)
        {
            Task t = new Task(() => {
                x.All(y => { y.Start(); return true; });
                Task.WaitAll(x.ToArray());
            });
            t.Start();
            return t;
        }
        public static string GetEnumDescription(this Enum value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());
            if (fi.GetCustomAttributes(typeof(DescriptionAttribute), false) is DescriptionAttribute[] attributes && attributes.Any())
            {
                return attributes.First().Description;
            }
            return value.ToString();
        }
    }
}
