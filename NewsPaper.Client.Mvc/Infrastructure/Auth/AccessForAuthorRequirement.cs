using Microsoft.AspNetCore.Authorization;

namespace NewsPaper.Client.Mvc.Infrastructure.Auth
{
    public class AccessForAuthorRequirement : IAuthorizationRequirement
    {
        public string Role { get; }

        public AccessForAuthorRequirement()
        {
            Role = "Author";
        }
    }
}