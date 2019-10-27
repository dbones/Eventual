namespace Eventual.Transport
{
    using System;
    using System.Collections.Generic;

    public class WorkerPool : IDisposable
    {
        private readonly List<Work> _workingProcesses = new List<Work>();

        public void Schedule(Work work)
        {
            _workingProcesses.Add(work);
            work.Start();
        }

        public void Dispose()
        {
            foreach (var work in _workingProcesses)
            {
                work.Dispose();
            }

            _workingProcesses.Clear();
        }
    }
}