using System.Collections.Generic;
using System.Threading.Tasks;
using Xartic.App.Infrastructure.Helpers;

namespace Xartic.App.Abstractions.Navigation
{
    public interface INavigatedFromAware
    {
        Task OnNavigatedFromAsync(IDictionary<string, StringValues> parameters);
    }
}
