
using Files_in_Locall_network.Services.Client;
using Files_in_Locall_network.Services.Interfaces;
using Files_in_Locall_network.Services.Server;
using Files_in_Locall_network.Views;


namespace Files_in_Locall_network;

internal static class PrismStartup
{
    public static void Configure(PrismAppBuilder builder)
    {
        builder.RegisterTypes(RegisterTypes)
                .OnAppStart("NavigationPage/MainPage");
    }

    private static void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<MainPage>()
                     .RegisterInstance(SemanticScreenReader.Default)
                     .RegisterSingleton<IFolderPicker, Platforms.Android.Services.FolderPicker>()
                     .RegisterSingleton<IMediaService, Platforms.Android.Services.MediaService>()
                     .RegisterSingleton<IClient_Service, Client_Service>()
                     .RegisterSingleton<IServer_Service, Server_Service>();

    }
}
