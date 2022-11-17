

using System.Runtime.InteropServices;


namespace Files_in_Locall_network.Models
{
    public struct Info_File
    {
        public long File_Size;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string File_Name;
    }
}
