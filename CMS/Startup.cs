using DataService;
using FunctService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelService;
using System;

namespace CMS
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
            services.AddControllersWithViews();
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            //----------------Configuring Database Service---------------------
            //Migrating AppDbContext
            services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("CMS_Dev"), x => x.MigrationsAssembly("CMS")));
            //Migrating DataProtectionKeyContext
            services.AddDbContext<DataProtectionKeyContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DataProtectionKeyContext"), x => x.MigrationsAssembly("CMS")));
            //----------------Configuring Functional Service---------------------
            //Creates new Functional Service per request
            services.AddTransient<IFunctServ, FunctServ>();
            //Load default users from appsettings.json
            services.Configure<AppUO>(Configuration.GetSection("AppUO"));
            services.Configure<AdminUO>(Configuration.GetSection("AdminUO"));
            //----------------Configuring Identity Options---------------------
            //Defines properties for user creation
            var identityDefaultConfig = Configuration.GetSection("IdentityDefaultOp");
            services.Configure<IdentityDefaultOp>(identityDefaultConfig);
            var identityDefaultOp = identityDefaultConfig.Get<IdentityDefaultOp>();
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = identityDefaultOp.PwdReqDigit;
                options.Password.RequiredLength = identityDefaultOp.PwdReqLength;
                options.Password.RequireNonAlphanumeric = identityDefaultOp.PwdReqNonAlphanumeric;
                options.Password.RequireUppercase = identityDefaultOp.PwdReqUpperCase;
                options.Password.RequireLowercase = identityDefaultOp.PwdReqLowerCase;
                options.Password.RequiredUniqueChars = identityDefaultOp.PwdReqUniqueChars;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(identityDefaultOp.LockoutDefaultLockoutMinutes);
                options.Lockout.MaxFailedAccessAttempts = identityDefaultOp.LockoutMaxFailedAttempts;
                options.Lockout.AllowedForNewUsers = identityDefaultOp.LockoutAllowedForNewUser;
                options.User.RequireUniqueEmail = identityDefaultOp.UserReqUniqueEmail;
                options.SignIn.RequireConfirmedEmail = identityDefaultOp.SignInReqConfirmEmail;
            }).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller}/{action=Index}/{id?}");
            });

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    spa.UseAngularCliServer(npmScript: "start");
                }
            });
        }
    }
}
