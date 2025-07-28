using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using Lemorange.Modules.FinHubAddOns.Models;
using Lemorange.Modules.FinHubAddOns.Components;
using DotNetNuke.Security;
using DotNetNuke.Entities.Users;

namespace Lemorange.Modules.FinHubAddOns.Services
{
    public class UserRoleController : DnnApiController
    {
        private readonly IUserRoleRepository _repository;

        public UserRoleController()
        {
            _repository = new UserRoleRepository();

            //// Configure JSON serialization to use camelCase
            //var formatter = GlobalConfiguration.Configuration.Formatters.JsonFormatter;
            //formatter.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();

        }

        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage GetServiceProviders()
        {
            try
            {
                var serviceProviders = _repository.GetServiceProviderUsers(ActiveModule.PortalID);
                return Request.CreateResponse(HttpStatusCode.OK, serviceProviders);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage UpdateRoleExpiration(UpdateRoleExpirationRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Request is null");
                }

                if (request.UserId <= 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid UserId");
                }

                // Check if ActiveModule is available
                if (ActiveModule == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Module context not available");
                }

                var portalId = ActiveModule.PortalID;
                var currentUser = UserController.Instance.GetCurrentUserInfo();

                var success = _repository.UpdateRoleExpiration(
                    request.UserId,
                    "Service Provider",
                    request.ExpirationDate,
                    portalId,
                    currentUser.UserID
                );

                if (success)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Failed to update role expiration - check if user exists and has the Service Provider role");
                }
            }
            catch (Exception ex)
            {
                // Log the full exception
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, $"Error: {ex.Message}");
            }
        }

        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage RemoveUserFromRole(RemoveUserFromRoleRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid request");
                }

                var currentUser = UserController.Instance.GetCurrentUserInfo();
                var success = _repository.RemoveUserFromRole(
                    request.UserId,
                    "Service Provider",
                    ActiveModule.PortalID,
                    currentUser.UserID
                );

                if (success)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Failed to remove user from role");
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }

    // Request DTOs
    public class UpdateRoleExpirationRequest
    {
        public int UserId { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }

    public class RemoveUserFromRoleRequest
    {
        public int UserId { get; set; }
    }
}