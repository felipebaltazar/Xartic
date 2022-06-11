namespace Martic.Abstractions
{
    public interface IApplicationLifeCycleAware
    {
        Task OnSleepAsync(CancellationToken token);

        Task OnResumeAsync(CancellationToken token);
    }
}
