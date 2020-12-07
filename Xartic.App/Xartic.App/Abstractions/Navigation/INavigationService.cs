using System.Collections.Generic;
using System.Threading.Tasks;
using Xartic.App.Infrastructure.Helpers;

namespace Xartic.App.Abstractions.Navigation
{
    public interface INavigationService
    {
        Task NavigateTo(string url);

        Task NavigateTo(string url, bool animate);

        Task NavigateTo(string url, IDictionary<string, StringValues> parameters);

        Task NavigateTo(string url, IDictionary<string, StringValues> parameters, bool animated);
    }
}
