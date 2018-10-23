using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace GruntiMaps.WebAPI.Services
{
    // Copyright (c) .NET Foundation. Licensed under the Apache License, Version 2.0. 
    /// <summary>
    ///     Base class for implementing a long running <see cref="IHostedService" />.
    /// </summary>
    public abstract class BackgroundService : IHostedService
    {
        private Task _executingTask;
        private readonly CancellationTokenSource _stoppingCts = new CancellationTokenSource();

        protected virtual async Task ExecuteAsync(CancellationToken stoppingToken) {
            do {
                await Process();
                await Task.Delay(5000, stoppingToken);
            } while (!stoppingToken.IsCancellationRequested);
        }
        protected abstract Task Process();

        public virtual Task StartAsync(CancellationToken cancellationToken)
        {
            // Store the task we're executing
            _executingTask = ExecuteAsync(_stoppingCts.Token);

            // If the task is completed then return it, 
            // this will bubble cancellation and failure to the caller
            if (_executingTask.IsCompleted) return _executingTask;

            // Otherwise it's running
            return Task.CompletedTask;
        }

        public virtual async Task StopAsync(CancellationToken cancellationToken)
        {
            // Stop called without start
            if (_executingTask == null) return;

            try
            {
                // Signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                // Wait until the task completes or the stop token triggers
                await Task.WhenAny(_executingTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }
        }
    }
}