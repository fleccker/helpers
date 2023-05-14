using System.Text;

namespace helpers.Pooling.Pools
{
    public class StringBuilderPool : Pool<StringBuilder>
    {
        public StringBuilderPool() 
        {
            _prepareToGet = PrepareGet;
            _prepareToStore = PrepareStore;
            _constructor = Construct;
        }

        public static StringBuilderPool Pool { get; } = new StringBuilderPool();

        public string PushReturn(StringBuilder stringBuilder)
        {
            var str = stringBuilder.ToString();

            Push(stringBuilder);
            return str;
        }

        private void PrepareGet(StringBuilder stringBuilder)
        {
            if (stringBuilder is null)
                return;

            stringBuilder.Clear();
        }

        private void PrepareStore(StringBuilder stringBuilder)
        {
            stringBuilder.Clear();
        }

        private StringBuilder Construct()
        {
            return new StringBuilder();
        }
    }
}
