using System;

namespace Ghostly.Core
{
    public interface IErrorHandler
    {
        void HandleError(Exception ex);
    }
}
