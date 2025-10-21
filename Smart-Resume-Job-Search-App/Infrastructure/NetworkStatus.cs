using Microsoft.Maui.Networking;

namespace Smart_Resume_Job_Search_App.Infrastructure
{
    public class NetworkStatus : INetworkStatus
    {
        public bool IsOnline() => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
    }
}
