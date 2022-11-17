

using Files_in_Locall_network.Models;
using Files_in_Locall_network.Delegates;
using Files_in_Locall_network.Helpers;

using System.Net.Sockets;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;


namespace Files_in_Locall_network.Services.Client
{
    internal class Client_Service : IClient_Service
    {

        public event Action event_EndCheckDevice;
        public event ProgressBardelegate progressBarEvent;
        public event ProgressChangeDelegate progressChangeEvent;


        public async void SendFile_Async(string clientIP, List<string> filePathList)
        {

            List<string> tempList = filePathList;

            for (int i = 0; i < filePathList.Count; i++)
            {

                TcpClient client = new TcpClient();

                await client.ConnectAsync(clientIP, 1235);

                if (!client.Connected)
                {
                    Console.WriteLine("Client connection Error " + clientIP);
                    SendFile_Async(clientIP, tempList);
                }
                else
                {
                    using (FileStream fs = File.OpenRead(filePathList[i]))
                    {

                        Info_File myStruct = new Info_File();

                        myStruct.File_Name = filePathList[i].Split('/').Last<string>();
                        myStruct.File_Size = (int)fs.Length;

                        byte[] lenBytes = ToByteArray(myStruct);


                        client.Client.Send(lenBytes);

                        progressBarEvent(true);
                        long loadingByte = 0;
                        long len = (int)fs.Length;

                        byte[] buffer = new byte[1024];
                        int bytesRead;
                        fs.Position = 0;

                        while ((bytesRead = fs.Read(buffer, 0, 1024)) > 0)
                        {
                            client.Client.Send(buffer, bytesRead, SocketFlags.None);

                            loadingByte += bytesRead;
                            double percentage = (double)loadingByte * 100.0 / len;
                            progressChangeEvent(percentage, false);
                        }

                        progressChangeEvent(0.0, true);
                        //remove the element this
                        tempList = filePathList.Where(e => e != filePathList[i]).ToList<string>();
                    }
                }
                client.Close();

            }
        }

        public void Check_Devices(object obj)
        {

            CancellationToken token = (CancellationToken)obj;

            IPAddress ip = GetIP.MyIP;

            string thisDeviceName = DeviceInfo.Name;

            string[] temp = ip.ToString().Split('.');
            string firstStr = temp[0] + "." + temp[1] + "." + temp[2] + ".";
            int last = Convert.ToInt16(temp[3]);

            if (last < 100)
                last = 0;
            else if (last > 100 && last < 200)
                last = 100;
            else
                last = 200;

            Parallel.For(1, 20, (i, state) =>
            {
                TcpClient client = new TcpClient();

                if (ip.ToString() != firstStr + (i + last).ToString()
                    && Connect_Server(client, firstStr + (i + last).ToString()).Result)
                {
                    Send_DeviceName_Ip(client, ip.ToString(), thisDeviceName);
                }
                else
                {
                    client.Close();
                }
                if (token.IsCancellationRequested)
                {
                    // Console.WriteLine("Task is closed");
                    state.Stop();
                }
            });

            event_EndCheckDevice.Invoke();
        }

        private async Task<bool> Connect_Server(TcpClient client, string clientIP)
        {

            try
            {
                await client.ConnectAsync(clientIP, 1234);

                if (client.Connected)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception x)
            {
                Console.WriteLine("Connection failed! " + x.Message);
                return false;
            }
        }

        private void Send_DeviceName_Ip(TcpClient client, string ip, string thisDeviceName)
        {
            try
            {
                NetworkStream stream;
                stream = client.GetStream();
                string s = ip + ";" + thisDeviceName + ";";
                byte[] message = Encoding.ASCII.GetBytes(s);
                stream.Write(message, 0, message.Length);
            }
            catch (Exception e)
            {
                Console.WriteLine("Send message ERROR - " + e.Message);
            }
        }

        private byte[] ToByteArray(Info_File info_File)
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

    }
}
