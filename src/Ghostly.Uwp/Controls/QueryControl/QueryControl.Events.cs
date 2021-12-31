using System;

namespace Ghostly.Uwp.Controls
{
    public sealed partial class QueryControl
    {
        public event EventHandler<QueryClearedEventArgs> OnQueryCleared = (s, e) => { };
        public event EventHandler<QuerySubmittedEventArgs> OnQuerySubmitted = (s, e) => { };
    }
}
