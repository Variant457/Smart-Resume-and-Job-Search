namespace Smart_Resume_Job_Search_App
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            System.Diagnostics.Debug.WriteLine("[AppShell] .ctor start");
            InitializeComponent();
            System.Diagnostics.Debug.WriteLine("[AppShell] .ctor end");
        }

        protected override void OnAppearing()
        {
            System.Diagnostics.Debug.WriteLine("[AppShell] OnAppearing");
            base.OnAppearing();
        }
    }
}
