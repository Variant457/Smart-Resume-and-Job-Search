
namespace Smart_Resume_Job_Search_App.UINav
{
    public interface IViewModelFactory
    {
        TViewModel Create<TViewModel>(object? parameter = null);
    }
}
