using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WindowsDefender_WebApp.Startup))]
namespace WindowsDefender_WebApp
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            app.MapSignalR();
        }
    }
}
