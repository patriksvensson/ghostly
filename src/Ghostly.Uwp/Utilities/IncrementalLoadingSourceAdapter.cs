using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Ghostly.Core.Mvvm;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;

namespace Ghostly.Uwp.Utilities
{
    public class IncrementalLoadingSourceAdapter<T> :
        IList, INotifyCollectionChanged, INotifyPropertyChanged, ISupportIncrementalLoading
    {
        public IIncrementalLoadingSource<T> Source { get; }
        public bool HasMoreItems => Source.HasMoreItems;
        public CoreDispatcher Dispatcher { get; }
        private IList List
        {
            get { return Source as IList; }
        }

        public bool IsFixedSize => List.IsFixedSize;
        public bool IsReadOnly => List.IsReadOnly;
        public int Count => List.Count;
        public bool IsSynchronized => List.IsSynchronized;
        public object SyncRoot => List.SyncRoot;
        public object this[int index] { get => List[index]; set => List[index] = value; }

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        public IncrementalLoadingSourceAdapter(IIncrementalLoadingSource<T> source)
        {
            Source = source ?? throw new ArgumentNullException(nameof(source));
            Dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            if (Source is INotifyPropertyChanged propertyChangedEvent)
            {
                propertyChangedEvent.PropertyChanged += PropChange_PropertyChanged;
            }

            if (Source is INotifyCollectionChanged collectionChangedEvent)
            {
                collectionChangedEvent.CollectionChanged += CollectChange_CollectionChanged;
            }

            Source.Refresh += OnClearCallback;
        }

        public IAsyncOperation<LoadMoreItemsResult> LoadMoreItemsAsync(uint count)
        {
            return Task.Run(async () =>
            {
                var result = await Source.LoadMoreItemsAsync((int)count);
                return new LoadMoreItemsResult
                {
                    Count = (uint)result,
                };
            }).AsAsyncOperation();
        }

        private void OnClearCallback(object sender, EventArgs e)
        {
            RefreshAsync().FireAndForgetSafeAsync();
        }

        public void Dispose()
        {
        }

        public Task RefreshAsync()
        {
            var previousCount = List.Count;
            List.Clear();
            if (previousCount == 0)
            {
                return LoadMoreItemsAsync(0).AsTask();
            }

            return Task.CompletedTask;
        }

        private void PropChange_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(sender, e);
        }

        private async void CollectChange_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                try
                {
                    CollectionChanged?.Invoke(sender, e);
                }
                catch
                {
                }
            });
        }

        public int Add(object value)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(object value)
        {
            return List.Contains(value);
        }

        public int IndexOf(object value)
        {
            return List.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            throw new NotSupportedException();
        }

        public void Remove(object value)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }

        public void CopyTo(Array array, int index)
        {
            List.CopyTo(array, index);
        }

        public IEnumerator GetEnumerator()
        {
            return List.GetEnumerator();
        }
    }
}
