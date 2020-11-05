using System;
using NewsPaper.Client.Mvc.ViewModels.Base;

namespace NewsPaper.Client.Mvc.ViewModels.Editor
{
    public class EditorViewModel : ViewModelBase
    {
        public override Guid Id { get; set; }

        public Guid IdentityGuid { get; set; }

        public string NikeName { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public int CountСompletedArticles { get; set; }

        public int CountUnderRevisionArticles { get; set; }
    }
}
