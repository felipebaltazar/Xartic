using Xamarin.Forms;

namespace Xartic.App.Abstractions
{
    public interface IApplicationProvider
    {
        Application GetApplication();
        Page GetCurrentPage();
    }
}
