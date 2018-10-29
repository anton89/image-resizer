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
using Task = ImageResizer.Model.Task;

namespace ImageResizer.Views
{
    /// <summary>
    /// Interaction logic for TaskControl.xaml
    /// </summary>
    public partial class TaskControl : UserControl
    {
        internal TaskCollection Tasks { get; set; }

        public TaskControl()
        {
            InitializeComponent();
            executeButton.IsEnabled = false;
        }

        // attached from main window
        public void Tasks_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Tasks.Count > 0)
            {
                executeButton.IsEnabled = true;
                if (VisualTreeHelper.GetChildrenCount(listBox) > 0)
                {
                    Border border = (Border)VisualTreeHelper.GetChild(listBox, 0);
                    ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                    scrollViewer.ScrollToBottom();
                }
            }
            else 
                executeButton.IsEnabled = false;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            for (int i = Tasks.Count - 1; i >= 0; i--)
            {
                var item = Tasks[i];
                Service.ResizeItem(item.ItemThumbnail, item.Preset);
                Tasks.Remove(item);
            }
        }

        private void listBox_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                for (int i = listBox.SelectedItems.Count - 1; i >= 0; i--)
                {
                    var selected = (Task)listBox.SelectedItems[i];
                    Tasks.Remove(selected);
                }
            }
        }
    }
}
