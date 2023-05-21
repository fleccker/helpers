using Binarysharp.MemoryManagement;

using System.Linq;

using System.Diagnostics;

namespace helpers
{
    [LogSource("Memory")]
    public class Memory
    {
        private MemorySharp _mem;

        public Memory(Process process)
        {
            _mem = new MemorySharp(process);
        }

        public Memory(int pId)
        {
            _mem = new MemorySharp(pId);
        }

        public static Memory Current { get; } = new Memory(Process.GetCurrentProcess());

        public void KeyboardWrite(string windowName, object message)
        {
            _mem?.Windows?.GetWindowsByClassName(windowName)?.FirstOrDefault()?.Keyboard?.Write(message.ToString());
        }
    }
}
