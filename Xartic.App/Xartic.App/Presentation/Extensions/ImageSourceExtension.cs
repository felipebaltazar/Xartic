using System;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace Xartic.App.Presentation.Extensions
{
    [ContentProperty(nameof(Source))]
    public class ImageResourceExtension : IMarkupExtension
    {
        #region Properties

        public string Source { get; set; }

        #endregion

        #region IMarkupExtension

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null)
            {
                return null;
            }

            var imageSource = ImageSource.FromResource($"Xartic.App.Resources.Images.{Source}", typeof(ImageResourceExtension).GetTypeInfo().Assembly);
            return imageSource;
        }

        #endregion
    }
}
