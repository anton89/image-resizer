using ImageResizer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageResizer.Model
{
    enum IconType
    {
        Computer,
        Drive,
        Folder,
        None
    }

    class TreeItem : ObservableObject
    {
        public TreeItem()
        {
            _iconType = IconType.None;
            _fullPath = ". . .";
            _path = ". . . ";
        }

        public TreeItem(IconType icon, string fullPath, string path)
        {
            _iconType = icon;
            _fullPath = fullPath;
            _path = path;
            _children = new TreeItemCollection();
            _children.Add(new TreeItem());
        }

        private bool _isExpanded;
        private bool _isSelected;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    NotifyPropertyChanged("IsExpanded");
                }
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");
                }
            }
        }


        private string _path;

        public string Path
        {
            get { return _path; }
            set
            {
                if (_path != value)
                {
                    _path = value;
                    NotifyPropertyChanged("Path");
                }
            }
        }

        private string _fullPath;

        public string FullPath
        {
            get { return _fullPath; }
            set
            {
                if (_fullPath != value)
                {
                    _fullPath = value;
                    NotifyPropertyChanged("FullPath");
                }
            }
        }

        private IconType _iconType;

        public IconType IconType
        {
            get { return _iconType; }
            set { _iconType = value; }
        }

        public BitmapImage Icon
        {
            get
            {
                switch (_iconType)
                {
                    case IconType.Computer:
                        return (BitmapImage)App.Current.FindResource("computerIcon");
                    case IconType.Drive:
                        return (BitmapImage)App.Current.FindResource("driveIcon");
                    case IconType.Folder:
                        return (BitmapImage)App.Current.FindResource("folderIcon");
                    default:
                        return null;
                }
            }
        }

        private TreeItemCollection _children;

        public TreeItemCollection Children
        {
            get { return _children; }
            set
            {
                if (_children != value)
                {
                    _children = value;
                    NotifyPropertyChanged("Children");
                }
            }
        }

        public override string ToString()
        {
            return _fullPath;
        }

    }
}
