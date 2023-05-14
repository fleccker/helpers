using helpers.Logging;

namespace helpers.Pooling.Pools
{
    public class LogBuilderPool : Pool<LogBuilder>
    {
        public LogBuilderPool()
        {
            _prepareToStore = PrepareStore;
            _constructor = Construct;
        }

        public static LogBuilderPool Pool { get; } = new LogBuilderPool();

        private void PrepareStore(LogBuilder logBuilder)
        {
            logBuilder.Reset();
        }

        private LogBuilder Construct()
        {
            return new LogBuilder();
        }
    }
}
