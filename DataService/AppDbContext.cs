using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using ModelService;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace DataService
{
    public class AppDbContext : IdentityDbContext<AppUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<IdentityRole>().HasData(
                    new { Id = "1", Name = "Administrator", NormalizedName = "ADMINISTRATOR", RoleName = "Admin", Handle = "admin", RoleIcon = "/uploads/roles/icons/default/role.png", IsActive = true},
                    new { Id = "2", Name = "Customer", NormalizedName = "CUSTOMER", RoleName = "customer", Handle = "customer", RoleIcon = "/uploads/roles/icons/default/role.png", IsActive = true}
                );
        }
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<AdressModel> Adresses { get; set; }

    }
}
