using DatingApp.API.Data;
using DatingApp.API.Helpers;
using DatingApp.API.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.API.Installers
{
    public class RepositoryInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration Configuration)
        {
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IDatingRepository,DatingRepository>();
            
        }
    }
}