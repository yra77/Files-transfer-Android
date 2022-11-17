

using Files_in_Locall_network.Services.Interfaces;


namespace Files_in_Locall_network.Platforms.Android.Services
{
    internal class FolderPicker : IFolderPicker
    {
        public async Task<List<string>> PickFolder()
        {
            try
            {

                var result = await FilePicker.Default.PickMultipleAsync();
                List<string> pathList = new List<string>();

                if (result != null)
                {
                    foreach (var item in result)
                    {
                        pathList.Add(item.FullPath);
                    }
                }
                //if (result != null)
                //{
                //    if (result.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase) ||
                //        result.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                //    {
                //        using var stream = await result.OpenReadAsync();
                //        var image = ImageSource.FromStream(() => stream);
                //    }
                //}

                return pathList;
            }
            catch (Exception)
            {
                
            }

            return null;
        }
    }
}
