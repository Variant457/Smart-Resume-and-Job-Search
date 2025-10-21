using Smart_Resume_Job_Search_App.AppServices;
using Smart_Resume_Job_Search_App.Domain;
using Smart_Resume_Job_Search_App.UINav;
using Smart_Resume_Job_Search_App.Infrastructure;
using System.ComponentModel;
using System.Windows.Input;

namespace Smart_Resume_Job_Search_App.Presentation
{
    public sealed class HomeViewModel : INotifyPropertyChanged
    {
        private readonly IAppFlow appFlow;
        private readonly ApplicationFacade facade;
        private readonly INetworkStatus net;
        public bool IsUploadEnabled;
        public bool IsJobSearchVisible;
        public bool IsJobSearchEnabled;
        public bool IsHistoryVisible;

        public HomeViewModel(IAppFlow appFlow, ApplicationFacade facade, INetworkStatus net)
        {
            this.appFlow = appFlow;
            this.facade = facade;
            this.net = net;
            
            IsUploadEnabled = false;
            IsJobSearchVisible = false;
            IsJobSearchEnabled = false;
            IsHistoryVisible = false;

            StartJobSearchCommand = new DelegateCommand(async _ => await StartJobSearch(), _ => IsJobSearchEnabled);
            OpenHistoryCommand = new DelegateCommand(async _ => await OpenHistory(), _ => IsHistoryVisible);
        }

        public void OnLoaded()
        {
            var online = net.IsOnline();
            var hasPrev = facade.HasPreviousResume();

            IsUploadEnabled = online;
            IsJobSearchVisible = hasPrev;
            IsJobSearchEnabled = online && hasPrev;
            IsHistoryVisible = hasPrev;
        }

        public async Task UploadResume(InputBlob input, CancellationToken ct = default)
        {
            await facade.UploadResume(input, ct);
        }

        public ICommand StartJobSearchCommand { get; }
        public ICommand OpenHistoryCommand { get; }
        
        private Task StartJobSearch() => appFlow.StartSearch();

        private Task OpenHistory()
        {
            var last = facade.GetHistory()?.LastOrDefault();
            return last is null ? Task.CompletedTask
                : appFlow.ShowResults(last.jobs);
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private bool Set<T>(ref T field, T value, string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            return true;
        }

        private void RefreshCommands()
        {
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
            public bool CanExecute(object? parameter) => canExecute?.Invoke(parameter) ?? true;
            public async void Execute(object? parameter) => await execute(parameter);
            public event EventHandler? CanExecuteChanged;
            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
