using System;

namespace Ghostly.Uwp.Controls
{
    public sealed class QuerySubmittedEventArgs : EventArgs
    {
        public string QueryText { get; }

        public QuerySubmittedEventArgs(string queryText)
        {
            QueryText = queryText;
        }
    }
}
