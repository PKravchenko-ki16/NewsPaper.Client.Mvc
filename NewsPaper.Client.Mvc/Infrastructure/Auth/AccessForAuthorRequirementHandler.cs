using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace NewsPaper.Client.Mvc.Infrastructure.Auth
{
    public class AccessForAuthorRequirementHandler : AuthorizationHandler<AccessForAuthorRequirement>
    {
        /// <summary>
        /// Makes a decision if authorization is allowed based on a specific requirement.
        /// </summary>
        /// <param name="context">The authorization context.</param>
        /// <param name="requirement">The requirement to evaluate.</param>
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context,
            AccessForAuthorRequirement requirement)
        {
            var hasClaim = context.User.HasClaim(x => x.Type == ClaimTypes.Role);
            if (!hasClaim)
            {
                return Task.CompletedTask;
            }

            var roleValue = context.User.FindFirst(x => x.Type == ClaimTypes.Role).Value;
            if (roleValue == requirement.Role)
            {
                context.Succeed(requirement);

            }

            return Task.CompletedTask;
        }
    }
}