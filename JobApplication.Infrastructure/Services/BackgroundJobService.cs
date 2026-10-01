using Hangfire;
using JobApplication.Application.Interfaces;
using System.Linq.Expressions;

namespace JobApplication.Infrastructure.Services
{
    public class BackgroundJobService : IBackgroundJobService
    {
        private readonly IBackgroundJobClient _jobClient;

        public BackgroundJobService(IBackgroundJobClient jobClient)
        {
            _jobClient = jobClient;
        }
        public Task Enqueue<T>(Expression<Action<T>> methodCall)
        {
            _jobClient.Enqueue(methodCall);
            return Task.CompletedTask;
        }

        public Task Enqueue<T>(Expression<Func<T, Task>> methodCall)
        {
            _jobClient.Enqueue(methodCall);
            return Task.CompletedTask;
        }

        public Task Schedule<T>(Expression<Action<T>> methodCall, TimeSpan timeSpan)
        {
            _jobClient.Schedule(methodCall, timeSpan);
            return Task.CompletedTask;
        }

        public Task Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan timeSpan)
        {
            _jobClient.Schedule(methodCall, timeSpan);
            return Task.CompletedTask;
        }
    }
}
