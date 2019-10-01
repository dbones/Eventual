namespace Eventual.Middleware.Publishing
{
    using System;
    using System.Collections.Generic;

    public class PublishContextActions
    {
        public Type PrepareMessageContextForPublish { get; set; }
        public Type InvokePublisherAction { get; set; }
        public Type ApmAction { get; set; }
        //public Type LoggingAction { get; set; }
        public List<Type> CustomActions { get; set; } = new List<Type>();
    }
}