namespace RaceTrack.Core.Messaging;


public class EventAggregator
{
    private readonly Dictionary<Type, List<Delegate>> eventSubscribers = new Dictionary<Type, List<Delegate>>();

    public void Publish<TMessage>(TMessage message)
    {
        var messageType = typeof(TMessage);
        if (eventSubscribers.ContainsKey(messageType))
        {
            var subscribers = eventSubscribers[messageType];
            foreach (var subscriber in subscribers)
            {
                var action = subscriber as Action<TMessage>;
                action?.Invoke(message);
            }
        }
    }

    public void Subscribe<TMessage>(Action<TMessage> action)
    {
        var messageType = typeof(TMessage);
        if (!eventSubscribers.ContainsKey(messageType))
        {
            eventSubscribers[messageType] = new List<Delegate>();
        }
        eventSubscribers[messageType].Add(action);
    }
}
