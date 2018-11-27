using System;
using System.IO;
using GruntiMaps.ResourceAccess.Local;
using GruntiMaps.WebAPI.Models;

namespace GruntiMaps.Tests
{
    public class LocalStatusTableTest : IDisposable
    {
        private readonly LocalStatusTable _table;
        private readonly string _jobId;

        public LocalStatusTableTest()
        {
            var options = new Options(Path.GetTempPath())
            {
                QueueTimeLimit = 1, // set the time limit to 1 minute so we can check that expiry works
                QueueEntryTries = 2 // try twice in our tests.
            };
            _table = new LocalStatusTable(options.StoragePath, "testTable");
            _jobId = Guid.NewGuid().ToString();
        }

//        [Fact]
//        public async void NewStatusRecordShouldBeQueuedByDefault()
//        {
//            _table.Clear();
//            await _table.AddStatus("queue-1", _jobId);
//            var status = await _table.GetStatus(_jobId);
//            Assert.Equal(LayerStatus.Processing, status);
//        }
//
//        [Fact]
//        public async void JobStatusShouldBeFinishedIfAllRecordsFinished()
//        {
//            _table.Clear();
//            await _table.AddStatus("queue-1", _jobId);
//            await _table.AddStatus("queue-2", _jobId);
//            await _table.UpdateStatus("queue-1", LayerStatus.Finished);
//            await _table.UpdateStatus("queue-2", LayerStatus.Finished);
//            var status = await _table.GetStatus(_jobId);
//            Assert.Equal(LayerStatus.Finished, status);
//        }
//
//        [Fact]
//        public async void JobStatusShouldBeQueuedIfAtLeastOneRecordStillQueued()
//        {
//            _table.Clear();
//            await _table.AddStatus("queue-1", _jobId);
//            await _table.AddStatus("queue-2", _jobId);
//            await _table.UpdateStatus("queue-1", LayerStatus.Finished);
//            var status = await _table.GetStatus(_jobId);
//            Assert.Equal(LayerStatus.Processing, status);
//        }
//
//        [Fact]
//        public async void JobStatusShouldBeFailedIfAtLeastOneRecordFailed()
//        {
//            _table.Clear();
//            await _table.AddStatus("queue-1", _jobId);
//            await _table.AddStatus("queue-2", _jobId);
//            await _table.UpdateStatus("queue-1", LayerStatus.Failed);
//            var status = await _table.GetStatus(_jobId);
//            Assert.Equal(LayerStatus.Failed, status);
//        }

        public void Dispose()
        {
        }
    }
}