using ImageResizer.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer.Model
{
    public class PresetCollection : ObservableObject, IList<Preset>, INotifyCollectionChanged
    {
        private readonly List<Preset> _collection;
        private Preset _selectedItem;

        public PresetCollection()
        {
            _collection = new List<Preset>();
        }

        public PresetCollection(IEnumerable<Preset> folders)
        {
            _collection = new List<Preset>(folders);
        }

        public event EventHandler SelectedItemChanged;

        public Preset SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    NotifyPropertyChanged("SelectedItem");
                }
            }
        }

        private void OnPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedItem")
                OnSelectedItemChanged();
        }

        protected void OnSelectedItemChanged()
        {
            SelectedItemChanged?.Invoke(this, new EventArgs());
        }

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion INotifyCollectionChanged Members

        #region IList Implementation

        public int Count
        {
            get { return _collection.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public Preset this[int index]
        {
            get { return _collection[index]; }
            set { _collection[index] = value; }
        }

        public void Add(Preset item)
        {
            _collection.Add(item);

            CollectionChanged?.Invoke(item, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
        }

        public void Clear()
        {
            _collection.Clear();

            CollectionChanged?.Invoke(_collection, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Contains(Preset item)
        {
            return _collection.Contains(item);
        }

        public void CopyTo(Preset[] array, int arrayIndex)
        {
            _collection.CopyTo(array, arrayIndex);
        }

        public IEnumerator<Preset> GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _collection.GetEnumerator();
        }

        public int IndexOf(Preset item)
        {
            return _collection.IndexOf(item);
        }

        public void Insert(int index, Preset item)
        {
            _collection.Insert(index, item);

            CollectionChanged?.Invoke(_collection, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace));
        }

        public void Reset(PresetCollection folders)
        {
            _collection.Clear();
            _collection.AddRange(folders);

            CollectionChanged?.Invoke(_collection, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public bool Remove(Preset item)
        {
            int index = _collection.IndexOf(item);
            bool result = _collection.Remove(item);

            CollectionChanged?.Invoke(item, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
            return result;
        }

        public void RemoveAt(int index)
        {
            _collection.RemoveAt(index);

            CollectionChanged?.Invoke(index, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove));
        }

        #endregion IList Implementation
    }
}
