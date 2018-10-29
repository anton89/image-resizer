using ImageResizer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer.Model
{
    class ItemThumbnail : ObservableObject
    {
        public ItemThumbnail(string fullName, string name, string extension, Dimension dimension, string size)
        {
            _name = name;
            FullName = fullName;
            _dimension = dimension;
            _extension = extension;
            _size = size;

            if (dimension != null)
            {
                _icon = "/Assets/icons8-picture-80.png";
            } else
            {
                _icon = "/Assets/icons8-file-96.png";
            }
        }
        public string FullName { get; set; }

        private string _icon;

        public string Icon
        {
            get { return _icon; }
            set
            {
                if (_icon != value)
                {
                    _icon = value;
                    NotifyPropertyChanged("Icon");
                }
            }
        }

        private string _size;

        public string Size
        {
            get { return _size; }
            set
            {
                if (_size != value)
                {
                    _size = value;
                    NotifyPropertyChanged("Size");
                }
            }
        }

        private string _name;

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    NotifyPropertyChanged("Name");
                }
            }
        }

        private string _extension;

        public string Extension
        {
            get { return _extension; }
            set
            {
                if (_extension != value)
                {
                    _extension = value;
                    NotifyPropertyChanged("Extension");
                }
            }
        }

        private Dimension _dimension;

        public Dimension Dimension
        {
            get { return _dimension; }
            set
            {
                if (_dimension != value)
                {
                    _dimension = value;
                    NotifyPropertyChanged("Dimension");
                }
            }
        }



    }
}
