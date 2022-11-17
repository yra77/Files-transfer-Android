
using System.Net;
using System.Net.NetworkInformation;


namespace Files_in_Locall_network.Helpers
{
    internal static class GetIP
    {

        public static IPAddress MyIP;

        static GetIP()
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            try
            {
                MyIP = properties.GetUnicastAddresses()[6].Address;
            }
            catch(Exception)
            {
                MyIP = properties.GetUnicastAddresses()[4].Address;
            }
        }
    }
}
