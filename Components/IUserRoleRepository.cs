using Lemorange.Modules.FinHubAddOns.Models;
using System;
using System.Collections.Generic;

namespace Lemorange.Modules.FinHubAddOns.Components
{
    public interface IUserRoleRepository
    {
        List<ServiceProviderUser> GetServiceProviderUsers(int portalId);
        bool UpdateRoleExpiration(int userId, string roleName, DateTime? expirationDate, int portalId, int modifiedByUserId);
        bool RemoveUserFromRole(int userId, string roleName, int portalId, int modifiedByUserId);
    }
}