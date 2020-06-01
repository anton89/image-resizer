using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ImageResizer.Model
{
    [Serializable()]
    public class Setting
    {
        public Setting()
        {
            SettingLocation = System.Reflection.Assembly.GetEntryAssembly().Location + "setting.xml";
            Presets = new PresetCollection();
            Presets.CollectionChanged += Presets_CollectionChanged;
            FilteredPresets = Enumerable.Empty<Preset>();
            ImageExtensions = new List<string>() { ".jpg", ".jpeg", ".png" };
        }

        private void Presets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            FilteredPresets = Presets.Where(o => o.IsUseHotKey && !string.IsNullOrEmpty(o.Modifier) && !string.IsNullOrEmpty(o.Key));
        }

        [XmlIgnore]
        public string SettingLocation { get; set; }
        [XmlIgnore]
        public IEnumerable<Preset> FilteredPresets { get; set; }

        public List<string> ImageExtensions { get; set; }

        public PresetCollection Presets { get; set; }
        

        public string LastPath { get; set; }

        public int SplitterWidth { get; set; }
    }
}
