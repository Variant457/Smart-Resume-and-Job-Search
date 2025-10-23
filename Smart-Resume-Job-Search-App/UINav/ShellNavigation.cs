using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;

namespace Smart_Resume_Job_Search_App.UINav
{
    internal class ShellNavigation : INavigationService
    {
        private readonly IViewModelFactory viewModelFactory;
        private readonly IViewResolver resolver;

        public ShellNavigation(IViewModelFactory viewModelFactory, IViewResolver resolver)
        {
            this.viewModelFactory = viewModelFactory;
            this.resolver = resolver;
        }

        public Task ReplaceRoot<TViewModel>(object? parameter = null)
        {
            Debug.WriteLine($"[ShellNavigation] ReplaceRoot<{typeof(TViewModel).Name}> invoked");
            var vm = viewModelFactory.Create<TViewModel>(parameter)
                ?? throw new InvalidOperationException($"ViewModel of type {typeof(TViewModel).Name} could not be created.");

            void Apply()
            {
                Debug.WriteLine("[ShellNavigation] Apply ReplaceRoot on UI thread");
                // Prefer setting Application.MainPage to avoid Window.Page setter issues on Windows
                var app = Application.Current ?? throw new InvalidOperationException("Application.Current is null");

                var shell = app.MainPage as AppShell ?? new AppShell();
                if (app.MainPage != shell)
                {
                    Debug.WriteLine("[ShellNavigation] Setting Application.Current.MainPage = AppShell");
                    app.MainPage = shell;
                }

                shell.Items.Clear();

                var title = typeof(TViewModel).Name.Replace("ViewModel", string.Empty);

                // Resolve the Page and construct canonical Shell hierarchy
                var page = resolver.Resolve(vm);
                Debug.WriteLine($"[ShellNavigation] Resolved page: {page?.GetType().Name}");

                var content = new ShellContent
                {
                    Title = title,
                    Content = page
                };

                var section = new ShellSection { Title = title };
                section.Items.Add(content);

                var item = new ShellItem { Title = title };
                item.Items.Add(section);

                shell.Items.Add(item);
                shell.CurrentItem = item;
                Debug.WriteLine("[ShellNavigation] Root replaced successfully");
            }

            if (MainThread.IsMainThread)
            {
                Apply();
                return Task.CompletedTask;
            }

            var tcs = new TaskCompletionSource();
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    Apply();
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ShellNavigation] ReplaceRoot failed: {ex}");
                    tcs.SetException(ex);
                }
            });
            return tcs.Task;
        }

        public async Task Show<TViewModel>(object? parameter = null)
        {
            Debug.WriteLine($"[ShellNavigation] Show<{typeof(TViewModel).Name}> invoked");
            var vm = viewModelFactory.Create<TViewModel>(parameter)
                ?? throw new InvalidOperationException($"ViewModel of type {typeof(TViewModel).Name} could not be created.");

            var page = resolver.Resolve(vm);
            Debug.WriteLine($"[ShellNavigation] Resolved page: {page?.GetType().Name}");

            if (MainThread.IsMainThread)
            {
                await Shell.Current!.Navigation.PushAsync(page);
                Debug.WriteLine("[ShellNavigation] Navigation.PushAsync completed");
                return;
            }

            var tcs = new TaskCompletionSource();
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await Shell.Current!.Navigation.PushAsync(page);
                    Debug.WriteLine("[ShellNavigation] Navigation.PushAsync completed (UI thread)");
                    tcs.SetResult();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ShellNavigation] Show failed: {ex}");
                    tcs.SetException(ex);
                }
            });
            await tcs.Task;
        }

        public Task GoBack()
        {
            Debug.WriteLine("[ShellNavigation] GoBack invoked");
            return Shell.Current!.Navigation.PopAsync();
        }
    }
}
