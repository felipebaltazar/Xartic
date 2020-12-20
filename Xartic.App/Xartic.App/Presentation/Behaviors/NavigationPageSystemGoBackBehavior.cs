using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xartic.App.Presentation.Extensions;

namespace Xartic.App.Presentation.Behaviors
{
    [Preserve]
    public sealed class NavigationPageSystemGoBackBehavior : BehaviorBase<NavigationPage>
    {
        protected override void OnAttachedTo(NavigationPage bindable)
        {
            bindable.Popped += NavigationPage_Popped;
            base.OnAttachedTo(bindable);
        }

        protected override void OnDetachingFrom(NavigationPage bindable)
        {
            bindable.Popped -= NavigationPage_Popped;
            base.OnDetachingFrom(bindable);
        }

        private void NavigationPage_Popped(object sender, NavigationEventArgs e)
        {
            e.Page.HandleSystemGoBack(AssociatedObject.CurrentPage);
        }
    }
}
