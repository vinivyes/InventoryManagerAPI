using InventoryManagerAPI.Authorization;
using InventoryManagerAPI.Context;
using InventoryManagerAPI.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace InventoryManagerAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //Add database context
            services.AddDbContext<InventoryContext>(options => options.UseNpgsql(Configuration.GetConnectionString("InventoryContext")));

            //Configure authentication service via JWT
            services.AddAuthentication(auth =>
            {
                auth.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                auth.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                //Load parameters from App Settings
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = Configuration.GetValue<bool>("Jwt:ValidateIssuer"),
                    ValidIssuer = Configuration.GetValue<string>("Jwt:Issuer"),
                    ValidateAudience = Configuration.GetValue<bool>("Jwt:ValidateAudience"),
                    ValidAudience = Configuration.GetValue<string>("Jwt:Audience"),
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetValue<string>("Jwt:Key")))
                };
            });

            // Register IHttpContextAccessor
            services.AddHttpContextAccessor();

            // Register ActionAuthorizationHandler
            services.AddScoped<IAuthorizationHandler, ActionAuthorizationHandler>();

            // Register Authorization policy
            services.AddAuthorization(options =>
            {
                options.AddPolicy("ActionPolicy", policy =>
                {
                    policy.RequireAuthenticatedUser();
                    policy.Requirements.Add(new ActionAuthorizationRequirement());
                });
            });


            //Register Service to handle user authorization
            services.AddScoped<UserAuthorizationService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
