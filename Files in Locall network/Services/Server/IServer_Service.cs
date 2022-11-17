
using Files_in_Locall_network.Services.Interfaces;
using Files_in_Locall_network.Delegates;


namespace Files_in_Locall_network.Services.Server
{
    public interface IServer_Service
    {

        public event Server_TextError_CallBack textErrorEvent;
        public event Server_Text_CallBack server_Text_Event;
        public event ProgressBardelegate progressBarEvent;
        public event ProgressChangeDelegate progressChangeEvent;

        public void ReceiveFile_Async(IMediaService mediaService);
        public void Server1234_Async();
    }
}
