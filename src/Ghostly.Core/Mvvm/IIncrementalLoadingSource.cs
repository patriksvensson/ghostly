using System;
using System.Threading.Tasks;

namespace Ghostly.Core.Mvvm
{
    public interface IIncrementalLoadingSource<T>
    {
        event EventHandler Refresh;

        Task<int> LoadMoreItemsAsync(int count);
        bool HasMoreItems { get; }
    }
}
