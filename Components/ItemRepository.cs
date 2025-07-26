using DotNetNuke.Framework;
using System;

namespace Lemorange.Modules.FinHubAddOns.Components
{
    public class ItemRepository : ServiceLocator<IItemRepository, ItemRepository>, IItemRepository
    {
        protected override Func<IItemRepository> GetFactory()
        {
            return () => new ItemRepository();
        }
    }
}