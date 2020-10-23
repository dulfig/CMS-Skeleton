using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using LoggingService;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Sinks.SystemConsole.Themes;
using DataService;
using FunctService;

namespace CMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using(var scope = host.Services.CreateScope())
            {

                var services = scope.ServiceProvider;
                try
                {
                    //DbContext with services
                    var context = services.GetRequiredService<AppDbContext>();
                    var dpContext = services.GetRequiredService<DataProtectionKeyContext>();
                    var functContext = services.GetRequiredService<IFunctServ>();
                    DbContextInitializer.Initialize(dpContext, context, functContext).Wait();

                }
                catch (Exception e)
                {
                    Log.Error("An error has occurred {Error} {StackTrace} {InnerException} {Source}",
                        e.Message, e.StackTrace, e.InnerException, e.Source);
                }
                //Testing for logging
                /*try
                {
                    int zero = 0;
                    int result = 100 / zero;
                }
                catch (DivideByZeroException e)
                {
                    //Calls Serilog to print out this log message
                    Log.Error("An error has occurred {Error} {StackTrace} {InnerException} {Source}",
                        e.Message, e.StackTrace, e.InnerException, e.Source);
                }*/
            }

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //Log context configuration
                    webBuilder.UseSerilog((hostingContext, loggingConfiguration)=>loggingConfiguration
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", "CMS_Skeleton")
                    .Enrich.WithProperty("MachineName", Environment.MachineName)
                    .Enrich.WithProperty("CurrentManagedThreadID", Environment.CurrentManagedThreadId)
                    .Enrich.WithProperty("OSVersion", Environment.OSVersion)
                    .Enrich.WithProperty("Version", Environment.Version)
                    .Enrich.WithProperty("UserName", Environment.UserName)
                    .Enrich.WithProperty("ProcessID", Process.GetCurrentProcess().Id)
                    .Enrich.WithProperty("ProcessName", Process.GetCurrentProcess().ProcessName)
                    //Creates a new path where the project is called Logs and stores the file containing the issues which can be used across all platforms
                    .WriteTo.File(formatter: new TextFormatter(), path: Path.Combine(hostingContext.HostingEnvironment.ContentRootPath + $"{Path.DirectorySeparatorChar}Logs{Path.DirectorySeparatorChar}", $"load_error_{DateTime.Now:yyyy-MM-dd}.txt"))
                    .ReadFrom.Configuration(hostingContext.Configuration));
                    webBuilder.UseStartup<Startup>();
                });
    }
}
