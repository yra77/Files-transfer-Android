

namespace Files_in_Locall_network.Delegates
{

    public delegate void Server_Text_CallBack(string taskResult);
    public delegate void Server_TextError_CallBack(string str, bool isError);
    public delegate void ProgressBardelegate(bool isSending);
    public delegate void ProgressChangeDelegate(double percentage, bool isComplete);

}
