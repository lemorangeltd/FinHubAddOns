using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Framework;
using DotNetNuke.Security.Roles;
using DotNetNuke.Entities.Users;
using Lemorange.Modules.FinHubAddOns.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Lemorange.Modules.FinHubAddOns.Components
{
    public class UserRoleController : ServiceLocator<IUserRoleRepository, UserRoleRepository>, IUserRoleRepository
    {
        protected override Func<IUserRoleRepository> GetFactory()
        {
            return () => new UserRoleRepository();
        }

        public List<ServiceProviderUser> GetServiceProviderUsers(int portalId)
        {
            var users = new List<ServiceProviderUser>();

            using (var dr = DataProvider.Instance().ExecuteReader("FinHubAddOns_GetServiceProviderUsers", portalId))
            {
                while (dr.Read())
                {
                    users.Add(new ServiceProviderUser
                    {
                        // Basic User Info
                        UserId = Convert.ToInt32(dr["UserID"]),
                        Username = dr["Username"] != DBNull.Value ? dr["Username"].ToString() : string.Empty,
                        FirstName = dr["FirstName"] != DBNull.Value ? dr["FirstName"].ToString() : string.Empty,
                        LastName = dr["LastName"] != DBNull.Value ? dr["LastName"].ToString() : string.Empty,
                        Email = dr["Email"] != DBNull.Value ? dr["Email"].ToString() : string.Empty,
                        DisplayName = dr["DisplayName"] != DBNull.Value ? dr["DisplayName"].ToString() : string.Empty,

                        // Role Information
                        RoleStartDate = dr["RoleStartDate"] != DBNull.Value ? Convert.ToDateTime(dr["RoleStartDate"]) : (DateTime?)null,
                        RoleExpirationDate = dr["RoleExpirationDate"] != DBNull.Value ? Convert.ToDateTime(dr["RoleExpirationDate"]) : (DateTime?)null,
                        IsRoleOwner = dr["IsRoleOwner"] != DBNull.Value && Convert.ToBoolean(dr["IsRoleOwner"]),

                        // Account Status & Dates
                        AccountCreated = Convert.ToDateTime(dr["AccountCreated"]),
                        AccountLastModified = dr["AccountLastModified"] != DBNull.Value ? Convert.ToDateTime(dr["AccountLastModified"]) : (DateTime?)null,
                        PortalJoinDate = dr["PortalJoinDate"] != DBNull.Value ? Convert.ToDateTime(dr["PortalJoinDate"]) : (DateTime?)null,
                        IsAuthorized = dr["IsAuthorized"] != DBNull.Value && Convert.ToBoolean(dr["IsAuthorized"]),
                        IsSuperUser = dr["IsSuperUser"] != DBNull.Value && Convert.ToBoolean(dr["IsSuperUser"]),

                        // Address Information
                        Street = dr["Street"] != DBNull.Value ? dr["Street"].ToString() : string.Empty,
                        City = dr["City"] != DBNull.Value ? dr["City"].ToString() : string.Empty,
                        StateRegion = dr["State_Region"] != DBNull.Value ? dr["State_Region"].ToString() : string.Empty,
                        PostalCode = dr["PostalCode"] != DBNull.Value ? dr["PostalCode"].ToString() : string.Empty,
                        Country = dr["Country"] != DBNull.Value ? dr["Country"].ToString() : string.Empty,

                        // Contact Info
                        Phone = dr["Phone"] != DBNull.Value ? dr["Phone"].ToString() : string.Empty,
                        Mobile = dr["Mobile"] != DBNull.Value ? dr["Mobile"].ToString() : string.Empty,

                        // Calculated Fields
                        DaysInRole = dr["DaysInRole"] != DBNull.Value ? Convert.ToInt32(dr["DaysInRole"]) : 0,
                        OtherRoles = dr["OtherRoles"] != DBNull.Value ? dr["OtherRoles"].ToString() : string.Empty
                    });
                }
            }

            return users;
        }

        public bool UpdateRoleExpiration(int userId, string roleName, DateTime? expirationDate, int portalId, int modifiedByUserId)
        {
            try
            {
                // Get the role
                var roleController = new RoleController();
                var role = roleController.GetRoleByName(portalId, roleName);

                if (role == null)
                    return false;

                // Get the user
                var user = UserController.GetUserById(portalId, userId);
                if (user == null)
                    return false;

                // Get the user role
                var userRole = roleController.GetUserRole(portalId, userId, role.RoleID);
                if (userRole == null)
                    return false;

                // Update the expiration date using SQL directly
                var expiryDate = expirationDate ?? DateTime.MaxValue;
                DataProvider.Instance().ExecuteNonQuery(
                    "UPDATE UserRoles SET ExpiryDate = @0, LastModifiedOnDate = GETDATE(), LastModifiedByUserID = @1 WHERE UserID = @2 AND RoleID = @3",
                    expiryDate, modifiedByUserId, userId, role.RoleID);

                // Clear the cache
                DataCache.RemoveCache("UserRoles");

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool RemoveUserFromRole(int userId, string roleName, int portalId, int modifiedByUserId)
        {
            try
            {
                // Get the role
                var roleController = new RoleController();
                var role = roleController.GetRoleByName(portalId, roleName);

                if (role == null)
                    return false;

                // Get the user
                var user = UserController.GetUserById(portalId, userId);
                if (user == null)
                    return false;

                // Get portal settings
                var portalSettings = new DotNetNuke.Entities.Portals.PortalSettings(portalId);

                // Remove the user from the role
                RoleController.DeleteUserRole(user, role, portalSettings, false);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}