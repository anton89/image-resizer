using ImageResizer.Helper;
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

        internal ItemThumbnailCollection ItemThumbnails { get; set; }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var border = sender as Border;
                var context = border.DataContext as ItemThumbnail;

                var window = new ImageWindow() { Owner = App.Current.MainWindow, ShowInTaskbar = false, WindowStartupLocation = WindowStartupLocation.CenterOwner };
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

        private void thumbnailView_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            //if (Keyboard.IsKeyDown(Key.LeftCtrl) && e.Key == Key.C)
            //{
            //}

            if (e.Key == Key.Delete)
            {
                for (int i = thumbnailView.SelectedItems.Count - 1; i >= 0; i--)
                {
                    var selected = (ItemThumbnail)thumbnailView.SelectedItems[i];
                    FileOperationAPIWrapper.Send(selected.FullName);
                    ItemThumbnails.Remove(selected);
                }
            }
        }
    }
}
