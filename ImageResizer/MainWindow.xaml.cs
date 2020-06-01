using ImageResizer.Helper;
using ImageResizer.Model;
using ImageResizer.Views;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
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
using Task = ImageResizer.Model.Task;

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
        TreeItemCollection TreeListItemsSource { get; set; }
        TaskCollection Tasks { get; set; }

        public MainWindow()
        {
            Tasks = new TaskCollection();
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
            TreeListItemsSource = new TreeItemCollection() { item };

            InitializeComponent();

            List<string> path = new List<string>();

            splitterWidth.Width = new GridLength(Setting.SplitterWidth);

            TreelistInput.ItemsSource = TreeListItemsSource;
            //TreelistInput.ex

            if (Setting.LastPath != null)
            {
                var directory = new DirectoryInfo(Setting.LastPath);
                _level = new List<string>();
                _level.Add(directory.Name);
                while (directory.Parent != null)
                {
                    _level.Add(directory.Parent.Name);
                    directory = directory.Parent;
                }
            }

            PresetButton.ItemsSource = Setting.Presets;
            taskControl.Tasks = Tasks;
            Tasks.CollectionChanged += taskControl.Tasks_CollectionChanged;
            taskControl.listBox.ItemsSource = Tasks;
        }

        private void OnPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            TreeViewItem treeViewItem = VisualUpwardSearch(e.OriginalSource as DependencyObject);

            if (treeViewItem != null)
            {
                treeViewItem.Focus();
            }
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

            var thumbs = new List<ItemThumbnail>();

            foreach (var file in directory.GetFiles())
            {
                var item = CreateItemThumbnail(file);
                thumbs.Add(item);
            }

            thumbControl.thumbnailView.ItemsSource = thumbControl.ItemThumbnails = new ItemThumbnailCollection(SortItemThumbnail(thumbs));
            UriTextbox.Text = obj.FullPath;
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
                    if (selected != null) selected.IsExpanded = true;
                    _level.RemoveAt(_level.Count - 1);

                    if (_level.Count == 0 && selected != null)
                        selected.IsSelected = true;
                }
            }
        }

        private static void GetDirectories(TreeItem item)
        {
            item.Children.Clear();
            var items = new DirectoryInfo(item.FullPath).GetDirectories();
            Array.Sort(items, new DirectoryComparer());
            foreach (var directory in items)
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
            var selected = thumbControl.thumbnailView.SelectedItem as ItemThumbnail;

            //var ratio = selected.Dimension.Width / selected.Dimension.Height;

            var window = new PresetWindow() { Owner = this, IsAdd = true, Setting = Setting, ShowInTaskbar = false };
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
            ExecutePreset(preset);
        }

        private void ExecutePreset(Preset preset)
        {
            if (thumbControl.thumbnailView.SelectedItems != null && thumbControl.thumbnailView.SelectedItems.Count > 0)
            {
                ProcessItems(preset, thumbControl.thumbnailView.SelectedItems.Cast<ItemThumbnail>());
            }
            else if (thumbControl.thumbnailView.Items.Count > 0 && preset.SelectAllIfNothingSelected)
            {
                ProcessItems(preset, (IEnumerable<ItemThumbnail>)thumbControl.thumbnailView.Items.SourceCollection);
            }
            else
            {
                MessageBox.Show("you need to select item to convert");
            }
        }

        private void ProcessItems(Preset preset, IEnumerable<ItemThumbnail> selecteds)
        {
            if (preset.ProcessImmediately) 
            {
                Service.ResizeItem(selecteds, preset);

                RefreshThumbnail();

            }
            else
            {
                foreach (ItemThumbnail selected in selecteds)
                {
                    if (!Tasks.Any(o => o.TaskName.Equals(selected.FullName)))
                    {
                        var task = new Task(selected.FullName, selected, preset);
                        Tasks.Add(task);
                    }
                    else
                    {
                        MessageBox.Show("item already in task", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void RefreshThumbnail()
        {
            TreeItem treeItem = (TreeItem)TreelistInput.SelectedItem;
            var directory = new DirectoryInfo(treeItem.FullPath);

            var thumbs = new List<ItemThumbnail>();

            foreach (var file in directory.GetFiles())
            {
                var item = CreateItemThumbnail(file);
                thumbs.Add(item);
            }

            thumbControl.thumbnailView.ItemsSource = thumbControl.ItemThumbnails = new ItemThumbnailCollection(SortItemThumbnail(thumbs));
        }

        private ItemThumbnail CreateItemThumbnail(FileInfo file)
        {
            ItemThumbnail item;
            if (Setting.ImageExtensions.Contains(file.Extension))
            {
                var dimension = ImageDimension.GetDimensions(file.FullName);
                item = new ItemThumbnail(file.FullName, file.Name, file.Extension, dimension, (file.Length / 1000).ToString());
            }
            else
            {
                item = new ItemThumbnail(file.FullName, file.Name, file.Extension, null, (file.Length / 1000).ToString());
            }

            return item;
        }

        private IEnumerable<ItemThumbnail> SortItemThumbnail(List<ItemThumbnail> thumbs)
        {
            switch (((ContentControl)sortBy.SelectedItem).Content)
            {
                case "File Name":
                    return thumbs.OrderBy(o => o, new ItemThumbnailNameComparer());

                case "File Name Desc":
                    return thumbs.OrderBy(o => o, new ItemThumbnailNameDescComparer());

                case "Dimension":
                    return thumbs.OrderBy(o => o, new ItemThumbnailDimensionComparer());

                case "Dimension Desc":
                    return thumbs.OrderBy(o => o, new ItemThumbnailDimensionComparer());

                case "Type":
                    return thumbs.OrderBy(o => o, new ItemThumbnailTypeComparer());

                case "Type Desc":
                    return thumbs.OrderBy(o => o, new ItemThumbnailDimensionComparer());

                default:
                    throw new Exception("unkown sorting");
            }
        }

        private void EditPreset(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            var context = button.DataContext as Preset;

            var window = new PresetWindow() { Owner = this, IsAdd = false, Setting = Setting, ShowInTaskbar = false };
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
                update.SelectAllIfNothingSelected = preset.SelectAllIfNothingSelected;
                update.IsUseHotKey = preset.IsUseHotKey;
                update.Modifier = preset.Modifier;
                update.Key = preset.Key;
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
            copy.IsUseHotKey = preset.IsUseHotKey;
            copy.Modifier = preset.Modifier;
            copy.Key = preset.Key;

            return copy;
        }

        private void ButtonSetting_Click(object sender, RoutedEventArgs e)
        {
            var setting = new SettingWindow() { Owner = this, ShowInTaskbar = false, WindowStartupLocation = WindowStartupLocation.CenterOwner };
            setting.ShowDialog();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if (thumbControl.thumbnailView.SelectedItems != null && thumbControl.thumbnailView.SelectedItems.Count > 0)
            {
                if (MessageBox.Show("Are sure you want to delete this item", "Delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    foreach (ItemThumbnail selected in thumbControl.thumbnailView.SelectedItems)
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

        private void TreeOpenContainingFolderClick(object sender, RoutedEventArgs e)
        {
            var button = sender as MenuItem;
            var context = button.DataContext as TreeItem;

            Process.Start(context.FullPath);
        }

        private void TreeDeleteClick(object sender, RoutedEventArgs e)
        {
            var button = (MenuItem)sender;

            var context = (TreeItem)button.DataContext;
            DeleteFolder(context.FullPath);
        }

        static TreeViewItem VisualUpwardSearch(DependencyObject source)
        {
            while (source != null && !(source is TreeViewItem))
                source = VisualTreeHelper.GetParent(source);

            return source as TreeViewItem;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.RemovedItems.Count == 0) return;

            RefreshThumbnail();
        }

        private void ButtonTask_Click(object sender, RoutedEventArgs e)
        {
            if (taskControl.Visibility == Visibility.Collapsed)
                taskControl.Visibility = Visibility.Visible;
            else
                taskControl.Visibility = Visibility.Collapsed;
        }

        private void UriTextbox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var directory = new DirectoryInfo(UriTextbox.Text);
                var lv = new List<string>();
                lv.Add(directory.FullName);
                while (directory.Parent != null)
                {
                    lv.Add(directory.Parent.FullName);
                    directory = directory.Parent;
                }

                var items = TreeListItemsSource.First().Children;

                for (int i = lv.Count - 1; i >= 0; i--)
                {
                    directory = new DirectoryInfo(lv[i]);
                    if (directory.Exists)
                    {
                        var selected = items.FirstOrDefault(o => o.FullPath == directory.FullName);
                        if (selected == null)
                        {
                            if (directory.Parent == null)
                                TreeListItemsSource.Add(new TreeItem(IconType.Drive, directory.FullName, directory.Name));
                            else
                                TreeListItemsSource.Add(new TreeItem(IconType.Folder, directory.FullName, directory.Name));

                            selected = TreeListItemsSource.FirstOrDefault(o => o.FullPath == directory.FullName);


                        }
                        else
                        {
                            selected = items.FirstOrDefault(o => o.FullPath == directory.FullName);
                        }

                        GetDirectories(selected);
                        items = selected.Children;
                        selected.IsExpanded = true;
                        if (i == 0)
                        {
                            selected.IsSelected = true;
                        }
                    }
                }
            }
        }

        private TreeItem RePopulateChildern(TreeItem parent)
        {
            var directories = new DirectoryInfo(parent.FullPath).GetDirectories();
            parent.Children.Clear();

            Array.Sort(directories, new DirectoryComparer());
            foreach (var directory in directories)
            {
                parent.Children.Add(new TreeItem(IconType.Folder, directory.FullName, directory.Name));
            }

            return parent;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            foreach (var item in Setting.FilteredPresets)
            {
                Key? modifier = null;
                switch (item.Modifier)
                {
                    case "Ctrl":
                        modifier = Key.LeftCtrl;
                        break;
                    case "Alt":
                        modifier = Key.LeftAlt;
                        break;
                    case "Shift":
                        modifier = Key.LeftShift;
                        break;
                }

                string key = e.Key.ToString();
                if (key[0] == 'D')
                {
                    int num = 0;
                    if (int.TryParse(key[1].ToString(), out num))
                    {
                        key = num.ToString();
                    }
                }

                if (modifier != null && Keyboard.IsKeyDown(modifier.Value) && item.Key == key)
                {
                    ExecutePreset(item);
                }
            }

        }

        private void TreelistInput_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                if (e.Key == Key.X)
                {
                    var list = new ClipboardModel() { MyProperty = TreelistInput.SelectedItem.ToString() };
                    //list.Add((TreeItem)TreelistInput.SelectedItem);
                    Clipboard.SetData("imageViewer", list);
                }
                else if (e.Key == Key.V)
                {
                    if (Clipboard.ContainsData("imageViewer"))
                    {
                        var list = (ClipboardModel)Clipboard.GetData("imageViewer");

                        if (list != null)
                        {
                            var target = new DirectoryInfo(TreelistInput.SelectedItem.ToString());
                            var source = new DirectoryInfo(list.ToString());

                            if (target.Exists && source.Exists && target.FullName != source.Parent.FullName)
                            {
                                var level = new List<string>();
                                level.Add(source.Name);
                                while (source.Parent != null)
                                {
                                    level.Add(source.Parent.Name);
                                    source = source.Parent;
                                }
                                TreeItem temp = TreeListItemsSource[0];
                                TreeItem parent = null;
                                TreeItem delete = null;
                                for (int i = level.Count - 1; i >= 0; i--)
                                {
                                    var exist = temp.Children.FirstOrDefault(o => o.Path == level[i]);
                                    if (exist != null) temp = exist;
                                    else break;

                                    if (i == 1) parent = exist;
                                    if (i == 0) delete = exist;
                                }

                                if (parent != null && delete != null)
                                {
                                    parent.Children.Remove(delete);
                                }

                                var move = new DirectoryInfo(list.ToString());
                                move.MoveTo(System.IO.Path.Combine(target.FullName, move.Name));
                                Clipboard.Clear();
                            }
                        }
                    }
                }
            }

            if (e.Key == Key.Delete)
            {
                DeleteFolder(TreelistInput.SelectedItem.ToString());
            }
        }

        private void DeleteFolder(string path)
        {
            var directory = new DirectoryInfo(path);
            var list = new List<string>();

            while (directory.Parent != null)
            {
                list.Add(directory.Parent.Name);
                directory = directory.Parent;
            }

            var items = TreeListItemsSource.First().Children;
            TreeItem selected = null;

            for (int i = list.Count - 1; i >= 0; i--)
            {
                selected = items.FirstOrDefault(o => o.Path == list[i]);
                items = selected.Children;
            }

            if (MessageBox.Show(string.Format("Are sure you want to delete this folder: {0}", path), "Delete", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                FileOperationAPIWrapper.Send(path);

                var deleted = selected.Children.FirstOrDefault(o => o.FullPath == path);
                // make sure to jump to prev or next if deleted is selected to prevent selected go back to root
                if (deleted.IsSelected && selected.Children.Count > 1)
                {
                    var i = selected.Children.IndexOf(deleted);
                    if (i == selected.Children.Count - 1)
                        i--;
                    else if (i >= 0)
                        i++;
                    var nextSelected = selected.Children[i];
                    nextSelected.IsSelected = true;
                }

                selected.Children.Remove(deleted);
            }
        }
    }
}
