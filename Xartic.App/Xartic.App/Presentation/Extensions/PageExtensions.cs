using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xartic.App.Abstractions.Navigation;
using Xartic.App.Infrastructure.Helpers;

namespace Xartic.App.Presentation.Extensions
{
    public static class PageExtensions
    {
        public static void HandleSystemGoBack(this Page previousPage, Page currentPage)
        {
            var parameters = new Dictionary<string, StringValues>();
            parameters.Add("NavigationMode", "Back");
            OnNavigatedFrom(previousPage, parameters);
            OnNavigatedTo(currentPage.GetOnNavigatedToTargetFromChild(), parameters);
            previousPage.DestroyPage();
        }

        public static void OnNavigatedFrom(Page page, IDictionary<string, StringValues> parameters)
        {
            if (page != null)
                _ = Task.Run(() => InvokeViewAndViewModelActionAsync<INavigatedFromAware>(page, v => v.OnNavigatedFromAsync(parameters)));
        }

        public static void OnNavigatedTo(Page page, IDictionary<string, StringValues> parameters)
        {
            if (page != null)
                _ = Task.Run(() => InvokeViewAndViewModelActionAsync<INavigatedAware>(page, v => v.OnNavigatedAsync(parameters)));
        }

        public static async Task InvokeViewAndViewModelActionAsync<T>(this BindableObject view, Func<T, Task> action) where T : class
        {
            if (view is T viewAsT)
            {
                await action(viewAsT);
            }

            if (view is BindableObject element && element.BindingContext is T viewModelAsT)
            {
                await action(viewModelAsT);
            }
        }

        public static void InvokeViewAndViewModelAction<T>(this BindableObject view, Action<T> action) where T : class
        {
            if (view is T viewAsT)
            {
                action(viewAsT);
            }

            if (view is BindableObject element && element.BindingContext is T viewModelAsT)
            {
                action(viewModelAsT);
            }
        }

        public static void DestroyPage(this Page page)
        {
            try
            {
                page.DestroyChildren();
                InvokeViewAndViewModelAction<IDisposable>(page, v => v.Dispose());

                page.BindingContext = null;
                page.Behaviors?.Clear();
            }
            catch (Exception ex)
            {
                throw new Exception($"Cannot destroy {page}.", ex);
            }
        }

        private static void DestroyChildren(this Page page)
        {
            switch (page)
            {
                case MasterDetailPage mdp:
                    DestroyPage(mdp.Master);
                    DestroyPage(mdp.Detail);
                    break;
                case TabbedPage tabbedPage:
                    foreach (var item in tabbedPage.Children.Reverse())
                    {
                        DestroyPage(item);
                    }
                    break;
                case CarouselPage carouselPage:
                    foreach (var item in carouselPage.Children.Reverse())
                    {
                        DestroyPage(item);
                    }
                    break;
                case NavigationPage navigationPage:
                    foreach (var item in navigationPage.Navigation.NavigationStack.Reverse())
                    {
                        DestroyPage(item);
                    }
                    break;
            }
        }

        public static Page GetCurrentPage(this Page mainPage)
        {
            var page = mainPage;

            var lastModal = page.Navigation.ModalStack.LastOrDefault();
            if (lastModal != null)
                page = lastModal;

            return GetOnNavigatedToTargetFromChild(page);
        }

        public static Page GetOnNavigatedToTargetFromChild(this Page target)
        {
            Page child = null;

            if (target is MasterDetailPage)
                child = ((MasterDetailPage)target).Detail;
            else if (target is TabbedPage)
                child = ((TabbedPage)target).CurrentPage;
            else if (target is CarouselPage)
                child = ((CarouselPage)target).CurrentPage;
            else if (target is NavigationPage)
                child = target.Navigation.NavigationStack.Last();

            if (child != null)
                target = GetOnNavigatedToTargetFromChild(child);

            return target;
        }
    }
}
