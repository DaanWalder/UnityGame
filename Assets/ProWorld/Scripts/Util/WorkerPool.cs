using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace ProWorldSDK
{
    public static class WorkerPool
    {
        private class WorkToDo
        {
            public readonly DoWorkEventHandler Function;
            public readonly object Data;

            public WorkToDo(DoWorkEventHandler function, object data)
            {
                Function = function;
                Data = data;
            }
        }

        public static int MaxWorkers = 3;
        private static int _currentWorkers;
        private static readonly Queue<WorkToDo> WorkQueue = new Queue<WorkToDo>();
        private static readonly BackgroundWorker Manager = new BackgroundWorker();

        static WorkerPool()
        {
            MaxWorkers = Math.Max(Environment.ProcessorCount - 1, 1); // 1 less than processors, but at least 1

            Manager.DoWork += Assign;
        }

        private static void Assign(object sender, DoWorkEventArgs doWorkEventArgs)
        {
            while (WorkQueue.Count > 0)
            {
                if(_currentWorkers < MaxWorkers)
                {
                    var work = WorkQueue.Dequeue();

                    var bw = new BackgroundWorker();
                    bw.DoWork += work.Function;
                    bw.RunWorkerCompleted += Done;
                    bw.RunWorkerAsync(work.Data);

                    _currentWorkers++;
                }
                Thread.Sleep(50);
            }
        }

        private static void Done(object sender, RunWorkerCompletedEventArgs e)
        {
            _currentWorkers--;
        }
        public static void QueueWork(DoWorkEventHandler function, object data)
        {
            WorkQueue.Enqueue(new WorkToDo(function, data));

            if (!Manager.IsBusy)
                Manager.RunWorkerAsync();
        }
    }
}
