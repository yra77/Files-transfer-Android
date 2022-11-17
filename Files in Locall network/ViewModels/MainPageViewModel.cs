

using Files_in_Locall_network.Helpers;
using Files_in_Locall_network.Models;
using Files_in_Locall_network.Services;
using Files_in_Locall_network.Services.Client;
using Files_in_Locall_network.Services.Interfaces;
using Files_in_Locall_network.Services.Server;

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;


namespace Files_in_Locall_network.ViewModels
{

    public class MainPageViewModel : BindableBase, INavigatedAware
    {

        private readonly INavigationService _navigationService;
        private readonly IFolderPicker _folderPicker;
        private readonly IMediaService _mediaService;
        private readonly IServer_Service _server;
        private readonly IClient_Service _client;

        private CancellationTokenSource _cancellTokenSource;
        private CancellationToken _cancellToken;
        private IPAddress _ip;
        private string _thisDeviceName;
        private bool _isCheckingDevice;


        public MainPageViewModel(INavigationService navigationService,
                                      IFolderPicker folderPicker,
                                      IMediaService mediaService,
                                      IServer_Service server,
                                      IClient_Service client)
        {

            _navigationService = navigationService;
            _folderPicker = folderPicker;
            _mediaService = mediaService;
            _server = server;
            _client = client;

            _server.textErrorEvent += ServerTextError_Callback;
            _server.server_Text_Event += ServerText_Callback;
            _server.progressBarEvent += ProgressBarVisible;
            _server.progressChangeEvent += ProgressBarChange;

            _client.progressBarEvent += ProgressBarVisible;
            _client.progressChangeEvent += ProgressBarChange;

            CheckingEthernet.TextErrorEvent += ServerTextError_Callback;

            IsVisibleProgressBar = false;

            ProgressBar = 0.0;
            CheckDeviceBtnText = "Start";

            Device_List = new ObservableCollection<Device_Info>();

            _isCheckingDevice = false;

            _client.event_EndCheckDevice += EndCheckDevice;
        }


        #region Public property

        private ObservableCollection<Device_Info> device_List;
        public ObservableCollection<Device_Info> Device_List
        {
            get => device_List;
            set => device_List = value;
        }


        private string _textError;
        public string TextError { get => _textError; set => SetProperty(ref _textError, value); }

        private Color _textErrorColor;
        public Color TextErrorColor { get => _textErrorColor; set => SetProperty(ref _textErrorColor, value); }


        private string _deviceName_Ip;
        public string DeviceName_Ip { get => _deviceName_Ip; set => SetProperty(ref _deviceName_Ip, value); }


        private string _checkDeviceBtnText;
        public string CheckDeviceBtnText { get => _checkDeviceBtnText; set => SetProperty(ref _checkDeviceBtnText, value); }


        private string _label2;
        public string Label2 { get => _label2; set => SetProperty(ref _label2, value); }


        private double _progressBar;
        public double ProgressBar { get => _progressBar; set => SetProperty(ref _progressBar, value); }


        private bool _isVisibleProgressBar;
        public bool IsVisibleProgressBar { get => _isVisibleProgressBar; set => SetProperty(ref _isVisibleProgressBar, value); }


        public DelegateCommand CheckDeviceBtn => new DelegateCommand(CheckingDevices_Click);
        public DelegateCommand<Device_Info> ClickToItem => new DelegateCommand<Device_Info>(DeviceTapped);

        #endregion


        #region private helpers

        private async void Start()
        {
            // is disable Wi-Fi connection.
            await CheckingEthernet.IsConnections();
            await Task.Delay(1500);//задержка нужна

            _ip = GetIP.MyIP;
            _thisDeviceName = DeviceInfo.Name;

            DeviceName_Ip = $"IP - {_ip}   Name - {_thisDeviceName}";

            Task task = Task.Factory.StartNew(() =>
            {
                _server.ReceiveFile_Async(_mediaService);
            });

            Thread threadServer = new Thread(_server.Server1234_Async);
            threadServer.Start();

        }

        private void ProgressBarVisible(bool isSending)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (isSending)
                    Label2 = "Sending file";
                else
                    Label2 = "Loading file";

                IsVisibleProgressBar = true;
            });
        }

        private void ProgressBarChange(double percentage, bool isComplete)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                ProgressBar = percentage / 100;

                if (isComplete)
                {
                    IsVisibleProgressBar = false;
                }
            });
        }

        private void CheckingDevices_Click()
        {

            if (_isCheckingDevice)
            {
                EndCheckDevice();
            }

            _cancellTokenSource = new CancellationTokenSource();
            _cancellToken = _cancellTokenSource.Token;

            CheckDeviceBtnText = "Update";

            Task task1 = Task.Factory.StartNew(() =>
            {
                _client.Check_Devices(_cancellTokenSource.Token);
            }, _cancellTokenSource.Token);

            _isCheckingDevice = true;
        }

        private void EndCheckDevice()
        {
            _cancellToken.ThrowIfCancellationRequested();
            _cancellTokenSource.Cancel();
            _cancellTokenSource.Dispose();
            _isCheckingDevice = false;
        }

        private async void DeviceTapped(Device_Info item)
        {

            if (_isCheckingDevice)
            {
                EndCheckDevice();
            }

            List<string> fileNameList = await _folderPicker.PickFolder();

            if (fileNameList != null && fileNameList.Count > 0)
            {
                Task task = Task.Factory.StartNew(() =>
                {
                    _client.SendFile_Async(item.IP, fileNameList);
                });
            }
        }

        private void ServerTextError_Callback(string str, bool isError)
        {
            if (str != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if(isError)
                    {
                        TextErrorColor = Microsoft.Maui.Graphics.Colors.Red;
                    }
                    else
                    {
                        TextErrorColor = Microsoft.Maui.Graphics.Colors.Green;
                    }
                    TextError = str;
                });
            }
        }

        private void ServerText_Callback(string str)
        {
            if (str != null)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    string[] name_IP = str.Split(';');

                    if (name_IP[0] != null && name_IP[1] != null)
                    {
                        Device_Info device_Info = new Device_Info { IP = name_IP[0], Name = name_IP[1] };
                        Device_List.Add(device_Info);
                    }
                    else
                        Console.WriteLine(str);
                });
            }
        }

        #endregion


        #region Interface InavigatedAword implementation

        public void OnNavigatedFrom(INavigationParameters parameters) { }

        public void OnNavigatedTo(INavigationParameters parameters)
        {
           // await Task.Delay(5000);
            //List<string> fileNameList = await _folderPicker.PickFolder();
           // var t = Task.Run(() =>
            //{
            //    Remote.SendFile_Async();
            //    // UDPsendFile(fileNameList[0]);
            //});
            Start();
        }

        #endregion


        #region ---- Override ----

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
            //switch (args.PropertyName)
            //{
            //    case "Search":
            //        break;
            //    default:
            //        break;
            //}
        }

        #endregion

    }
}
