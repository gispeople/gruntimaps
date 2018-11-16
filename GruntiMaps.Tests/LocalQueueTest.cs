using System;
using System.Diagnostics;
using System.IO;
using GruntiMaps.WebAPI.Models;
using Xunit;
using Xunit.Abstractions;

namespace GruntiMaps.Tests
{
    public class LocalQueueTest: IDisposable
    {
        private readonly LocalQueue _queue;
        private readonly ITestOutputHelper _output;
        public LocalQueueTest(ITestOutputHelper output)
        {
            _output = output;
            var options = new Options(Path.GetTempPath()) {QueueTimeLimit = 1, QueueEntryTries = 2};
            // set the time limit to 1 minute so we can check that expiry works
            // try twice in our tests.
            _queue = new LocalQueue(options, "testQueue");
        }
        
        [Fact]
        public async void AddMessage()
        {
            _output.WriteLine("Adding a message");
            _queue.Clear();
            const string msgString = "a new message";
            var msgId = await _queue.AddMessage(msgString);
            var message = await _queue.GetMessage();
            Assert.Equal(msgId, message.Id);
            Assert.Equal(msgString, message.Content);
            _output.WriteLine("Finished adding a message");
        }

        [Fact]
        public async void DeleteMessage()
        {
            _output.WriteLine("Deleting a message");
            _queue.Clear();
            const string msgString = "another message";
            var msgId = await _queue.AddMessage(msgString);
            var message = await _queue.GetMessage();
            Assert.NotNull(message); // we just added a message so we should be able to get one!
            Assert.Equal(msgId, message.Id); // the message id should match the one we just added
            Assert.NotNull(message.PopReceipt); // there should be a pop receipt string
            await _queue.DeleteMessage(message); // delete the message from the queue
            var message2 = await _queue.GetMessage();
            Assert.Null(message2);  // we started with an empty queue so message2 should have nothing in it.
            _output.WriteLine("Finished deleting a message");
        }

        [Fact]
        public async void PoisonMessage()
        {
            _output.WriteLine("Poisoning a message");
           
            Stopwatch swAdd = new Stopwatch();
            Stopwatch swGet = new Stopwatch();
            const int total = 500;
            // first, add messages
            for (int i = 0; i < total; i++)
            {
                var msgString = $"yet another message {i}";
                swAdd.Start();
                var msgId = await _queue.AddMessage(msgString);
                swAdd.Stop();
                _output.WriteLine($"Add msg {msgId}");
            }
            // now get them 
            for (int j = 0; j < total; j++)
            {
                swGet.Start();
                var message = await _queue.GetMessage();
                swGet.Stop();
                _output.WriteLine(message != null ? $"retrieved message {message.Id}" : "No message available");
            }
            _output.WriteLine($"{total} adds took {swAdd.Elapsed}");
            _output.WriteLine($"{total} gets took {swGet.Elapsed}");
            _output.WriteLine("Finished poisoning a message");

        }
        public void Dispose()
        {
        }
    }
}
