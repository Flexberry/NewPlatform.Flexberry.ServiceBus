namespace NewPlatform.Flexberry.ServiceBus.Editor
{
    using System;
    using System.Reflection;
    using System.Web;
    using System.Web.Http;
    using System.Web.Security;

    using ICSSoft.Services;
    using ICSSoft.STORMNET;
    using ICSSoft.STORMNET.Security;
    using IIS.Caseberry.Logging.Objects;

    using Microsoft.Practices.Unity;

    using NewPlatform.Flexberry;
    using NewPlatform.Flexberry.AspNet.WebApi.Cors;
    using NewPlatform.Flexberry.ORM.ODataService;
    using NewPlatform.Flexberry.ORM.ODataService.Extensions;
    using NewPlatform.Flexberry.ORM.ODataService.Model;
    using NewPlatform.Flexberry.Security;
    using NewPlatform.Flexberry.Services;

    /// <summary>
    /// Configure OData Service.
    /// </summary>
    internal static class ODataConfig
    {
        /// <summary>
        /// Configure OData by DataObjects assembly.
        /// </summary>
        /// <param name="config">Http configuration object.</param>
        /// <param name="container">Unity container.</param>
        public static void Configure(HttpConfiguration config, IUnityContainer container)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (container == null)
            {
                throw new ArgumentNullException(nameof(container));
            }

            // Enable CORS with SupportsCredentials
            config.EnableCors(new DynamicCorsPolicyProvider(true));

            // Use Unity as WebAPI dependency resolver
            config.DependencyResolver = new UnityDependencyResolver(container);

            // Create EDM model builder
            var assemblies = new[]
            { 
				Assembly.Load("NewPlatform.Flexberry.ServiceBus.Objects"),
                typeof(ApplicationLog).Assembly,
                typeof(UserSetting).Assembly,
                typeof(FlexberryUserSetting).Assembly,
                typeof(Lock).Assembly
			};
            var builder = new DefaultDataObjectEdmModelBuilder(assemblies);
            builder.PropertyFilter = PropertyFilter;

            // Map OData Service
            var token = config.MapODataServiceDataObjectRoute(builder);

            // User functions
            token.Functions.Register(new Func<string>(GetAuthenticatedUser));
            token.Functions.Register(new Func<string, string, bool>(Login));
            token.Functions.Register(new Func<bool>(Logout));

            // Event handlers
            token.Events.CallbackAfterGet = CallbackAfterGet;
        }

        public static void CallbackAfterGet(ref DataObject[] objects)
        {
            foreach (var _object in objects)
            {
                if (_object.GetType() == typeof(Agent))
                {
                    ((Agent)_object).Pwd = null;
                }
            }
        }

        private static bool PropertyFilter(PropertyInfo propertyInfo)
        {
            return Information.ExtractPropertyInfo<Agent>(x => x.Pwd) != propertyInfo;
        }

        private static string GetAuthenticatedUser()
        {
            HttpCookie authCookie = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null)
                return string.Empty;

            FormsAuthenticationTicket ticket = FormsAuthentication.Decrypt(authCookie.Value);
            if (ticket == null)
                return string.Empty;

            return ticket.Name;
        }

        private static bool Login(string login, string password)
        {
            IUserManager userManager = UnityFactory.GetContainer().Resolve<IUserManager>();

            if (userManager.IsUserExist(login, password))
            {
                FormsAuthentication.SetAuthCookie(login, true);
                return true;
            }

            return false;
        }

        private static bool Logout()
        {
            FormsAuthentication.SignOut();
            return true;
        }
    }
}