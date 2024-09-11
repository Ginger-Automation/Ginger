using Amdocs.Ginger.CoreNET.Telemetry;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Threading.Tasks;

namespace GingerCoreNETUnitTest.Telemetry
{
    [TestClass]
    [TestCategory(TestCategory.UnitTest)]
    public class BlockingBufferQueueTests
    {
        [TestMethod]
        public async Task Enqueue_MultipleProducers_QueueHasExpectedItemCount()
        {
            BlockingBufferQueue<int> queue = new(bufferSize: 4);
            int parallelTaskCount = 5;
            int eachTaskItemCount = 20;
            Task[] tasks = new Task[parallelTaskCount];
            Random random = new();
            for(int index = 0; index < parallelTaskCount; index++)
            {
                tasks[index] = new(() =>
                {
                    for (int count = 0; count < eachTaskItemCount; count++)
                    {
                        queue.Enqueue(random.Next());
                    }
                });
            }
            
            foreach(Task task in tasks)
            {
                task.Start();
            }
            await Task.WhenAll(tasks);

            Assert.AreEqual(expected: parallelTaskCount * eachTaskItemCount, actual: queue.Count);
        }

        [TestMethod]
        public async Task Dequeue_QueueSizeLessThanBufferSizeSingleConsumer_ConsumerBlocked()
        {
            BlockingBufferQueue<int> queue = new(bufferSize: 2);

            Task dequeueTask = Task.Run(queue.Dequeue);
            await Task.Delay(TimeSpan.FromMilliseconds(50));

            Assert.IsFalse(dequeueTask.IsCompleted);
        }

        [TestMethod]
        public async Task Dequeue_QueueSizeLessThanBufferSizeMultipleConsumer_AllConsumersBlocked()
        {
            BlockingBufferQueue<int> queue = new(bufferSize: 2);

            Task dequeueTask1 = Task.Run(queue.Dequeue);
            Task dequeueTask2 = Task.Run(queue.Dequeue);
            await Task.Delay(TimeSpan.FromMilliseconds(50));

            Assert.IsFalse(dequeueTask1.IsCompleted);
            Assert.IsFalse(dequeueTask2.IsCompleted);
        }

        [TestMethod]
        public async Task Dequeue_QueueSizeGreaterThanBufferSizeSingleConsumer_ConsumerNotBlocked()
        {
            BlockingBufferQueue<int> queue = new(bufferSize: 2);

            Task dequeueTask = Task.Run(queue.Dequeue);
            queue.Enqueue(new Random().Next());
            queue.Enqueue(new Random().Next());
            await Task.Delay(TimeSpan.FromMilliseconds(50));

            Assert.IsTrue(dequeueTask.IsCompleted);
        }

        #region Flaky Tests Group 1
        /*Below tests are flaky because in case of some resource shortage, the task might not reach the expected status in the given time. 
         *It doesn't mean that the task was blocked by the queue, but instead it just didn't reach the status in time.
         *To see the flaky scenario, use 'Run Until Failure' option from 'Test Explorer'.
         *Couldn't find a better way to test these scenario, you are welcome to improve.
         */


        [TestMethod]
        public async Task Dequeue_QueueSizeGreaterThanBufferSizeMultipleConsumer_FirstConsumerNotBlocked()
        {
            BlockingBufferQueue<int> queue = new(bufferSize: 2);

            queue.Enqueue(new Random().Next());
            queue.Enqueue(new Random().Next());
            Task dequeueTask1 = Task.Run(queue.Dequeue);
            Task dequeueTask2 = Task.Run(queue.Dequeue);
            await Task.Delay(100);

            Assert.IsTrue(dequeueTask1.IsCompleted);
        }

        [TestMethod]
        public async Task Dequeue_QueueSizeGreaterThanBufferSizeMultipleConsumer_SubsequentConsumersBlocked()
        {
            BlockingBufferQueue<int> queue = new(bufferSize: 2);

            queue.Enqueue(new Random().Next());
            queue.Enqueue(new Random().Next());
            Task dequeueTask1 = Task.Run(queue.Dequeue);
            Task dequeueTask2 = Task.Run(queue.Dequeue);
            await Task.Delay(100);

            Assert.IsFalse(dequeueTask2.IsCompleted);
        }
        #endregion
    }
}
