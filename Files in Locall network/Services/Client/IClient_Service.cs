

using Files_in_Locall_network.Delegates;


namespace Files_in_Locall_network.Services.Client
{
    public interface IClient_Service
    {

        public event Action event_EndCheckDevice;
        public event ProgressBardelegate progressBarEvent;
        public event ProgressChangeDelegate progressChangeEvent;

        public void SendFile_Async(string clientIP, List<string> filePath);
        public void Check_Devices(object obj);
    }
}
