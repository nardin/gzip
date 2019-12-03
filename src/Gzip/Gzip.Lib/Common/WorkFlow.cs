using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Gzip.Lib.Collection;
using Gzip.Lib.Worker;

namespace Gzip.Lib.Common
{
    /// <summary>
    /// WorkFlow builder.
    /// </summary>
    /// <typeparam name="TMessageType">Message's type</typeparam>
    internal class WorkFlow<TMessageType> where TMessageType: new()
    {
        /// <summary>
        /// All Workers.
        /// </summary>
        protected List<IWorker<TMessageType>> Workers;
        /// <summary>
        /// Last added worker.
        /// </summary>
        protected List<IWorker<TMessageType>> LastWorkers;
        /// <summary>
        /// Last added pipe.
        /// </summary>
        protected IPipe<TMessageType> LastPipe;
        /// <summary>
        /// Link to WorkFlow pipe.
        /// </summary>
        protected WorkFlowPipe<TMessageType> Pipe;
        /// <summary>
        /// Exceptions.
        /// </summary>
        protected List<Exception> Exceptions;
        /// <summary>
        /// Threads in Workflow.
        /// </summary>
        protected List<Thread> Treads;
        /// <summary>
        /// Cancellation token source/
        /// </summary>
        protected volatile CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        public WorkFlow()
        {
            Pipe = new WorkFlowPipe<TMessageType>(this);
            Workers = new List<IWorker<TMessageType>>();
            Exceptions = new List<Exception>();
        }

        public WorkFlow(CancellationTokenSource cancellationTokenSource)
        {
            this.cancelTokenSource = cancellationTokenSource ?? new CancellationTokenSource();

            Pipe = new WorkFlowPipe<TMessageType>(this);
            Workers = new List<IWorker<TMessageType>>();
            Exceptions = new List<Exception>();
        }

        /// <summary>
        /// WorkFlow Pipe 
        /// </summary>
        /// <typeparam name="TMessageType">Message's type</typeparam>
        internal class WorkFlowPipe<TMessageType> where TMessageType : new()
        {
            protected WorkFlow<TMessageType> workFlow;

            public WorkFlowPipe(WorkFlow<TMessageType> workFlow)
            {
                this.workFlow = workFlow;
            }

            /// <summary>
            /// Set pipe between workers.
            /// </summary>
            /// <typeparam name="T">Pipe's type.</typeparam>
            /// <param name="scale">Scale pipe.</param>
            /// <returns>WorkFlow</returns>
            public WorkFlow<TMessageType> Pipe<T>(int scale = 1) where T : IPipe<TMessageType>
            {
                var newPipe = (T)Activator.CreateInstance(typeof(T), scale);
                foreach (var worker in workFlow.LastWorkers)
                {
                    worker.SetOutPipe(newPipe);
                }

                workFlow.LastPipe = newPipe;
                return workFlow;
            }

            /// <summary>
            /// Run WorkFlow.
            /// </summary>
            public void Run()
            {
                var count = workFlow.Workers.Count;
                var threads = new List<Thread>(count);
                this.workFlow.Treads = threads;
                for (var i = 0; i < count; i++)
                {
                    var worker = workFlow.Workers[i];
                    worker.SetCancellationToken(this.workFlow.cancelTokenSource.Token);
                    var thread = new Thread((() =>
                    {
                        try
                        {
                            using (worker)
                            {
                                worker.Run();
                            }
                        }
                        catch (ThreadInterruptedException)
                        {
                            // Ignore. Throws when WorkFlow is canceled.
                        }
                        catch (OperationCanceledException)
                        {
                            // Ignore. Throws when WorkFlow is canceled.
                        }
                        catch (Exception e)
                        {
                            // Uncaught exception in worker.
                            this.workFlow.Exceptions.Add(e);
                            this.workFlow.Cancel();
                        }
                    }));
                    thread.Name = worker.GetType().Name + "_" + i;
                    thread.IsBackground = true;
                    thread.Start();
                    threads.Add(thread);
                }

                this.workFlow.cancelTokenSource.Token.Register(() =>
                {
                    this.workFlow.Cancel();
                });

                // Wait finish all workers.
                foreach (var thread in threads)
                {
                    if (thread.IsAlive && !this.workFlow.cancelTokenSource.IsCancellationRequested)
                    {
                        thread.Join();
                    }
                }

                //Aggregate exception from children threads.
                if (this.workFlow.Exceptions.Count > 0)
                {
                    throw new AggregateException(this.workFlow.Exceptions);
                }
            }
        }

        /// <summary>
        /// Set Step.
        /// </summary>
        /// <param name="worker">Worker object.</param>
        /// <returns>WorkFlow</returns>
        public WorkFlowPipe<TMessageType> Step(IWorker<TMessageType> worker)
        {
            LastWorkers = new List<IWorker<TMessageType>> {worker};

            if (LastPipe != null)
            {
                worker.SetInPipe(LastPipe);
            }
            Workers.AddRange(LastWorkers);
            return Pipe;
        }

        /// <summary>
        /// Set Step.
        /// </summary>
        /// <typeparam name="T">Worker type.</typeparam>
        /// <param name="scale">Scale workers.</param>
        /// <returns>WorkFlow</returns>
        public WorkFlowPipe<TMessageType> Step<T>(int scale) where T:IWorker<TMessageType>
        {
            if (scale <= 0)
            {
                throw new ArgumentException("", nameof(scale));
            }

            LastWorkers = new List<IWorker<TMessageType>>(scale);
            for (var i = 0; i < scale; i++)
            {
                var worker = Activator.CreateInstance<T>();
                if (LastPipe != null)
                {
                    worker.SetInPipe(LastPipe);
                }
                LastWorkers.Add(worker);
            }

            Workers.AddRange(LastWorkers);
            return Pipe;
        }

        /// <summary>
        /// Cancel Workflow.
        /// </summary>
        public void Cancel()
        {
            // Cancel children thread.
            if (!cancelTokenSource.IsCancellationRequested)
            {
                cancelTokenSource.Cancel();
            }

            // Interrupt waiting threads.
            foreach (var thread in this.Treads)
            {
                if ((thread.ThreadState & ThreadState.WaitSleepJoin) != 0)
                {
                    thread.Interrupt();
                }
            }
        }

        /// <summary>
        /// Get Count alive thread.
        /// </summary>
        public int GetCountThreadAlive()
        {
            return this.Treads.Count(tread => tread.IsAlive);
        }
    }
}
