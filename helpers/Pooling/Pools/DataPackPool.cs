using helpers.Network.Data;

namespace helpers.Pooling.Pools
{
    public class DataPackPool : Pool<DataPack>
    {
        public static DataPackPool Pool { get; } = new DataPackPool();

        public DataPackPool() 
        {
            _prepareToGet = PrepareGet;
            _prepareToStore = PrepareStore;
            _constructor = Constructor;
        }

        private void PrepareGet(DataPack dataPack) => dataPack?.Clear();
        private void PrepareStore(DataPack dataPack) => dataPack.Clear();
        
        private DataPack Constructor() => new DataPack();
    }
}