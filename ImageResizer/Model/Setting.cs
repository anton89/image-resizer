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
        }

        [XmlIgnore]
        public string SettingLocation { get; set; }

        public PresetCollection Presets { get; set; }

        public string LastPath { get; set; }

        public int SplitterWidth { get; set; }
    }
}
