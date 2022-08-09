using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Martic.Presentation.Extensions
{
    internal static class PageExtensions
    {
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

            if (target is FlyoutPage)
                child = ((FlyoutPage)target).Detail;
            else if (target is TabbedPage)
                child = ((TabbedPage)target).CurrentPage;
            //else if (target is CarouselPage)
            //    child = ((CarouselPage)target).CurrentPage;
            else if (target is NavigationPage)
                child = target.Navigation.NavigationStack.Last();

            if (child != null)
                target = GetOnNavigatedToTargetFromChild(child);

            return target;
        }
    }
}
