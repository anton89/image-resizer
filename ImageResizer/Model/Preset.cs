using ImageResizer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Serialization;

namespace ImageResizer.Model
{
    public enum ResizeMode
    {
        InPixel,
        Percentage,
        BaseOnOneSide
    }

    public enum PredefineSide
    {
        Width,
        Height
    }

    public class Preset : ObservableObject
    {
        public Preset()
        {
            _quality = 90;
        }
        
        private ResizeMode _resizeMode;

        public ResizeMode ResizeMode
        {
            get { return _resizeMode; }
            set
            {
                if (_resizeMode != value)
                {
                    _resizeMode = value;
                    NotifyPropertyChanged("ResizeMode");
                }
            }
        }

        private int _quality;

        public int Quality
        {
            get { return _quality; }
            set
            {
                if (_quality != value)
                {
                    _quality = value;
                    NotifyPropertyChanged("Quality");
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

        private bool _processImmediately;

        public bool ProcessImmediately
        {
            get { return _processImmediately; }
            set
            {
                if (_processImmediately != value)
                {
                    _processImmediately = value;
                    NotifyPropertyChanged("ProcessImmediately");
                }
            }
        }

        private bool _isUseHotKey;

        public bool IsUseHotKey
        {
            get { return _isUseHotKey; }
            set
            {
                if (_isUseHotKey != value)
                {
                    _isUseHotKey = value;
                    NotifyPropertyChanged("IsUseHotKey");
                }
            }
        }

        private string _key;

        public string Key
        {
            get { return _key; }
            set
            {
                if (_key != value)
                {
                    _key = value;
                    NotifyPropertyChanged("Key");
                }
            }
        }

        private string _modifier;

        public string Modifier
        {
            get { return _modifier; }
            set
            {
                if (_modifier != value)
                {
                    _modifier = value;
                    NotifyPropertyChanged("Modifier");
                }
            }
        }

        #region BaseOnOneSide

        private int _predefineSide;

        public int PredefineSide
        {
            get { return _predefineSide; }
            set
            {
                if (_predefineSide != value)
                {
                    _predefineSide = value;
                    NotifyPropertyChanged("ResizeMode");
                }
            }
        }

        private int _baseNumber;

        public int BaseNumber
        {
            get { return _baseNumber; }
            set
            {
                if (_baseNumber != value)
                {
                    _baseNumber = value;
                    NotifyPropertyChanged("BaseNumber");
                }
            }
        }

        #endregion

        #region Percentage

        private int _percentage;

        public int Percentage
        {
            get { return _percentage; }
            set
            {
                if (_percentage != value)
                {
                    _percentage = value;
                    NotifyPropertyChanged("Percentage");
                }
            }
        }

        #endregion

        #region InPixel

        private int _width;

        public int Width
        {
            get { return _width; }
            set
            {
                if (_width != value)
                {
                    _width = value;
                    NotifyPropertyChanged("Width");
                }
            }
        }
    
        private int _height;

        public int Height
        {
            get { return _height; }
            set
            {
                if (_height != value)
                {
                    _height = value;
                    NotifyPropertyChanged("Height");
                }
            }
        }

        #endregion
    }
}
