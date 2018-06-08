using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using MIXUI.Entities;

namespace MIXUI.Helpers
{
    public class WorkspaceAuthorizationHandler : AuthorizationHandler<SameUserRequirement, Workspace>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
                                                   SameUserRequirement requirement,
                                                   Workspace resource)
        {
            if (context.User.HasClaim(Constants.Strings.JwtClaimIdentifiers.Id, resource.IdentityId))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}
