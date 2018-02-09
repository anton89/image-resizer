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
        public ItemThumbnail(string fullName, string name, Dimension dimension)
        {
            _name = name;
            FullName = fullName;
            _dimension = dimension;
        }
        public string FullName { get; set; }

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
