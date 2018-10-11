using GruntiMaps.Models;
using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace GruntiMaps.Tests
{
    public class LocalQueueTest: IDisposable
    {
        private readonly LocalQueue _queue;
        private readonly ITestOutputHelper output;
        public LocalQueueTest(ITestOutputHelper output)
        {
            this.output = output;
            var options = new Options(Path.GetTempPath());
            options.QueueTimeLimit = 1; // set the time limit to 1 minute so we can check that expiry works
            _queue = new LocalQueue(options, "testQueue");
        }
        [Fact]
        public async void AddMessage()
        {
            _queue.Clear();
            var msgString = "a new message";
            var msgId = await _queue.AddMessage(msgString);
            var message = await _queue.GetMessage();
            Assert.Equal(msgId, message.Id);
            Assert.Equal(msgString, message.Content);
        }

        [Fact]
        public async void DeleteMessage()
        {
            _queue.Clear();
            var msgString = "another message";
            var msgId = await _queue.AddMessage(msgString);
            var message = await _queue.GetMessage();
            Assert.NotNull(message); // we just added a message so we should be able to get one!
            Assert.Equal(msgId, message.Id); // the message id should match the one we just added
            Assert.NotNull(message.PopReceipt); // there should be a pop receipt string
            await _queue.DeleteMessage(message); // delete the message from the queue
            var message2 = await _queue.GetMessage();
            Assert.Null(message2);  // we started with an empty queue so message2 should have nothing in it.
        }
        [Fact]
        public async void PoisonMessage()
        {
            Stopwatch swAdd = new Stopwatch();
            Stopwatch swGet = new Stopwatch();
            int total = 500;
            // first, add messages
            for (int i = 0; i < total; i++)
            {
                var msgString = $"yet another message {i}";
                swAdd.Start();
                var msgId = await _queue.AddMessage(msgString);
                swAdd.Stop();
                output.WriteLine($"Add msg {msgId}");
            }
            // now get them 
            for (int j = 0; j < total; j++)
            {
                swGet.Start();
                var message = await _queue.GetMessage();
                swGet.Stop();
                output.WriteLine($"retrieved message {message.Id}");
            }
            output.WriteLine($"{total} adds took {swAdd.Elapsed}");
            output.WriteLine($"{total} gets took {swGet.Elapsed}");

        }
        public void Dispose()
        {
        }
    }
}
