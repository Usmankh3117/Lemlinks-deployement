using LimLink_API.DBHelper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LimLink_API
{
    public class Startup
    {
        //readonly string MyAllowSpecificOrigins = "AllowOrigin";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddCors();
            services.AddControllers();
            services.AddMvc();

            //services.AddCors(options =>
            //{
            //    options.AddPolicy(name: MyAllowSpecificOrigins,
            //                      builder =>
            //                      {
            //                          builder.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin();
            //                      });
            //});

            // For Entity Framework
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // For Identity
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //services.Configure<ForwardedHeadersOptions>(options =>
            //{
            //    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
            //    options.KnownNetworks.Clear();
            //    options.KnownProxies.Clear();
            //});

            services.AddControllersWithViews();
            services.AddRazorPages();
            //Authorization settings.  
            services.AddMvc().AddRazorPagesOptions(options =>
            {
                //options.Conventions.AuthorizeFolder("/");
                options.Conventions.AllowAnonymousToPage("/Index");
            });
            // Adding Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // Adding Jwt Bearer
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                };
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseStaticFiles();
            app.UseCors(x => x.AllowAnyMethod().AllowAnyHeader().SetIsOriginAllowed(origin => true).AllowCredentials());
            app.UseHsts();
            app.UseForwardedHeaders();

            app.UseHttpsRedirection();
            StripeConfiguration.ApiKey = Configuration.GetSection("StripeURLs").GetSection("APIKey").Value;
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
