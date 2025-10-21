using Smart_Resume_Job_Search_App.UINav;

namespace Smart_Resume_Job_Search_App
{
    public partial class App : Application
    {
        private readonly IAppFlow flow;

        public App(IAppFlow flow)
        {
            InitializeComponent();
            this.flow = flow;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            var window = new Window(new AppShell());
            Dispatcher.Dispatch(async () => await flow.Start());
            return window;
        }
    }
}