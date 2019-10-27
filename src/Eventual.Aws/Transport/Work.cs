namespace Eventual.Transport
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class Work : IDisposable
    {
        private Task _task;
        private readonly CancellationTokenSource _source = new CancellationTokenSource();

        public Action<CancellationToken> Action { get; set; }

        public Work(Action<CancellationToken> action)
        {
            Action = action;
        }

        public void Start()
        {
            _task = new Task(() => Action(_source.Token));
            _task.Start();
        }

        public void Dispose()
        {
            _source.Cancel();
        }
    }
}