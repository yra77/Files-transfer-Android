
using Plugin.CurrentActivity;

using Files_in_Locall_network.Services.Interfaces;

using Android.Content;
using andrNET = Android.Net;
using andrOS = Android.OS;


namespace Files_in_Locall_network.Platforms.Android.Services
{
    internal class MediaService:IMediaService
    {
        Context CurrentContext => CrossCurrentActivity.Current.Activity;

        public void SaveImageFromByte(byte[] imageByte, string fileName)
        {
            try
            {
                string str = fileName.Split('.').Last<string>();
                Java.IO.File storagePath = null;

                if (str == "png" || str == "jpg" || str == "jpeg" || str == "mp4")
                {
                    storagePath = andrOS.Environment.GetExternalStoragePublicDirectory(andrOS.Environment.DirectoryPictures);
                }
                else
                {
                    storagePath = andrOS.Environment.GetExternalStoragePublicDirectory(andrOS.Environment.DirectoryDownloads);
                }
                string path = System.IO.Path.Combine(storagePath.ToString(), fileName);
                System.IO.File.WriteAllBytes(path, imageByte);
                var mediaScanIntent = new Intent(Intent.ActionMediaScannerStarted);// ActionMediaScannerScanFile);
                mediaScanIntent.SetData(andrNET.Uri.FromFile(new Java.IO.File(path)));
                CurrentContext.SendBroadcast(mediaScanIntent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error android saving file + " + e);
            }
        }
    }
}
