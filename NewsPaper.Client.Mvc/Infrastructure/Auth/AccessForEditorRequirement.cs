using Microsoft.AspNetCore.Authorization;

namespace NewsPaper.Client.Mvc.Infrastructure.Auth
{
    public class AccessForEditorRequirement : IAuthorizationRequirement
    {
        public string Role { get; }

        public AccessForEditorRequirement()
        {
            Role = "Editor";
        }
    }
}