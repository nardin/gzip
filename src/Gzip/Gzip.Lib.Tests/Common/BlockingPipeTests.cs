using System;
using System.Threading.Tasks;
using Gzip.Lib.Collection;
using Xunit;

namespace Gzip.Lib.Tests.Common
{
    public class BlockingPipeTests
    {
        [Fact]
        public static void TestBasicScenarios()
        {
            var bc = new BlockingPipe<int>(3);
            var tks = new Task[2];
            // A simple blocking consumer with no cancellation.
            int expect = 1;
            tks[0] = Task.Run(() =>
            {
                while (!bc.IsCompleted)
                {
                    try
                    {
                        int data = bc.Take();
                        Assert.Equal(expect, data);
                        expect++;
                    }
                    catch (InvalidOperationException)
                    {
                    } // throw when CompleteAdding called
                }
            });

            // A simple blocking producer with no cancellation.
            tks[1] = Task.Run(() =>
            {
                bc.Add(1);
                bc.Add(2);
                bc.Add(3);
            });

            Task.WaitAll(tks);
        }
    }
}
