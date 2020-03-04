using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.API
{
    public interface IInstaller
    {
         void InstallServices(IServiceCollection services,IConfiguration Configuration);
    }
}