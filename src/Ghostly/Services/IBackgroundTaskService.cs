namespace Ghostly.Services
{
    public interface IBackgroundTaskService
    {
        bool BackgroundTasksAllowed { get; }

        bool IsRegistered(string name);
        bool ToggleTask(string name);
    }
}
