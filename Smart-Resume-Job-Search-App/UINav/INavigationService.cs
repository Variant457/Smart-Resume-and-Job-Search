

namespace Smart_Resume_Job_Search_App.UINav
{
    public interface INavigationService
    {
        Task ReplaceRoot<TViewModel>(object? parameter = null);
        Task Show<TViewModel>(object? parameter = null);
        Task GoBack();
    }
}
