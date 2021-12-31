using System;
using System.ComponentModel;
using System.Diagnostics;

namespace Ghostly.Core.Mvvm
{
    public interface IReadOnlyState<T> : INotifyPropertyChanged
    {
        T Value { get; }
    }

    [DebuggerDisplay("{Value,nq}")]
    public sealed class Stateful<T> : Observable, IReadOnlyState<T>
    {
        private readonly Action<T> _action;
        private T _backing;

        public T Value
        {
            get => _backing;
            set
            {
                SetProperty(ref _backing, value);
                _action?.Invoke(value);
            }
        }

        public Stateful()
        {
            _backing = default;
        }

        public Stateful(Action<T> action)
            : this()
        {
            _action = action;
        }

        public Stateful(T value)
        {
            _backing = value;
        }

        public Stateful(T value, Action<T> action)
            : this(value)
        {
            _action = action;
        }

        public void Initialize(T value)
        {
            SetProperty(ref _backing, value);
        }
    }
}
