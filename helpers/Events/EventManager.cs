using System.Collections.Generic;
using System.Linq;

namespace helpers.Events
{
    public static class EventManager
    {
        private static readonly HashSet<EventProvider> _knownEvents = new HashSet<EventProvider>();

        public static void Add(EventProvider eventProvider) => _knownEvents.Add(eventProvider);
        public static void Remove(EventProvider eventProvider) => _knownEvents.Remove(eventProvider);
        public static void Clear() => _knownEvents.Clear();
        public static EventProvider Get(string eventId) => _knownEvents.First(x => x.Id == eventId);
    }
}