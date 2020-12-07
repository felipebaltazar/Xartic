using System.Collections.Generic;
using System.Threading.Tasks;
using Xartic.App.Infrastructure.Helpers;

namespace Xartic.App.Abstractions.Navigation
{
    public interface INavigatedAware
    {
        Task OnNavigatedAsync(IDictionary<string, StringValues> parameters);
    }
}
