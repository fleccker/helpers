using helpers.Extensions;

using System.Collections.Generic;

namespace helpers.Events
{
    public static class EventManager
    {
        private static readonly List<EventProvider> m_Handlers = new List<EventProvider>();

        public static IReadOnlyList<EventProvider> Handlers => m_Handlers;

        public static void DisableHandler(string id)
        {
            if (TryGetHandler(id, out var handler))
            {
                handler.IsAllowed = false;
            }
        }

        public static void EnableHandler(string id)
        {
            if (TryGetHandler(id, out var handler))
            {
                handler.IsAllowed = true;
            }
        }

        public static EventProvider AddHandler(string name)
        {
            if (GetHandler(name) != null)
            {
                Log.Warn("Event Manager", $"Tried registering a duplicate handler: {name}");
                return null;
            }

            var handler = new EventProvider(name);

            m_Handlers.Add(handler);

            return handler;
        }

        public static EventProvider GetHandler(string id)
            => TryGetHandler(id, out var handler) ? handler : null;

        public static bool RemoveHandler(string id)
            => m_Handlers.RemoveAll(ev => ev.Id == id) > 0;

        public static bool TryGetHandler(string id, out EventProvider handler)
            => m_Handlers.TryGetFirst(ev => ev.Id == id, out handler);
    }
}