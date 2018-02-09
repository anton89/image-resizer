using ImageResizer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageResizer.Views
{
    /// <summary>
    /// Interaction logic for PresetWindow.xaml
    /// </summary>
    public partial class PresetWindow : Window
    {
        public bool IsAdd { get; set; }
        public Setting Setting { get; set; }

        public PresetWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var preset = DataContext as Preset;
            if (string.IsNullOrEmpty(preset.Name) || (IsAdd && Setting.Presets.Any(o => o.Name == preset.Name)))
            {
                MessageBox.Show("name is unique and required");
            }
            else
            {
                DialogResult = true;
            }
        }
    }
}
