using ImageResizer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer.Helper
{
    class Service
    {
        public static void ResizeItem(ItemThumbnailCollection selectedThumbs, Preset preset)
        {
            foreach (ItemThumbnail selectedThumb in selectedThumbs)
            {
                ResizeItem(selectedThumb, preset);
            }
        }

        public static void ResizeItem(ItemThumbnail selectedThumb, Preset preset)
        {
            if (selectedThumb.Dimension == null) return;
                //throw new Exception("error, fail to read dimension header");

            float width = 0;
            float height = 0;
            var info = new FileInfo(selectedThumb.FullName);
            var id = "";
            var tempFilePath = "";

            switch (preset.ResizeMode)
            {
                case Model.ResizeMode.InPixel:
                    throw new Exception();
                case Model.ResizeMode.Percentage:
                    float percentage = preset.Percentage / 100f;
                    width = selectedThumb.Dimension.Width * percentage;
                    height = selectedThumb.Dimension.Height * percentage;

                    break;
                case Model.ResizeMode.BaseOnOneSide:
                    float ratio = (float)selectedThumb.Dimension.Width / (float)selectedThumb.Dimension.Height;
                    
                    switch ((PredefineSide)preset.PredefineSide)
                    {
                        case PredefineSide.Width:
                            width = preset.BaseNumber;
                            height = preset.BaseNumber / ratio;
                            break;
                        case PredefineSide.Height:
                            height = preset.BaseNumber;
                            width = preset.BaseNumber * ratio;
                            break;
                        default:
                            throw new Exception("no predefine side selected");
                    }

                    break;
                default:
                    throw new Exception();
            }

            using (var img = System.Drawing.Image.FromFile(selectedThumb.FullName))
            {
                id = Guid.NewGuid().ToString("N");
                tempFilePath = System.IO.Path.Combine(info.Directory.FullName, id);

                var bitmap = Resizer.ResizeImage(img, (int)Convert.ToDouble(width.ToString()), (int)Convert.ToDouble(height.ToString()));
                Resizer.SaveJpeg(tempFilePath, bitmap, preset.Quality);
            }

            var result = FileOperationAPIWrapper.Send(selectedThumb.FullName);

            if (!result)
                throw new Exception("something went wrong when tried to delete file");

            var temp = new FileInfo(tempFilePath);

            System.IO.File.Move(tempFilePath, System.IO.Path.ChangeExtension(selectedThumb.FullName, ".jpg"));
        }
    }
}
