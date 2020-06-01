using ImageResizer.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer.Helper
{
    public static class DLL
    {
        [System.Security.SuppressUnmanagedCodeSecurity]
        [DllImport("Shlwapi.dll ", CharSet = CharSet.Unicode)]
        public static extern int StrCmpLogicalW(string psz1, string psz2);

        public static DateTime Start = new DateTime(1900, 1, 1);
    }

    class NumericComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return DLL.StrCmpLogicalW(x, y);
        }
    }

    class DirectoryComparer : IComparer<DirectoryInfo>
    {
        public int Compare(DirectoryInfo x, DirectoryInfo y)
        {
            return DLL.StrCmpLogicalW(x.Name, y.Name);
        }
    }

    class ItemThumbnailNameComparer : IComparer<ItemThumbnail>
    {
        public int Compare(ItemThumbnail x, ItemThumbnail y)
        {
            return DLL.StrCmpLogicalW(x.Name, y.Name);
        }
    }

    class ItemThumbnailNameDescComparer : IComparer<ItemThumbnail>
    {
        public int Compare(ItemThumbnail x, ItemThumbnail y)
        {
            return DLL.StrCmpLogicalW(y.Name, x.Name);
        }
    }

    class ItemThumbnailTypeComparer : IComparer<ItemThumbnail>
    {
        public int Compare(ItemThumbnail x, ItemThumbnail y)
        {
            return DLL.StrCmpLogicalW(x.Extension, y.Extension);
        }
    }

    class ItemThumbnailTypeDescComparer : IComparer<ItemThumbnail>
    {
        public int Compare(ItemThumbnail x, ItemThumbnail y)
        {
            return DLL.StrCmpLogicalW(y.Extension, x.Extension);
        }
    }

    class ItemThumbnailDimensionComparer : IComparer<ItemThumbnail>
    {
        public int Compare(ItemThumbnail x, ItemThumbnail y)
        {
            if (x.Dimension == null && y.Dimension != null)
                return DLL.StrCmpLogicalW("0", (y.Dimension.Width * y.Dimension.Height).ToString());
            else if (x.Dimension != null && y.Dimension == null)
                return DLL.StrCmpLogicalW((x.Dimension.Width * x.Dimension.Height).ToString(), "0");
            else if (x.Dimension == null && y.Dimension == null) return DLL.StrCmpLogicalW("0", "0");

            string xs = (x.Dimension.Width * x.Dimension.Height).ToString();
            string ys = (y.Dimension.Width * y.Dimension.Height).ToString();

            return DLL.StrCmpLogicalW(xs, ys);
        }
    }

    class ItemThumbnailDimensionDescComparer : IComparer<ItemThumbnail>
    {
        public int Compare(ItemThumbnail x, ItemThumbnail y)
        {
            if (x.Dimension == null)
                return -1;

            string xs = (x.Dimension.Width * x.Dimension.Height).ToString();
            string ys = (y.Dimension.Width * y.Dimension.Height).ToString();

            return DLL.StrCmpLogicalW(ys, xs);
        }
    }
}
