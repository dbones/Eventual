namespace Eventual.Middleware.Subscribing
{
    using System;
    using System.Collections.Generic;

    public class ReceivedContextActions
    {
        public Type ReadMessageFromQueueIntoContextAction { get; set; }
        public Type InvokeConsumerAction { get; set; }
        public Type DeadLetterAction { get; set; }
        public Type ApmAction { get; set; }
        public Type LoggingAction { get; set; }
        public List<Type> CustomActions { get; set; } = new List<Type>();
    }
}