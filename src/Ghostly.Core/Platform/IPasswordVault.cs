namespace Ghostly.Core.Pal
{
    public interface IPasswordVault
    {
        string GetPassword(string resource, string username);
        void SetPassword(string resource, string username, string password);
        void DeletePassword(string resource, string username, string password);
    }
}
