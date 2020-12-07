using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Xamarin.Forms;
using Xartic.App.Abstractions;
using Xartic.App.Abstractions.Navigation;
using Xartic.App.Infrastructure.Helpers;
using Xartic.App.Infrastructure.MVVM;

namespace Xartic.App.Infrastructure.Services
{
    public class NavigationService : INavigationService
    {
        private const string REMOVEPAGEPATH = "../";
        private const string RELATIVEPATH = "/";
        private const string REMOVEINSTRUCTION = "__RemovePage/";

        private readonly Assembly _assembly;
        private readonly IServiceResolver _resolver;
        private readonly IApplicationProvider _applicationProvider;

        public NavigationService(IServiceResolver resolver, IApplicationProvider applicationProvider)
        {
            _resolver = resolver;
            _applicationProvider = applicationProvider;

            _assembly = Assembly.GetExecutingAssembly();
        }

        public async Task NavigateTo(string url) =>
            await NavigateInternal(url, null, false).ConfigureAwait(false);

        public async Task NavigateTo(string url, bool animated) =>
            await NavigateInternal(url, null, animated).ConfigureAwait(false);

        public async Task NavigateTo(string url, IDictionary<string, StringValues> parameters) =>
            await NavigateInternal(url, parameters, false).ConfigureAwait(false);

        public async Task NavigateTo(string url, IDictionary<string, StringValues> parameters, bool animated) =>
            await NavigateInternal(url, parameters, animated).ConfigureAwait(false);

        private async Task NavigateInternal(string url, IDictionary<string, StringValues> parameters, bool animated)
        {
            parameters ??= new Dictionary<string, StringValues>();

            var uri = Parse(url);
            if (!url.StartsWith(RELATIVEPATH) &&
                !url.StartsWith(REMOVEPAGEPATH))
            {
                await SwithRootPage(uri, parameters, animated).ConfigureAwait(false);
                return;
            }

            var segments = uri.Segments.Where(s => !RELATIVEPATH.Equals(s));
            foreach (var segment in segments)
            {
                var decoded = HttpUtility.UrlDecode(segment);

                var indiceQuery = decoded.IndexOf("?") + 1;
                var query = indiceQuery > 0
                    ? decoded.Substring(indiceQuery)
                    : HttpUtility.UrlDecode(uri.Query);

                var queryParameters = ExtractParameters(query);
                if (queryParameters?.Count > 0)
                {
                    foreach (var parameter in queryParameters)
                    {
                        parameters.Add(parameter);
                    }
                }

                var pageName = indiceQuery > 0 ? decoded.Remove(indiceQuery) : decoded;
                await PushPageAsync(pageName, parameters, animated).ConfigureAwait(false);
            }
        }

        private async Task SwithRootPage(Uri uri, IDictionary<string, StringValues> parameters = null, bool animated = false)
        {
            var application = _applicationProvider.GetApplication();
            var page = ResolveRootPage(uri);

            if (application.MainPage is null)
            {
                application.MainPage = new NavigationPage(page);
            }
            else
            {
                if (application.MainPage is NavigationPage navPage)
                {
                    var currentRoot = navPage.Navigation.NavigationStack[0];
                    navPage.Navigation.InsertPageBefore(page, currentRoot);
                    await navPage.PopToRootAsync(animated).ConfigureAwait(false);
                }
            }

            InvokePageViewModelAction<INavigatedAware>(
                page, (navigatedAware) => Task.Run(() => navigatedAware.OnNavigatedAsync(parameters)));
        }

        private Page ResolveRootPage(Uri uri)
        {
            if (!uri.IsAbsoluteUri)
                return ResolvePage(uri.OriginalString);

            var segment = uri.Segments.First(s => !RELATIVEPATH.Equals(s));
            var decoded = HttpUtility.UrlDecode(segment);

            var indiceQuery = decoded.IndexOf("?") + 1;
            var query = indiceQuery > 0
                ? decoded.Substring(indiceQuery)
                : null;

            var parameters = ExtractParameters(query);
            var pageName = indiceQuery > 0 ? decoded.Remove(indiceQuery) : decoded;
            return ResolvePage(pageName, parameters);
        }

        private async Task PushPageAsync(string pageName, IDictionary<string, StringValues> parameters, bool animate)
        {
            var page = ResolvePage(pageName, parameters);
            var currentPage = _applicationProvider.GetCurrentPage();

            await currentPage.Navigation.PushAsync(page, animate).ConfigureAwait(false);
            InvokePageViewModelAction<INavigatedAware>(
                page, (navigatedAware) => Task.Run(() => navigatedAware.OnNavigatedAsync(parameters)));
        }

        private void InvokePageViewModelAction<TInterface>(Page page, Action<TInterface> invoker)
        {
            if (page is TInterface awarePage)
                invoker.Invoke(awarePage);

            if (page.BindingContext is TInterface awareVm)
                invoker.Invoke(awareVm);
        }

        private Page ResolvePage(string pageName, IDictionary<string, StringValues> parameters = null)
        {
            var pageType = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                            from type in asm.GetTypes()
                            where type.IsClass && type.Name == pageName
                            select type).Single();

            var page = ResolveAndBindPage(pageType);

            ResolveParameters(page, parameters);
            return page;
        }

        private void ResolveParameters(Page page, IDictionary<string, StringValues> parameters)
        {
            if (parameters is null)
                return;

            var vmType = page.BindingContext.GetType();
            foreach (var parameter in parameters)
            {
                var property = vmType.GetProperty(parameter.Key);
                if (property != null)
                {
                    property.SetValue(page.BindingContext, parameter.Value.First());
                }
            }
        }

        private static Uri Parse(string uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            if (uri.StartsWith("/", StringComparison.Ordinal))
                return new Uri("http://localhost" + uri, UriKind.Absolute);

            return new Uri(uri, UriKind.RelativeOrAbsolute);
        }

        private IDictionary<string, StringValues> ExtractParameters(string query)
        {
            if (!string.IsNullOrWhiteSpace(query))
                return QueryHelpers.ParseQuery(query);

            return null;
        }

        private Page ResolveAndBindPage(Type pageType)
        {
            var page = (Page)_resolver.Resolve(pageType);

            if (page.BindingContext is null)
            {
                var vmType = ViewModelLocator.FetchViewModelType(pageType);

                if (vmType != null)
                {
                    page.BindingContext = _resolver.Resolve(vmType);
                }
            }

            return page;
        }

    }
}
