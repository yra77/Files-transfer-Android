

using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;


namespace Files_in_Locall_network.Services
{
    public struct InfoPacket
    {
        public long File_Size;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 15)]
        public string Ip;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 100)]
        public string File_Name;
    }

    internal class Remote
    {

        public static async void SendMessage_Http()
        {
            try
            {
                HttpClient client = new HttpClient();
                using HttpResponseMessage response = await client.GetAsync("http://100.85.182.73:12330");
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                // Above three lines can be replaced with new helper method below
                // string responseBody = await client.GetStringAsync(uri);

                Console.WriteLine(responseBody);
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine("\nException Caught!");
                Console.WriteLine("Message :{0} ", e.Message);
            }
        }


        public static async void SendFile_Async()
        {
            var t = Task.Run(async () =>
            {
                await Task.Delay(20000);
                TcpClient client = new TcpClient();

                try
                {
                    await client.ConnectAsync(IPAddress.Parse("192.168.0.107"), 21);//"100.85.182.73"), 21);// "192.168.0.107"), 21);
                    NetworkStream stream;
                    stream = client.GetStream();
                    string s = "PASV";
                    byte[] message = Encoding.ASCII.GetBytes(s);
                    stream.Write(message, 0, message.Length);
                    var array = new byte[1024];
                    //Span<byte> data = new Span<byte>(array);
                    
                        stream.Read(array);
                        Console.WriteLine(Encoding.ASCII.GetString(array));
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("Send message ERROR - " + e.Message);
                    client.Close();
                }

                client.Close();
            });
        }



        public static void UDPsendFile(string filePath)
        {
            //IPAddress ip = GetIP.MyIP;
            //string thisDeviceName = DeviceInfo.Name;

            //my ip
           // string pubIp = await new HttpClient().GetStringAsync("https://api.ipify.org");
      
            //ip and port remote server
            UdpClient udpSender = new UdpClient();
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse("100.85.182.73"), 12333);
            //my ip and port for receive message
            UdpClient udpReceiver = new UdpClient(1237);
            IPEndPoint ipEndPointReceiver = new IPEndPoint(IPAddress.Parse("10.136.88.57"), 1237);
          
            using (FileStream fs = File.OpenRead(filePath))
            {

                InfoPacket myStruct = new InfoPacket();

                myStruct.File_Name = filePath.Split('/').Last<string>();
                myStruct.File_Size = (int)fs.Length;
                myStruct.Ip = "10.136.88.57";

                byte[] lenBytes = ToByteArray(myStruct);

                udpSender.Send(lenBytes, lenBytes.Length, ipEndPoint);

                long loadingByte = 0;
                long len = (int)fs.Length;

                byte[] buffer = new byte[1024];
                int bytesRead;
                fs.Position = 0;

                while ((bytesRead = fs.Read(buffer, 0, 1024)) > 0)
                {
                    udpSender.Send(buffer, bytesRead, ipEndPoint);

                    byte[] buf = udpReceiver.Receive(ref ipEndPointReceiver);
                    string serverResult = Encoding.UTF8.GetString(buf);
                    Console.WriteLine(serverResult);
                    if (serverResult == "ok")
                    {
                        //udpSender.Send(buffer, bytesRead, ipEndPoint);
                        //byte[] buf2 = udpReceiver.Receive(ref ipEndPointReceiver);
                        //string serverResult2 = Encoding.UTF8.GetString(buf2);
                        //if (serverResult != "ok")
                        //{
                        //    Console.WriteLine("ERRor sending");
                        //    return;
                        //}
                    }

                    loadingByte += bytesRead;
                }
            }
        }

        private static byte[] ToByteArray(InfoPacket info_File)
        {
            byte[] arr = null;
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(info_File);
                arr = new byte[size];
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(info_File, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error struct ToByteArray - " + e.Message);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }

            return arr;
        }

        //public static async void SendMessage()
        //{

        //    var t = Task.Run(() =>
        //    {
        //        SendUPD();
        //    });

        //    IPAddress ip = GetIP.MyIP;
        //    string thisDeviceName = DeviceInfo.Name;

        //    TcpClient client = new TcpClient();

        //    try
        //    {
        //        // client.SendTimeout = 150000;

        //        await client.ConnectAsync(IPAddress.Parse("100.85.182.73"), 12330);
        //        NetworkStream stream;
        //        stream = client.GetStream();
        //        string s = "QQQQQQQQQQQQ " + ip + ";" + thisDeviceName + ";";
        //        byte[] message = Encoding.ASCII.GetBytes(s);
        //        stream.Write(message, 0, message.Length);
        //    }
        //    catch (Exception e)
        //    {
        //        Console.WriteLine("Send message ERROR - " + e.Message);
        //        client.Close();
        //        SendMessage();
        //    }

        //    client.Close();
        //    //SendMessage();
        //}
    }
}
