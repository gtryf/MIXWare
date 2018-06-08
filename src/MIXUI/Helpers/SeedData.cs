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
                await EnsureUser(serviceProvider, testUserPw, "user1", "user1@mixware.com", false);
                await EnsureUser(serviceProvider, testUserPw, "user2", "user2@mixware.com", false);
            }
        }

        private static async Task<string> EnsureUser(IServiceProvider serviceProvider,
                                                    string testUserPw, string username, string email, bool isAdmin)
        {
            var userManager = serviceProvider.GetService<UserManager<AppUser>>();

            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                user = new AppUser { UserName = username, Email = email, IsEnabled = true };
                await userManager.CreateAsync(user, testUserPw);
            }

            return user.Id;
        }

        #endregion
    }
}
