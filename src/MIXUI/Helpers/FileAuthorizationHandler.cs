using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MIXUI.Entities;

namespace MIXUI.Helpers
{
    public class FileAuthorizationHandler : AuthorizationHandler<SameUserRequirement, File>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   SameUserRequirement requirement,
                                                   File resource)
        {
            if (context.User.HasClaim(Constants.Strings.JwtClaimIdentifiers.Id, resource.Workspace.IdentityId))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
