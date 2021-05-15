namespace Eventual.Transport
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Threading.Tasks.Sources;

    public class WorkerPool : IDisposable
    {
        private readonly ConcurrentQueue<Work> _workItems = new ConcurrentQueue<Work>();
        public volatile bool _isDisposing = false;

        public void Schedule(Work work)
        {
            if (_isDisposing)
            {
                work.Dispose();
                return;
            }

            _workItems.Enqueue(work);
        }

        public void Start()
        {
            var options = new ParallelOptions
            {
                MaxDegreeOfParallelism = 10
            };

            Parallel.ForEach(_workItems, options, workItem =>
            {
                try
                {
                    workItem.Start();
                }
                catch (Exception e)
                {
                    workItem.Dispose();
                }
            });
        }

        public void Dispose()
        {
            while (_workItems.TryDequeue(out var item))
            {
                item.Dispose();
            }
        }
    }
}