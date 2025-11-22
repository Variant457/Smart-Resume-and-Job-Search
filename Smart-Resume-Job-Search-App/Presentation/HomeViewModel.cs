using Smart_Resume_Job_Search_App.AppServices;
using Smart_Resume_Job_Search_App.Domain;
using Smart_Resume_Job_Search_App.UINav;
using Smart_Resume_Job_Search_App.Infrastructure;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Diagnostics;

namespace Smart_Resume_Job_Search_App.Presentation
{
    public sealed class HomeViewModel : INotifyPropertyChanged
    {
        private readonly IAppFlow appFlow;
        private readonly ApplicationFacade facade;
        private readonly INetworkStatus net;

        private bool isUploadEnabled;
        private bool isJobSearchVisible;
        private bool isJobSearchEnabled;
        private bool isHistoryVisible;

        public bool IsUploadEnabled
        {
            get => isUploadEnabled;
            set => Set(ref isUploadEnabled, value);
        }

        public bool IsJobSearchVisible
        {
            get => isJobSearchVisible;
            set => Set(ref isJobSearchVisible, value);
        }

        public bool IsJobSearchEnabled
        {
            get => isJobSearchEnabled;
            set => Set(ref isJobSearchEnabled, value);
        }

        public bool IsHistoryVisible
        {
            get => isHistoryVisible;
            set => Set(ref isHistoryVisible, value);
        }

        public HomeViewModel(IAppFlow appFlow, ApplicationFacade facade, INetworkStatus net)
        {
            this.appFlow = appFlow;
            this.facade = facade;
            this.net = net;

            IsUploadEnabled = false;
            IsJobSearchVisible = false;
            IsJobSearchEnabled = false;
            IsHistoryVisible = false;

            OpenHistoryCommand = new DelegateCommand(async _ => await OpenHistory(), _ => IsHistoryVisible);
        }

        public void OnLoaded()
        {
            Debug.WriteLine("[HomeViewModel] OnLoaded start");
            var online = net.IsOnline();
            var hasPrev = facade.HasPreviousResume();
            Debug.WriteLine($"[HomeViewModel] online={online}, hasPrev={hasPrev}");

            IsUploadEnabled = online;
            IsJobSearchVisible = hasPrev;
            IsJobSearchEnabled = online && hasPrev;
            IsHistoryVisible = hasPrev;

            RefreshCommands();
            Debug.WriteLine("[HomeViewModel] OnLoaded end");
        }

        public async Task UploadResume(InputBlob input)
        {
            Debug.WriteLine($"[HomeViewModel] UploadResume: {input?.FileName} size={input?.Content?.Length}");
            await appFlow.StartSearch(input);
        }

        public ICommand StartJobSearchCommand { get; }
        public ICommand OpenHistoryCommand { get; }

        private Task OpenHistory()
        {
            Debug.WriteLine("[HomeViewModel] OpenHistory");
            var last = facade.GetJobPostings().Result.LastOrDefault();
            return last is null ? Task.CompletedTask : appFlow.ShowResults();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        private bool Set<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            Debug.WriteLine($"[HomeViewModel] PropertyChanged: {propertyName} -> {value}");
            return true;
        }

        private void RefreshCommands()
        {
            Debug.WriteLine("[HomeViewModel] RefreshCommands");
            (StartJobSearchCommand as DelegateCommand)?.RaiseCanExecuteChanged();
            (OpenHistoryCommand as DelegateCommand)?.RaiseCanExecuteChanged();
        }

        public sealed class DelegateCommand : ICommand
        {
            private readonly Func<object?, Task> execute;
            private readonly Predicate<object?>? canExecute;
            public DelegateCommand(Func<object?, Task> execute, Predicate<object?>? canExecute = null)
            {
                this.execute = execute ?? throw new ArgumentNullException(nameof(execute));
                this.canExecute = canExecute;
            }
            public bool CanExecute(object? parameter)
            {
                var result = canExecute?.Invoke(parameter) ?? true;
                Debug.WriteLine($"[DelegateCommand] CanExecute -> {result}");
                return result;
            }
            public async void Execute(object? parameter)
            {
                Debug.WriteLine("[DelegateCommand] Execute");
                await execute(parameter);
            }
            public event EventHandler? CanExecuteChanged;
            public void RaiseCanExecuteChanged()
            {
                Debug.WriteLine("[DelegateCommand] RaiseCanExecuteChanged");
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
