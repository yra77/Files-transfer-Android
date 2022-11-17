
using Files_in_Locall_network.Delegates;


namespace Files_in_Locall_network.Helpers
{
    internal class CheckingEthernet
    {

        public static event Server_TextError_CallBack TextErrorEvent;

        public async static Task IsConnections()
        {
            NetworkAccess accessType = Connectivity.Current.NetworkAccess;
            IEnumerable<ConnectionProfile> profiles = Connectivity.Current.ConnectionProfiles;

            do
            {
                TextErrorEvent("Turn on WiFi", true);
                await Task.Delay(1000);

            } while (!profiles.Contains(ConnectionProfile.WiFi));

            if (accessType != NetworkAccess.Internet)
            {
               await IsConnections();
            }

            TextErrorEvent(" ", true);

        }
    }
}
