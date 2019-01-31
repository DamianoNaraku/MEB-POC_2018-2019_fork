using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace broadcastListener
{
    public static class ExtensionClass
    {
        public static string arrayToString(this byte[] arr) { return System.Text.Encoding.UTF8.GetString(arr); }
        public static byte[] stringToByteArr(this string s) { return Encoding.ASCII.GetBytes(s); }
    }
}
