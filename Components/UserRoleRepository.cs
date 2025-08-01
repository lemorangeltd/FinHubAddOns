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
    public class UserRoleRepository : ServiceLocator<IUserRoleRepository, UserRoleRepository>, IUserRoleRepository
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
                        IsDeleted = dr["IsDeleted"] != DBNull.Value && Convert.ToBoolean(dr["IsDeleted"]),

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
                        OtherRoles = dr["OtherRoles"] != DBNull.Value ? dr["OtherRoles"].ToString() : string.Empty,

                        // Payment Information
                        PaymentStatus = dr["PaymentStatus"] != DBNull.Value ? dr["PaymentStatus"].ToString() : "Unpaid",
                        SubscriptionEndDate = dr["SubscriptionEndDate"] != DBNull.Value ? Convert.ToDateTime(dr["SubscriptionEndDate"]) : (DateTime?)null,
                        CurrentPlan = dr["CurrentPlan"] != DBNull.Value ? dr["CurrentPlan"].ToString() : string.Empty,

                        // Rating Information
                        AverageRating = dr["AverageRating"] != DBNull.Value ? Convert.ToDecimal(dr["AverageRating"]) : 0m,

                        // ===== EXTENDED COMPANY INFORMATION =====
                        CompanyName = dr["CompanyName"] != DBNull.Value ? dr["CompanyName"].ToString() : string.Empty,
                        CompanyNumber = dr["CompanyNumber"] != DBNull.Value ? dr["CompanyNumber"].ToString() : string.Empty,
                        CompanyTurnover = dr["CompanyTurnover"] != DBNull.Value ? dr["CompanyTurnover"].ToString() : string.Empty,
                        CompanyIndustry = dr["CompanyIndustry"] != DBNull.Value ? dr["CompanyIndustry"].ToString() : string.Empty,
                        CompanyType = dr["CompanyType"] != DBNull.Value ? dr["CompanyType"].ToString() : string.Empty,
                        CompanyJurisdiction = dr["CompanyJurisdiction"] != DBNull.Value ? dr["CompanyJurisdiction"].ToString() : string.Empty,
                        CompanySize = dr["CompanySize"] != DBNull.Value ? dr["CompanySize"].ToString() : string.Empty,
                        YearOfEstablishment = dr["YearOfEstablishment"] != DBNull.Value ? dr["YearOfEstablishment"].ToString() : string.Empty,

                        // ===== EXTENDED PERSONAL INFORMATION =====
                        Prefix = dr["Prefix"] != DBNull.Value ? dr["Prefix"].ToString() : string.Empty,
                        MiddleName = dr["MiddleName"] != DBNull.Value ? dr["MiddleName"].ToString() : string.Empty,
                        PositionInCompany = dr["PositionInCompany"] != DBNull.Value ? dr["PositionInCompany"].ToString() : string.Empty,

                        // ===== SERVICES & BUSINESS INFORMATION =====
                        OurServices = dr["OurServices"] != DBNull.Value ? dr["OurServices"].ToString() : string.Empty,
                        AboutYourServices = dr["AboutYourServices"] != DBNull.Value ? dr["AboutYourServices"].ToString() : string.Empty,
                        AboutUs = dr["AboutUs"] != DBNull.Value ? dr["AboutUs"].ToString() : string.Empty,
                        FeesStructure = dr["FeesStructure"] != DBNull.Value ? dr["FeesStructure"].ToString() : string.Empty,
                        Publications = dr["Publications"] != DBNull.Value ? dr["Publications"].ToString() : string.Empty,

                        // ===== EXTENDED CONTACT INFORMATION =====
                        Fax = dr["Fax"] != DBNull.Value ? dr["Fax"].ToString() : string.Empty,
                        Website = dr["Website"] != DBNull.Value ? dr["Website"].ToString() : string.Empty,
                        Twitter = dr["Twitter"] != DBNull.Value ? dr["Twitter"].ToString() : string.Empty,

                        // ===== MEDIA & DOCUMENTS =====
                        Photo = dr["Photo"] != DBNull.Value ? dr["Photo"].ToString() : string.Empty,
                        Biography = dr["Biography"] != DBNull.Value ? dr["Biography"].ToString() : string.Empty
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

                // Get the user role info
                var userRole = roleController.GetUserRole(portalId, userId, role.RoleID);
                if (userRole == null)
                    return false;

                // Create updated user role info
                var userRoleInfo = new UserRoleInfo
                {
                    UserID = userId,
                    RoleID = role.RoleID,
                    RoleName = role.RoleName,
                    PortalID = portalId,
                    EffectiveDate = userRole.EffectiveDate,
                    ExpiryDate = expirationDate ?? DateTime.MaxValue,
                    IsOwner = userRole.IsOwner,
                    Status = RoleStatus.Approved
                };

                // Remove and re-add the role with new expiration
                var portalSettings = new DotNetNuke.Entities.Portals.PortalSettings(portalId);
                RoleController.DeleteUserRole(user, role, portalSettings, false);
                RoleController.AddUserRole(user, role, portalSettings, RoleStatus.Approved, userRole.EffectiveDate, expirationDate ?? DateTime.MaxValue, false, false);

                return true;
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
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