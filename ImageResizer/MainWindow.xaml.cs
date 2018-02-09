using ImageResizer.Helper;
using ImageResizer.Model;
using ImageResizer.Views;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Xml.Serialization;

namespace ImageResizer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Setting Setting { get; set; }
        public bool IterateLastPath { get; set; }
        private List<string> _level { get; set; }

        public MainWindow()
        {
            Setting = new Setting();
            if (File.Exists(Setting.SettingLocation))
            {
                var xml = new XmlSerializer(typeof(Setting));
                using (var stream = new StreamReader(Setting.SettingLocation))
                {
                    Setting = (Setting)xml.Deserialize(stream);
                    stream.Close();
                }

                IterateLastPath = !string.IsNullOrEmpty(Setting.LastPath);
            }


            var item = new TreeItem(IconType.Computer, ".", "My Computer");
            item.IsExpanded = true;
            var collection = new TreeItemCollection() { item };

            InitializeComponent();

            List<string> path = new List<string>();

            splitterWidth.Width = new GridLength(Setting.SplitterWidth);

            TreelistInput.ItemsSource = collection;
            TreelistInput.SelectedItemChanged += TreelistInput_SelectedItemChanged;
            //TreelistInput.ex

            var directory = new DirectoryInfo(Setting.LastPath);
            _level = new List<string>();
            _level.Add(directory.Name);
            while (directory.Parent != null)
            {
                _level.Add(directory.Parent.Name);
                directory = directory.Parent;
            }

            PresetButton.ItemsSource = Setting.Presets;
        }

        protected override void OnClosed(EventArgs e)
        {
            Setting.SplitterWidth = int.Parse(splitterWidth.Width.Value.ToString());
            var lastItem = (TreelistInput.SelectedItem as TreeItem);
            if (lastItem != null)
                Setting.LastPath = lastItem.FullPath;

            var xml = new XmlSerializer(typeof(Setting));
            using (var stream = new StreamWriter(Setting.SettingLocation))
            {
                xml.Serialize(stream, Setting);
                stream.Close();
            }

            base.OnClosed(e);
        }

        private void TreelistInput_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            var obj = (TreeItem)e.NewValue;
            var directory = new DirectoryInfo(obj.FullPath);

            if (!directory.Exists) return;

            var thumbs = new ItemThumbnailCollection();

            foreach (var file in directory.GetFiles())
            {
                var item = new ItemThumbnail(file.FullName, file.Name, ImageDimension.GetDimensions(file.FullName));
                thumbs.Add(item);
            }
            thumbControl.ThumbnailView.ItemsSource = thumbs;
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem tvi = e.OriginalSource as TreeViewItem;
            if (tvi != null)
            {
                var item = tvi.DataContext as TreeItem;

                if (item.IconType == IconType.Computer)
                    GetLogicalDrive(item);
                else
                    GetDirectories(item);

                if (IterateLastPath && _level.Count > 0)
                {
                    var selected = item.Children.FirstOrDefault(o => o.Path == _level[_level.Count - 1]);
                    if (selected != null) selected.IsExpanded = selected.IsSelected = true;
                    _level.RemoveAt(_level.Count - 1);
                }
            }
        }

        private static void GetDirectories(TreeItem item)
        {
            item.Children.Clear();

            foreach (var directory in new DirectoryInfo(item.FullPath).GetDirectories())
            {
                item.Children.Add(new TreeItem(IconType.Folder, directory.FullName, directory.Name));
            }
        }

        private static void GetLogicalDrive(TreeItem item)
        {
            item.Children.Clear();

            foreach (var drive in Directory.GetLogicalDrives())
            {
                var driveItem = new TreeItem(IconType.Drive, drive, drive);
                driveItem.Children = new TreeItemCollection() { new TreeItem() };
                item.Children.Add(driveItem);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var selected = thumbControl.ThumbnailView.SelectedItem as ItemThumbnail;

            //var ratio = selected.Dimension.Width / selected.Dimension.Height;

            var window = new PresetWindow() { Owner = this, IsAdd = true, Setting = Setting };
            var preset = new Preset();
            window.DataContext = preset;
            if (window.ShowDialog().GetValueOrDefault(false))
            {
                var copy = Clone(preset);
                Setting.Presets.Add(copy);
            }

            //using (var image = System.Drawing.Image.FromFile(selected.FullName))
            //{
            //    using (var resized = Resizer.ResizeImage(image, 1600, 1200))
            //    {
            //        //save the resized image as a jpeg with a quality of 90
            //        Resizer.SaveJpeg(@"C:\myimage.jpeg", resized, 90);
            //    }
            //}
        }

        private void PresetClick(object sender, RoutedEventArgs e)
        {
            var preset = (sender as Button).DataContext as Preset;

            if (thumbControl.ThumbnailView.SelectedItems != null && thumbControl.ThumbnailView.SelectedItems.Count > 0)
            {

                switch (preset.ResizeMode)
                {
                    case Model.ResizeMode.InPixel:
                        break;
                    case Model.ResizeMode.Percentage:
                        break;
                    case Model.ResizeMode.BaseOnOneSide:
                        foreach (ItemThumbnail selected in thumbControl.ThumbnailView.SelectedItems)
                        {
                            float ratio = (float)selected.Dimension.Width / (float)selected.Dimension.Height;
                            float width = 0;
                            float height = 0;
                            switch ((PredefineSide)preset.PredefineSide)
                            {
                                case PredefineSide.Width:
                                    width = preset.BaseNumber;
                                    height = preset.BaseNumber / ratio;
                                    break;
                                case PredefineSide.Height:
                                    height = preset.BaseNumber;
                                    width = preset.BaseNumber * ratio;
                                    break;
                                default:
                                    throw new Exception("no predefine side selected");
                            }

                            var info = new FileInfo(selected.FullName);
                            var id = "";
                            var tempFilePath = "";

                            using (var img = System.Drawing.Image.FromFile(selected.FullName))
                            {
                                id = Guid.NewGuid().ToString("N");
                                tempFilePath = System.IO.Path.Combine(info.Directory.FullName, id);

                                var bitmap = Resizer.ResizeImage(img, (int)Convert.ToDouble(width.ToString()), (int)Convert.ToDouble(height.ToString()));
                                Resizer.SaveJpeg(tempFilePath, bitmap, preset.Quality);
                            }

                            var result = FileOperationAPIWrapper.Send(selected.FullName);

                            if (!result)
                                throw new Exception("something went wrong when tried to delete file");

                            var temp = new FileInfo(tempFilePath);

                            System.IO.File.Move(tempFilePath, System.IO.Path.ChangeExtension(selected.FullName, ".jpg"));

                        }
                        break;
                    default:
                        break;
                }

                RefreshThumbnail();
            }
            else
            {
                MessageBox.Show("you need to select item to convert");
            }
        }

        private void RefreshThumbnail()
        {
            TreeItem treeItem = (TreeItem)TreelistInput.SelectedItem;
            var directory = new DirectoryInfo(treeItem.FullPath);

            var thumbs = new ItemThumbnailCollection();

            foreach (var file in directory.GetFiles())
            {
                var item = new ItemThumbnail(file.FullName, file.Name, ImageDimension.GetDimensions(file.FullName));
                thumbs.Add(item);
            }
            thumbControl.ThumbnailView.ItemsSource = thumbs;
        }

        private void EditPreset(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            var context = button.DataContext as Preset;

            var window = new PresetWindow() { Owner = this, IsAdd = false, Setting = Setting };
            window.DataContext = Clone(context);
            if (window.ShowDialog().GetValueOrDefault(false))
            {
                var update = Setting.Presets.FirstOrDefault(o => o.Name == context.Name);
                var preset = window.DataContext as Preset;
                update.BaseNumber = preset.BaseNumber;
                update.Height = preset.Height;
                update.Name = preset.Name;
                update.Percentage = preset.Percentage;
                update.PredefineSide = preset.PredefineSide;
                update.Quality = preset.Quality;
                update.ResizeMode = preset.ResizeMode;
                update.Width = preset.Width;
            }
        }

        private Preset Clone(Preset preset)
        {
            var copy = new Preset();
            copy.BaseNumber = preset.BaseNumber;
            copy.Height = preset.Height;
            copy.Name = preset.Name;
            copy.Percentage = preset.Percentage;
            copy.PredefineSide = preset.PredefineSide;
            copy.Quality = preset.Quality;
            copy.ResizeMode = preset.ResizeMode;
            copy.Width = preset.Width;

            return copy;
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (thumbControl.ThumbnailView.SelectedItems != null && thumbControl.ThumbnailView.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("Are sure you want to delete this item", "Delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    foreach (ItemThumbnail selected in thumbControl.ThumbnailView.SelectedItems)
                    {
                        FileOperationAPIWrapper.Send(selected.FullName);
                    }

                    RefreshThumbnail();
                }
            }
            else
            {
                MessageBox.Show("must select items to delete");
            }
        }
    }
}
