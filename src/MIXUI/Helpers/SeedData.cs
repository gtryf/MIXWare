using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MIXUI.Entities;

namespace MIXUI.Helpers
{
    public static class SeedData
    {
        #region snippet_Initialize
        public static async Task Initialize(IServiceProvider serviceProvider, string testUserPw)
        {
            using (var context = new DataContext(
                serviceProvider.GetRequiredService<DbContextOptions<DataContext>>()))
            {
                var adminID = await EnsureUser(serviceProvider, testUserPw, "power", "admin@mixware.com", true);
                var uid = await EnsureUser(serviceProvider, testUserPw, "george", "user@mixware.com", false);
            }
        }

        private static async Task<string> EnsureUser(IServiceProvider serviceProvider,
                                                    string testUserPw, string username, string email, bool isAdmin)
        {
            var userManager = serviceProvider.GetService<UserManager<AppUser>>();

            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                user = new AppUser { UserName = username, Email = email, IsAdministrator = isAdmin };
                await userManager.CreateAsync(user, testUserPw);
            }

            return user.Id;
        }

        #endregion
    }
}
