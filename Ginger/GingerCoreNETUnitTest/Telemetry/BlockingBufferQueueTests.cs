#region License
/*
Copyright © 2014-2024 European Support Limited

Licensed under the Apache License, Version 2.0 (the "License")
you may not use this file except in compliance with the License.
You may obtain a copy of the License at 

http://www.apache.org/licenses/LICENSE-2.0 

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS, 
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
See the License for the specific language governing permissions and 
limitations under the License. 
*/
#endregion

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
            for (int index = 0; index < parallelTaskCount; index++)
            {
                tasks[index] = new(() =>
                {
                    for (int count = 0; count < eachTaskItemCount; count++)
                    {
                        queue.Enqueue(random.Next());
                    }
                });
            }

            foreach (Task task in tasks)
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
            await TaskTryWaitAsync(dequeueTask, TimeSpan.FromSeconds(1));

            Assert.IsFalse(dequeueTask.IsCompleted);
        }

        [TestMethod]
        public async Task Dequeue_QueueSizeLessThanBufferSizeMultipleConsumer_AllConsumersBlocked()
        {
            BlockingBufferQueue<int> queue = new(bufferSize: 2);

            Task dequeueTask1 = Task.Run(queue.Dequeue);
            Task dequeueTask2 = Task.Run(queue.Dequeue);
            await TaskTryWaitAsync(dequeueTask1, TimeSpan.FromSeconds(1));
            await TaskTryWaitAsync(dequeueTask2, TimeSpan.FromSeconds(1));

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
            await TaskTryWaitAsync(dequeueTask, TimeSpan.FromSeconds(1));

            Assert.IsTrue(dequeueTask.IsCompleted);
        }

        private static async Task TaskTryWaitAsync(Task taskToWait, TimeSpan timeout)
        {
            try
            {
                await taskToWait.WaitAsync(timeout);
            }
            catch { }
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
