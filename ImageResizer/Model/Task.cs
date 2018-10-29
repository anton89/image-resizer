using ImageResizer.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer.Model
{
    class Task : ObservableObject
    {
        public Task(string taskName, ItemThumbnail itemThumbnail, Preset preset)
        {
            ItemThumbnail = itemThumbnail;
            Preset = preset;
            TaskName = taskName;
        }

        public ItemThumbnail ItemThumbnail { get; set; }
        public Preset Preset { get; set; }

        public string TaskName { get; set; }
    }
}
