using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(BlobStore.Startup))]
namespace BlobStore
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
