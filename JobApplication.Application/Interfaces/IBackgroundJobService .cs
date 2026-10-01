using System.Linq.Expressions;

namespace JobApplication.Application.Interfaces
{
    public interface IBackgroundJobService
    {
        Task Enqueue<T>(Expression<Action<T>> methodCall);
        Task Enqueue<T>(Expression<Func<T, Task>> methodCall);
        Task Schedule<T>(Expression<Action<T>> methodCall, TimeSpan timeSpan);
        Task Schedule<T>(Expression<Func<T, Task>> methodCall, TimeSpan timeSpan);
    }
}
