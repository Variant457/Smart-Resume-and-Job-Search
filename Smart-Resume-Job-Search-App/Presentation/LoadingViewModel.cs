using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Smart_Resume_Job_Search_App.Presentation
{
    public class LoadingViewModel : INotifyPropertyChanged
    {
        private string _message = "Working on it...";

        public string Message
        {
            get => _message;
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged();
                }
            }
        }

        // You can later add CancellationCommand, Progress, etc.

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
