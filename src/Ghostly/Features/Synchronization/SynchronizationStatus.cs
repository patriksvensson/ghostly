namespace Ghostly.Features.Synchronization
{
    public enum SynchronizationStatus
    {
        Completed = 0,
        AlreadyInProgress = 1,
        RequiresAuthentication = 2,
        AuthenticationFailed = 3,
        RateLimited = 4,
        BackOff = 5,
        NoInternetConnection = 6,
        UnknownError = 7,
    }
}
