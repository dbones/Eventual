namespace Eventual
{
    using System;
    using System.Collections.Generic;

    public class Message<T>
    {
        public Message(T body) : this()
        {
            Body = body;
        }

        public Message()
        {
            DateTime = DateTime.UtcNow;
            Id = Guid.NewGuid().ToString("D");
            CorrelationId = Id;
            Metadata = new Dictionary<string, string>();
        }

        public DateTime DateTime { get; set; }
        public string Id { get; set; }
        public string CorrelationId { get; set; }

        public IDictionary<string, string> Metadata { get; set; }

        public T Body { get; set; }
    }
}