using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

namespace Xartic.App.Infrastructure.MVVM
{
    public static class ViewModelLocator
    {
        private static readonly Dictionary<Type, Type> _viewModelMapping = new Dictionary<Type, Type>();

        public static void MapViewModel<TView, TViewModel>()
        {
            _viewModelMapping.Add(typeof(TView), typeof(TViewModel));
        }

        public static Type FetchViewModelType(Type viewType)
        {
            if (_viewModelMapping.TryGetValue(viewType, out var viewModelType))
                return viewModelType;

            var viewName = viewType.FullName.Replace(".Views.", ".ViewModels.");
            var viewAssemblyName = viewType.GetTypeInfo().Assembly.FullName;
            var viewModelName = string.Format(
                CultureInfo.InvariantCulture, "{0}ViewModel, {1}", viewName, viewAssemblyName);

            return Type.GetType(viewModelName);
        }
    }
}
