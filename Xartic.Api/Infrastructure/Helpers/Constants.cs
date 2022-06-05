namespace Xartic.Api.Infrastructure.Helpers
{
    public class Constants
    {
        ///<remarks/>
        public static class ApiInfo
        {
            /// <remarks/>
            public const string TITLE = "Xartic SignalR";
            /// <remarks/>
            public const string VERSION = "v1";
            /// <remarks/>
            public const string DESCRIPTION = "SignalR Hub from Xartic game";

            /// <remarks/>
            public static class Contact
            {
                /// <remarks/>
                public const string NAME = "Xartic";
                /// <remarks/>
                public const string URL = "https://github.com/felipebaltazar";
                /// <remarks/>
                public const string EMAIL = "felipe.dasilvabaltazar@gmail.com.br";
            }
        }

        /// <remarks/>
        public static class Documentation
        {
            /// <remarks/>
            public const string ROUTE_PREFIX = "docs";
            /// <remarks/>
            public const string ENDPOINT = "/swagger/v1/swagger.json";
            /// <remarks/>
            public const string CUSTOM_JS = "/swagger-ui/custom.js";
            /// <remarks/>
            public const string CUSTOM_CSS = "/swagger-ui/custom.css";
        }
    }
}
