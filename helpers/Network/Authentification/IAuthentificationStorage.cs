namespace helpers.Network.Authentification
{
    public interface IAuthentificationStorage
    {
        bool IsValid(IAuthentificationData data);

        void Reload();
        void Remove(IAuthentificationData authentificationData);
        void SetTarget(object target);

        IAuthentificationData Get(string key);
        IAuthentificationData New();
    }
}