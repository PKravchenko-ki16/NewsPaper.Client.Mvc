using System;
using NewsPaper.Client.Mvc.ViewModels.Base;

namespace NewsPaper.Client.Mvc.ViewModels.User
{
    public class UserViewModel : ViewModelBase
    {
        public override Guid Id { get; set; }

        public Guid IdentityGuid { get; set; }

        public string NikeName { get; set; }
    }
}
