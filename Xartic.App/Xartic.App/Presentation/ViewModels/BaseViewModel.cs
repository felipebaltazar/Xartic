using System;
using System.Threading;
using System.Threading.Tasks;
using Xartic.App.Infrastructure.Helpers;

namespace Xartic.App.Presentation.ViewModels
{
    public abstract class BaseViewModel : ObservableObject
    {
        private bool isBusy;

        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        protected virtual async Task ExecuteBusyTask(Func<Task> tarefa, CancellationToken token)
        {
            if (IsBusy || token.IsCancellationRequested)
                return;

            IsBusy = true;

            try
            {
                var taskCompletationSource = new TaskCompletionSource<bool>();
                using (var registration = token.Register(() => taskCompletationSource.SetCanceled()))
                {
                    await Task.WhenAny(tarefa(), taskCompletationSource.Task).ConfigureAwait(false);
                }
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}
