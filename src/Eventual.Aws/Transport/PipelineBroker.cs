namespace Eventual.Transport
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class PipelineBroker
    {
        private readonly Dictionary<string, Func<object, Task>> _routes = new Dictionary<string, Func<object, Task>>();
        public void AddRoute(string topic, Func<object, Task> @delegate)
        {
            _routes.Add(topic, @delegate);
        }

        public Task Dispatch(string topic, object payload)
        {
            if (_routes.TryGetValue(topic, out var @delegate))
            {
                return @delegate(payload);
            }

            throw new Exception($"topic is not supported: {topic}");
        }
    }
}