﻿using System;
using System.IO;
using GruntiMaps.WebAPI.Models;
using Xunit;

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
            _table = new LocalStatusTable(options, "testTable");
            _jobId = Guid.NewGuid().ToString();
        }

        [Fact]
        public async void NewStatusRecordShouldBeQueuedByDefault()
        {
            _table.Clear();
            await _table.AddStatus("queue-1", _jobId);
            var status = await _table.GetStatus(_jobId);
            Assert.Equal(JobStatus.Queued, status);
        }

        [Fact]
        public async void JobStatusShouldBeFinishedIfAllRecordsFinished()
        {
            _table.Clear();
            await _table.AddStatus("queue-1", _jobId);
            await _table.AddStatus("queue-2", _jobId);
            await _table.UpdateStatus("queue-1", JobStatus.Finished);
            await _table.UpdateStatus("queue-2", JobStatus.Finished);
            var status = await _table.GetStatus(_jobId);
            Assert.Equal(JobStatus.Finished, status);
        }

        [Fact]
        public async void JobStatusShouldBeQueuedIfAtLeastOneRecordStillQueued()
        {
            _table.Clear();
            await _table.AddStatus("queue-1", _jobId);
            await _table.AddStatus("queue-2", _jobId);
            await _table.UpdateStatus("queue-1", JobStatus.Finished);
            var status = await _table.GetStatus(_jobId);
            Assert.Equal(JobStatus.Queued, status);
        }

        [Fact]
        public async void JobStatusShouldBeFailedIfAtLeastOneRecordFailed()
        {
            _table.Clear();
            await _table.AddStatus("queue-1", _jobId);
            await _table.AddStatus("queue-2", _jobId);
            await _table.UpdateStatus("queue-1", JobStatus.Failed);
            var status = await _table.GetStatus(_jobId);
            Assert.Equal(JobStatus.Failed, status);
        }

        public void Dispose()
        {
        }
    }
}