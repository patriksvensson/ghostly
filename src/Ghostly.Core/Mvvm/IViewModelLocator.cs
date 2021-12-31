namespace Ghostly.Core.Mvvm
{
    public interface IViewModelLocator
    {
        object GetViewModel(object view, out bool rebind);
    }
}
