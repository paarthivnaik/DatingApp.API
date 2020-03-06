using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.API.Installers {
    public class AuthenticationInstaller : IInstaller {
        public void InstallServices (IServiceCollection services, IConfiguration Configuration) {
            services.AddAuthentication (JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer (options => {
                    options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey (Encoding.ASCII.GetBytes (Configuration.GetSection ("AppSettings:Token").Value)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                    };
                });
        }
    }
}