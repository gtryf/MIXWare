using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MIXUI.Entities;

namespace MIXUI.Helpers
{
    public class UserAuthorizationHandler : AuthorizationHandler<SameUserRequirement, AppUser>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   SameUserRequirement requirement,
                                                   AppUser resource)
        {
            if (context.User.HasClaim(Constants.Strings.JwtClaimIdentifiers.Id, resource.Id))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
