using System;
using System.Threading;
using Gzip.Lib.Common;
using Xunit;

namespace Gzip.Lib.Tests.Common
{
    public class WorkFlowTest
    {
        [Fact]
        public void ExceptionTest()
        {

            var workFlow = new WorkFlow<FileChunk>();
            Assert.Throws<AggregateException>(() =>
            {
                workFlow.Step<EndlessWorker>(1)
                    .Pipe<FakePipe>()
                    .Step<WaitWorker>(1)
                    .Pipe<FakePipe>()
                    .Step<ExeptionWorker>(1)
                    .Run();
            });

            Assert.Equal(workFlow.GetCountThreadAlive(), 0);
        }

        [Fact]
        public void CancelTest()
        {
            var workFlow = new WorkFlow<FileChunk>();
            new Thread(() =>
            {
                workFlow.Step<EndlessWorker>(1)
                    .Pipe<FakePipe>()
                    .Step<WaitWorker>(1)
                    .Pipe<FakePipe>()
                    .Step<EndlessWorker>(1)
                    .Run();
            }).Start();

            Thread.Sleep(100);
            workFlow.Cancel();
            Thread.Sleep(100);
            Assert.Equal(workFlow.GetCountThreadAlive(), 0);
        }

        [Fact]
        public void CancellationTokenSourceTest()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var workFlow = new WorkFlow<FileChunk>(cancellationTokenSource);
            new Thread(() =>
            {
                workFlow.Step<EndlessWorker>(1)
                    .Pipe<FakePipe>()
                    .Step<WaitWorker>(1)
                    .Pipe<FakePipe>()
                    .Step<EndlessWorker>(1)
                    .Run();
            }).Start();

            Thread.Sleep(100);
            cancellationTokenSource.Cancel(true);
            Thread.Sleep(100);
            Assert.Equal(workFlow.GetCountThreadAlive(), 0);
        }
    }
}
