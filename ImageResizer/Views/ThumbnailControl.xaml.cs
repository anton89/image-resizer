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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImageResizer.Views
{
    /// <summary>
    /// Interaction logic for ThumbnailControl.xaml
    /// </summary>
    public partial class ThumbnailControl : UserControl
    {
        public ThumbnailControl()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var border = sender as Border;
                var context = border.DataContext as ItemThumbnail;

                var window = new ImageWindow();
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(context.FullName);

                try
                {
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                catch (Exception)
                {
                }

                window.img.Source = bitmap;

                window.ShowDialog();
            }
        }
    }
}
