using System;

namespace NewsPaper.Client.Mvc.ViewModels.Base
{
    public abstract class ViewModelBase : IViewModel
    {
        public abstract Guid Id { get; set; }
    }
}