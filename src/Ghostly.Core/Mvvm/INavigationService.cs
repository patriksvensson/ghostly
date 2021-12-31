using System.Threading.Tasks;

namespace Ghostly.Core.Mvvm
{
    public interface INavigationService
    {
        bool HasContent { get; }
        bool CanGoBack { get; }

        Task<bool> GoBack();
        Task<bool> Navigate<TViewModel>(object parameter = null)
            where TViewModel : IViewModel;
    }
}
