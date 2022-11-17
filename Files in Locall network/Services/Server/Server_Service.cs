
using Files_in_Locall_network.Models;
using Files_in_Locall_network.Services.Interfaces;
using Files_in_Locall_network.Delegates;
using Files_in_Locall_network.Helpers;

using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;


namespace Files_in_Locall_network.Services.Server
{
    internal class Server_Service : IServer_Service
    {

        private string fileName;

        public event ProgressBardelegate progressBarEvent;
        public event ProgressChangeDelegate progressChangeEvent;
        public event Server_TextError_CallBack textErrorEvent;
        public event Server_Text_CallBack server_Text_Event;


        public async void Server1234_Async()
        {

            TcpClient server = default(TcpClient);

            IPAddress ip = GetIP.MyIP;

            TcpListener listener = new TcpListener(ip, 1234);
            listener.Start();

            while (true)
            {
                try
                {
                    server = await listener.AcceptTcpClientAsync();

                    const int bytesize = 1024 * 1024;
                    byte[] buffer = new byte[bytesize];
                    string x = server.GetStream().Read(buffer, 0, bytesize).ToString();
                    string data = ASCIIEncoding.ASCII.GetString(buffer);

                    if (data != null && data != " ")
                    {
                        server_Text_Event(data);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Message Error to server - " + e.Message);
                    break;
                }
            }

            server.Dispose();
            server.Close();
            listener.Stop();
            Console.WriteLine("Message to server ERROR");
            Server1234_Async();
        }

        public async void ReceiveFile_Async(IMediaService mediaService)
        {
            
            TcpClient server = default(TcpClient);
            TcpListener listener;

            IPAddress ip = GetIP.MyIP;

            listener = new TcpListener(ip, 1235);
            listener.Start();

            while (true)
            {

                try
                {

                    server = await listener.AcceptTcpClientAsync();

                    NetworkStream stream = server.GetStream();

                    byte[] fileSizeBytes = new byte[112];
                    int bytes = stream.Read(fileSizeBytes, 0, 112);

                    Info_File myStruct2 = new Info_File();
                    myStruct2 = FromBytes(fileSizeBytes);

                    fileName = myStruct2.File_Name;
                    long len = myStruct2.File_Size;


                    int bytesLeft = (int)myStruct2.File_Size;
                    byte[] data = new byte[myStruct2.File_Size];

                    progressBarEvent(false);
                    long loadingByte = 0;

                    int bufferSize = 1024;
                    int bytesRead = 0;

                    while (bytesLeft > 0)
                    {
                        int curDataSize = Math.Min(bufferSize, bytesLeft);

                        if (server.Available < curDataSize)
                            curDataSize = server.Available;

                        bytes = stream.Read(data, bytesRead, curDataSize);

                        bytesRead += curDataSize;
                        bytesLeft -= curDataSize;

                        loadingByte += bytes;
                        double percentage = (double)loadingByte * 100.0 / len;
                        progressChangeEvent(percentage, false);
                    }

                    progressChangeEvent(0.0, true);

                    //checking permissions file
                    PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                    if (status == PermissionStatus.Denied)
                    {
                        textErrorEvent("File not saved. You have not given permission to access the system", true);
                    }

                    mediaService.SaveImageFromByte(data, fileName);

                    textErrorEvent(Compare(fileName), false);

                    fileName = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Receive error " + ex.Message);
                }
            }
        }

        private string Compare(string filename)
        {
            string str = filename.Split('.').Last<string>();

            if (str == "png" || str == "jpg" || str == "jpeg" || str == "mp4")
            {
                return "File saved to Picture folder";
            }
            return "File saved to Download folder";
        }

        private Info_File FromBytes(byte[] arr)
        {
            Info_File str = new Info_File();

            int size = Marshal.SizeOf(str);
            GCHandle h = default(GCHandle);

            try
            {
                h = GCHandle.Alloc(arr, GCHandleType.Pinned);

                str = Marshal.PtrToStructure<Info_File>(h.AddrOfPinnedObject());

            }
            finally
            {
                if (h.IsAllocated)
                {
                    h.Free();
                }
            }
            return str;
        }
    }
}
