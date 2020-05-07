using DatingApp.API.Data;
using DatingApp.API.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.API.Installers
{
    public class DbInstaller : IInstaller
    {
        public void InstallServices(IServiceCollection services, IConfiguration Configuration)
        {
           services.AddDbContext<DataContext>(options=>options.UseSqlite(Configuration.GetConnectionString("DefaultConnection")));
        }
    }
}