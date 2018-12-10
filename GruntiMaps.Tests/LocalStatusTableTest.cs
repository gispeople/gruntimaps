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
            _table = new LocalStatusTable(Path.GetTempPath(), "testTable");
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