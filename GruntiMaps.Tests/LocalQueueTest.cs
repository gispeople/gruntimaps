/*

Copyright 2016, 2017, 2018 GIS People Pty Ltd

This file is part of GruntiMaps.

GruntiMaps is free software: you can redistribute it and/or modify it under 
the terms of the GNU Affero General Public License as published by the Free
Software Foundation, either version 3 of the License, or (at your option) any
later version.

GruntiMaps is distributed in the hope that it will be useful, but WITHOUT ANY
WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR
A PARTICULAR PURPOSE. See the GNU Affero General Public License for more 
details.

You should have received a copy of the GNU Affero General Public License along
with GruntiMaps.  If not, see <https://www.gnu.org/licenses/>.

*/
using System;
using System.Diagnostics;
using System.IO;
using GruntiMaps.ResourceAccess.Local;
using GruntiMaps.WebAPI.Models;
using Xunit;
using Xunit.Abstractions;

namespace GruntiMaps.Tests
{
    public class LocalQueueTest: IDisposable
    {
        private readonly LocalConversionQueue _queue;
        private readonly ITestOutputHelper _output;
        public LocalQueueTest(ITestOutputHelper output)
        {
            _output = output;
            // set the time limit to 1 minute so we can check that expiry works
            // try twice in our tests.
            _queue = new LocalConversionQueue(Path.GetTempPath(), 1, 2, "testQueue");
        }
        
//        [Fact]
//        public async void AddMessage()
//        {
//            _output.WriteLine("Adding a message");
//            _queue.Clear();
//            const string msgString = "a new message";
//            var msgId = await _queue.AddMessage(msgString);
//            var message = await _queue.GetMessage();
//            Assert.Equal(msgId, message.Id);
//            Assert.Equal(msgString, message.Content);
//            _output.WriteLine("Finished adding a message");
//        }
//
//        [Fact]
//        public async void DeleteMessage()
//        {
//            _output.WriteLine("Deleting a message");
//            _queue.Clear();
//            const string msgString = "another message";
//            var msgId = await _queue.AddMessage(msgString);
//            var message = await _queue.GetMessage();
//            Assert.NotNull(message); // we just added a message so we should be able to get one!
//            Assert.Equal(msgId, message.Id); // the message id should match the one we just added
//            Assert.NotNull(message.PopReceipt); // there should be a pop receipt string
//            await _queue.DeleteMessage(message); // delete the message from the queue
//            var message2 = await _queue.GetMessage();
//            Assert.Null(message2);  // we started with an empty queue so message2 should have nothing in it.
//            _output.WriteLine("Finished deleting a message");
//        }
//
//        [Fact]
//        public async void PoisonMessage()
//        {
//            _output.WriteLine("Poisoning a message");
//           
//            Stopwatch swAdd = new Stopwatch();
//            Stopwatch swGet = new Stopwatch();
//            const int total = 500;
//            // first, add messages
//            for (int i = 0; i < total; i++)
//            {
//                var msgString = $"yet another message {i}";
//                swAdd.Start();
//                var msgId = await _queue.AddMessage(msgString);
//                swAdd.Stop();
//                _output.WriteLine($"Add msg {msgId}");
//            }
//            // now get them 
//            for (int j = 0; j < total; j++)
//            {
//                swGet.Start();
//                var message = await _queue.GetMessage();
//                swGet.Stop();
//                _output.WriteLine(message != null ? $"retrieved message {message.Id}" : "No message available");
//            }
//            _output.WriteLine($"{total} adds took {swAdd.Elapsed}");
//            _output.WriteLine($"{total} gets took {swGet.Elapsed}");
//            _output.WriteLine("Finished poisoning a message");
//
//        }
        public void Dispose()
        {
        }
    }
}
