using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using ModelService;
using Serilog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace FunctService
{
    public class FunctServ : IFunctServ
    {
        private readonly AdminUO adminUO;
        private readonly AppUO appUO;
        private readonly UserManager<AppUser> userManager;
        public FunctServ(IOptions<AppUO> aUO, IOptions<AdminUO> adUO, UserManager<AppUser> uManager)
        {
            adminUO = adUO.Value;
            appUO = aUO.Value;
            userManager = uManager;
        }

        public async Task CreateDefaultAdmin()
        {
            try
            {
                var adminUser = new AppUser
                {
                    Email = adminUO.Email,
                    UserName = adminUO.Username,
                    EmailConfirmed = true,
                    ProfilePic = GetDefaultProfilePic(),
                    PhoneNumber = "8529208430",
                    PhoneNumberConfirmed = true,
                    FirstName = adminUO.FName,
                    LastName = adminUO.LName,
                    Role = "Administrator",
                    IsActive = true,
                    UserAdresses = new List<AdressModel>
                    {
                        new AdressModel {Country = adminUO.Country, Type = "Billing"},
                        new AdressModel {Country = adminUO.Country, Type = "Shipping"}

                    }
                };
                var res = await userManager.CreateAsync(adminUser, adminUO.Password);
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Administrator");
                    Log.Information("Admin user created {UserName}", adminUser.UserName);
                }
                else
                {
                    var errors = string.Join(",", res.Errors);
                    Log.Error("Error while creating user {Error}", errors);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error while creating user {Error} {StackTrace} {InnerException} {Source}",
                    e.Message, e.StackTrace, e.InnerException, e.Source);
            }
        }
        public async Task CreateDefaultUser()
        {
            try
            {
                var appUser = new AppUser
                {
                    Email = appUO.Email,
                    UserName = appUO.Username,
                    EmailConfirmed = true,
                    ProfilePic = GetDefaultProfilePic(),
                    PhoneNumber = "8529208430",
                    PhoneNumberConfirmed = true,
                    FirstName = appUO.FName,
                    LastName = appUO.LName,
                    Role = "Customer",
                    IsActive = true,
                    UserAdresses = new List<AdressModel>
                    {
                        new AdressModel {Country = appUO.Country, Type = "Billing"},
                        new AdressModel {Country = appUO.Country, Type = "Shipping"}

                    }
                };
                var res = await userManager.CreateAsync(appUser, appUO.Password);
                if (res.Succeeded)
                {
                    await userManager.AddToRoleAsync(appUser, "Administrator");
                    Log.Information("Admin user created {UserName}", appUser.UserName);
                }
                else
                {
                    var errors = string.Join(",", res.Errors);
                    Log.Error("Error while creating user {Error}", errors);
                }
            }
            catch (Exception e)
            {
                Log.Error("Error while creating user {Error} {StackTrace} {InnerException} {Source}",
                    e.Message, e.StackTrace, e.InnerException, e.Source);
            }
        }

        private string GetDefaultProfilePic()
        {
            return "";
        }
    }

}
