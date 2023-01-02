using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Music_Visual
{
    /// <summary>
    /// SettingWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SettingWindow : Window
    {
        MainWindow main;
        public SettingWindow(MainWindow main)
        {
            this.main = main;
            InitializeComponent();


            this.lineColor.DefaultColor = D_Brush2M_Color(main.setting.pen.Brush);
            this.target.Value = main.setting.Target;
            this.lineWidth.Value = main.setting.pen.Width;
            this.clip.LowerValue = main.setting.Skip;
            this.clip.UpperValue = main.setting.Take;
            //this.visualType.ItemsSource = new string[] { "时域", "频域" };
            this.visualType.SelectedIndex = main.setting.visualType == "时域" ? 0 : 1;
            this.isLine.IsOn = main.setting.isLine;
            this.amplitude.Value = main.setting.amplitude;
            this.smooth.IsOn = main.setting.smooth;
            this.margin.Text = main.setting.margin.ToString();
            this.target.ValueChanged += target_ValueChanged;
            this.lineWidth.ValueChanged += lineWidth_ValueChanged;
            this.lineColor.SelectedColorChanged += LineColor_SelectedColorChanged;
            this.clip.LowerValueChanged += Clip_LowerValueChanged;
            this.clip.UpperValueChanged += Clip_UpperValueChanged;
            this.visualType.SelectionChanged += VisualType_SelectionChanged;
            this.isLine.Toggled += IsLine_Toggled;
            this.amplitude.ValueChanged += Amplitude_ValueChanged;
            this.smooth.Toggled += Smooth_Toggled;
            this.margin.TextChanged += Margin_TextChanged;
        }

        private void Margin_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                int[] m = this.margin.Text.Trim().Split(",").Select(a => Convert.ToInt32(a)).ToArray();
                main.setting.margin = new Thickness(m[0], m[1], m[2], m[3]);
                Properties.Settings.Default.margin = this.margin.Text.Trim();
                Properties.Settings.Default.Save();
            }
            catch (Exception)
            {
            }


        }


        private void Smooth_Toggled(object sender, RoutedEventArgs e)
        {
            main.setting.smooth = this.smooth.IsOn;
            Properties.Settings.Default.smooth = this.smooth.IsOn;
            Properties.Settings.Default.Save();
        }

        private void LineColor_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            var a = this.lineColor.SelectedColor.Value;
            main.setting.pen = new Pen(M_Color2D_Brush(a), main.setting.pen.Width);
            Properties.Settings.Default.LineColor = $"{a.A},{a.R},{a.G},{a.B}";
            Properties.Settings.Default.Save();
        }

        private void Amplitude_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            main.setting.amplitude = (int)this.amplitude.Value;
            Properties.Settings.Default.amplitude = main.setting.amplitude;
            Properties.Settings.Default.Save();
        }

        private void IsLine_Toggled(object sender, RoutedEventArgs e)
        {
            main.setting.isLine = this.isLine.IsOn;
            Properties.Settings.Default.isLine = this.isLine.IsOn;
            Properties.Settings.Default.Save();
        }



        private void VisualType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            main.setting.visualType = this.visualType.SelectedValue.ToString().Replace("System.Windows.Controls.ComboBoxItem: ", "");
            Properties.Settings.Default.visualType = (string)main.setting.visualType;
            Properties.Settings.Default.Save();
        }

        private void Clip_UpperValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            main.setting.Take = (int)this.clip.UpperValue - (int)this.clip.LowerValue;
            Properties.Settings.Default.Take = main.setting.Take;
            Properties.Settings.Default.Save();
        }

        private void Clip_LowerValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            main.setting.Skip = (int)this.clip.LowerValue;
            Properties.Settings.Default.Skip = main.setting.Skip;
            Properties.Settings.Default.Save();
        }

        private void target_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            main.setting.Target = (int)this.target.Value;
            Properties.Settings.Default.Target = (int)this.target.Value;
            Properties.Settings.Default.Save();
        }

        private void lineWidth_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            main.setting.pen = new Pen(main.setting.pen.Brush, (float)this.lineWidth.Value);
            Properties.Settings.Default.LineWidth = (int)this.lineWidth.Value;
            Properties.Settings.Default.Save();
        }
        private System.Windows.Media.Brush D_Brush2M_Brush(Brush b)
        {
            Color m_Color = ((SolidBrush)b).Color;
            return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(m_Color.A, m_Color.R, m_Color.G, m_Color.B));
        }
        private System.Windows.Media.Color D_Brush2M_Color(Brush b)
        {
            Color m_Color = ((SolidBrush)b).Color;
            return System.Windows.Media.Color.FromArgb(m_Color.A, m_Color.R, m_Color.G, m_Color.B);
        }
        private Brush M_Color2D_Brush(System.Windows.Media.Color c)
        {
            return new SolidBrush(Color.FromArgb(c.A, c.R, c.G, c.B));
        }
    }
}
