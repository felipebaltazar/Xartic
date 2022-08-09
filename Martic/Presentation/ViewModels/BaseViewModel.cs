using Martic.Infrastructure.Helpers;

namespace Martic.Presentation.ViewModels
{
    public abstract class BaseViewModel : ObservableObject
    {
        private bool isBusy;

        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        protected virtual async Task ExecuteBusyTask(Func<Task> task, CancellationToken token)
        {
            if (IsBusy || token.IsCancellationRequested)
                return;

            IsBusy = true;

            try
            {
                var taskCompletationSource = new TaskCompletionSource<bool>();
                var taskResolved = task();
                using (var registration = token.Register(() => taskCompletationSource.SetCanceled()))
                {
                    var result = await Task.WhenAny(taskResolved, taskCompletationSource.Task).ConfigureAwait(false);
                    if (result == taskCompletationSource.Task)
                    {
                        throw new OperationCanceledException(token);
                    }

                    await taskResolved;
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
