namespace Xartic.Api.Infrastructure.Helpers
{
    public class Constants
    {
        ///<remarks/>
        public static class ApiInfo
        {
            /// <remarks/>
            public const string TITLE = "Api Xartic";
            /// <remarks/>
            public const string VERSION = "v1";
            /// <remarks/>
            public const string DESCRIPTION = "Api REST, ASP.NET Core, de controle e manipulação de informações do aplicativo Xartic ";

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
