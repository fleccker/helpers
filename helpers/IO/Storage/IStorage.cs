namespace helpers.IO.Storage
{
    public interface IStorage<TData>
    {
        int Size { get; }
        bool IsLoaded { get; }
        string TargetPath { get; }

        TData Data { get; }

        StorageMode Mode { get; }

        void Reload();
        void Load();
        void Save();
        void Clear();
    }
}