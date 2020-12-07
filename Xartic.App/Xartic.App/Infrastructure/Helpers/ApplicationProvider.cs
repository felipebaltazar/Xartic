using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using Xartic.App.Abstractions;

namespace Xartic.App.Infrastructure.Helpers
{
    public sealed class ApplicationProvider : IApplicationProvider
    {
        public Application GetApplication() =>
            Application.Current;

        public Page GetCurrentPage() =>
            GetCurrentPage(Application.Current.MainPage);

        private static Page GetCurrentPage(Page mainPage)
        {
            var page = mainPage;

            var lastModal = page.Navigation.ModalStack.LastOrDefault();
            if (lastModal != null)
                page = lastModal;

            return GetOnNavigatedToTargetFromChild(page);
        }

        private static Page GetOnNavigatedToTargetFromChild(Page target)
        {
            Page child = null;

            if (target is MasterDetailPage masterDetail)
                child = masterDetail.Detail;
            else if (target is TabbedPage tabbedPage)
                child = tabbedPage.CurrentPage;
            else if (target is CarouselPage carousel)
                child = carousel.CurrentPage;
            else if (target is NavigationPage navPage)
                child = navPage.Navigation.NavigationStack.Last();

            if (child != null)
                target = GetOnNavigatedToTargetFromChild(child);

            return target;
        }
    }
}
