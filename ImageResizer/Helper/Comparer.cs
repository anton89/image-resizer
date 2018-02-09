using System;
using System.Collections.Generic;
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
}
