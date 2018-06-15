using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace MIXUI.TaskQueues
{
    public interface IBackgroundTaskQueue
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, IServiceScope, Task> workItem);
        Task<Func<CancellationToken, IServiceScope, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}
