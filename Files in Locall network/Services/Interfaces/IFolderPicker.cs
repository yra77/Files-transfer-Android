

namespace Files_in_Locall_network.Services.Interfaces
{
    public interface IFolderPicker
    {
        Task<List<string>> PickFolder();
    }
}
