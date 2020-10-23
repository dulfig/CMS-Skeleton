using System.Linq;
using System.Threading.Tasks;
using FunctService;

namespace DataService
{
    public static class DbContextInitializer
    {
        public static async Task Initialize(DataProtectionKeyContext dPKey, AppDbContext appDbContext, IFunctServ functServ)
        {
            //Check if the DataProtectionKeyContext and AppDbContext has been created previously
            await dPKey.Database.EnsureCreatedAsync();
            await appDbContext.Database.EnsureCreatedAsync();
            //Check if db has any users else initialize the db with Admin and Client User
            if (appDbContext.AppUsers.Any())
            {
                return;
            }
            await functServ.CreateDefaultAdmin();
            await functServ.CreateDefaultUser();
        }
    }
}
