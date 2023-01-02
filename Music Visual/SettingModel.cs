using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Music_Visual
{
    internal class SettingModel
    {
        public Pen pen { get; set; }
        public int Target { get; set; }
        public int BarWidth { get; set; }
        public int Skip { get; set; }
        public int Take { get; set; }
        public string visualType { get; set; }
        public Pen leftPen { get; set; }
        public Pen rightPen { get; set; }
        public bool isLine { get; set; }
        public int amplitude { get; set; }
        public bool smooth { get; set; }
        public Thickness margin { get; set; }
    }
}
