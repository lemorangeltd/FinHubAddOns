using DotNetNuke.Common;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;
using Lemorange.Modules.FinHubAddOns.Components;

namespace Lemorange.Modules.FinHubAddOns.Services
{
    [SupportedModules("FinHubAddOns")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    public class ItemController : DnnApiController
    {
        private readonly IItemRepository _repository;

        public ItemController(IItemRepository repository)
        {
            Requires.NotNull(repository);

            _repository = repository;
        }

        public ItemController() : this(ItemRepository.Instance)
        {
        }
    }
}
