using GruntiMaps.Models;
using System;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace GruntiMaps.Tests
{
    public class UnitTest1: IDisposable
    {
        private readonly LocalQueue _queue;
        private readonly ITestOutputHelper output;
        public UnitTest1(ITestOutputHelper output)
        {
            this.output = output;
            var options = new Options(Path.GetTempPath());
            options.QueueTimeLimit = 1; // set the time limit to 1 minute so we can check that expiry works
            _queue = new LocalQueue(options, "testQueue");
        }
        [Fact]
        public async void TestLocalQueueAdd()
        {
            _queue.Clear();
            var msgString = "a new message";
            var msgId = await _queue.AddMessage(msgString);
            var message = await _queue.GetMessage();
            Assert.Equal(msgId, message.Id);
            Assert.Equal(msgString, message.Content);
        }

        [Fact]
        public async void TestLocalQueueDelete()
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
        public async void TestLocalQueuePoison()
        {
            for (int i = 0; i < 500; i++)
            {
                var msgString = $"yet another message {i}";
                var msgId = await _queue.AddMessage(msgString);
                output.WriteLine($"added msg {msgId}");
                var message = await _queue.GetMessage();
                output.WriteLine($"retrieved message {message.Id}");
            }

        }
        public void Dispose()
        {
        }
    }
}
