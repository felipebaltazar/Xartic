using System.Threading;
using System.Threading.Tasks;

namespace Xartic.App.Abstractions.Navigation
{
    public interface IApplicationLifeCycleAware
    {
        Task OnSleepAsync(CancellationToken token);

        Task OnResumeAsync(CancellationToken token);
    }
}
