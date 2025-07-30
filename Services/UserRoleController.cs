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
using System.Linq;

namespace Lemorange.Modules.FinHubAddOns.Services
{
    public class UserRoleController : DnnApiController
    {
        private readonly IUserRoleRepository _repository;
        private readonly IPaymentRepository _paymentRepository;

        public UserRoleController()
        {
            _repository = new UserRoleRepository();
            _paymentRepository = new PaymentRepository();
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

        // Payment-related endpoints
        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage GetSubscriptionPlans(string planType = null)
        {
            try
            {
                var plans = _paymentRepository.GetSubscriptionPlans(planType);
                return Request.CreateResponse(HttpStatusCode.OK, plans);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage RecordPayment(PaymentRequest request)
        {
            try
            {
                if (request == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid request");
                }

                var currentUser = UserController.Instance.GetCurrentUserInfo();
                var payment = new Payment
                {
                    UserID = request.UserID,
                    PaymentType = request.PaymentType,
                    PlanID = request.PlanID,
                    Amount = request.Amount,
                    DiscountAmount = request.DiscountAmount,
                    Currency = "EUR",
                    PaymentDate = request.PaymentDate,
                    PaymentMethod = request.PaymentMethod,
                    ReferenceNumber = request.ReferenceNumber,
                    SubscriptionStartDate = request.SubscriptionStartDate,
                    SubscriptionEndDate = request.SubscriptionEndDate,
                    Status = "Active",
                    Notes = request.Notes,
                    PortalID = ActiveModule.PortalID,
                    CreatedByUserID = currentUser.UserID
                };

                var paymentId = _paymentRepository.RecordPayment(payment);

                if (paymentId > 0)
                {
                    return Request.CreateResponse(HttpStatusCode.OK, new { success = true, paymentId = paymentId });
                }
                else
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Failed to record payment");
                }
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage GetPaymentHistory(int userId, string paymentType = null)
        {
            try
            {
                var payments = _paymentRepository.GetPaymentHistory(userId, paymentType);
                return Request.CreateResponse(HttpStatusCode.OK, payments);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage GetPaymentStatistics()
        {
            try
            {
                var allPayments = _paymentRepository.GetAllPayments(ActiveModule.PortalID, "ServiceProvider");
                var today = DateTime.Today;
                var thisMonth = new DateTime(today.Year, today.Month, 1);
                var thisYear = new DateTime(today.Year, 1, 1);

                // Use regular variables instead of anonymous type
                decimal totalRevenue = 0m;
                decimal totalDiscounts = 0m;
                decimal monthlyRevenue = 0m;
                decimal yearlyRevenue = 0m;
                int activeSubscriptions = 0;
                int expiredSubscriptions = 0;
                int expiringThisMonth = 0;

                foreach (var payment in allPayments)
                {
                    if (payment.Status == "Active" && payment.SubscriptionEndDate.HasValue)
                    {
                        if (payment.SubscriptionEndDate.Value >= today)
                        {
                            activeSubscriptions++;

                            if (payment.SubscriptionEndDate.Value <= today.AddDays(30))
                            {
                                expiringThisMonth++;
                            }
                        }
                        else
                        {
                            expiredSubscriptions++;
                        }
                    }

                    totalRevenue += payment.Amount;
                    totalDiscounts += payment.DiscountAmount;

                    if (payment.PaymentDate >= thisMonth)
                    {
                        monthlyRevenue += payment.Amount;
                    }

                    if (payment.PaymentDate >= thisYear)
                    {
                        yearlyRevenue += payment.Amount;
                    }
                }

                // Create the anonymous type for the response
                var stats = new
                {
                    totalRevenue = totalRevenue,
                    totalDiscounts = totalDiscounts,
                    monthlyRevenue = monthlyRevenue,
                    yearlyRevenue = yearlyRevenue,
                    activeSubscriptions = activeSubscriptions,
                    expiredSubscriptions = expiredSubscriptions,
                    expiringThisMonth = expiringThisMonth
                };

                return Request.CreateResponse(HttpStatusCode.OK, stats);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.Edit)]
        public HttpResponseMessage UpdateUser(UpdateUserRequest request)
        {
            try
            {
                if (request == null || request.UserId <= 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid request");
                }

                var portalId = ActiveModule.PortalID;
                var currentUser = UserController.Instance.GetCurrentUserInfo();

                // Get the user
                var user = UserController.GetUserById(portalId, request.UserId);
                if (user == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.NotFound, "User not found");
                }

                // Update basic user info
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;
                user.DisplayName = request.DisplayName;

                // Update user
                UserController.UpdateUser(portalId, user);

                // Update profile properties
                user.Profile.SetProfileProperty("Telephone", request.Phone);
                user.Profile.SetProfileProperty("Cell", request.Mobile);
                user.Profile.SetProfileProperty("Street", request.Street);
                user.Profile.SetProfileProperty("City", request.City);
                user.Profile.SetProfileProperty("Region", request.StateRegion);
                user.Profile.SetProfileProperty("PostalCode", request.PostalCode);
                user.Profile.SetProfileProperty("Country", request.Country);

                DotNetNuke.Entities.Profile.ProfileController.UpdateUserProfile(user);

                // Update authorization status using SQL if it has changed
                // DNN doesn't expose UserPortals in the standard API
                if (request.IsAuthorized != user.Membership.Approved)
                {
                    user.Membership.Approved = request.IsAuthorized;
                    UserController.UpdateUser(portalId, user);

                    // Also update UserPortals table directly
                    using (var connection = new System.Data.SqlClient.SqlConnection(DotNetNuke.Data.DataProvider.Instance().ConnectionString))
                    {
                        connection.Open();
                        using (var command = new System.Data.SqlClient.SqlCommand())
                        {
                            command.Connection = connection;
                            command.CommandText = @"UPDATE UserPortals 
                                                  SET Authorised = @Authorised 
                                                  WHERE UserID = @UserID AND PortalID = @PortalID";

                            command.Parameters.AddWithValue("@Authorised", request.IsAuthorized);
                            command.Parameters.AddWithValue("@UserID", request.UserId);
                            command.Parameters.AddWithValue("@PortalID", portalId);

                            command.ExecuteNonQuery();
                        }
                    }
                }

                // Update role expiration if provided
                if (request.RoleExpirationDate.HasValue)
                {
                    _repository.UpdateRoleExpiration(
                        request.UserId,
                        "Service Provider",
                        request.RoleExpirationDate,
                        portalId,
                        currentUser.UserID
                    );
                }

                // Handle soft delete
                if (request.IsDeleted)
                {
                    user.IsDeleted = true;
                    UserController.UpdateUser(portalId, user);
                }

                return Request.CreateResponse(HttpStatusCode.OK, new { success = true });
            }
            catch (Exception ex)
            {
                DotNetNuke.Services.Exceptions.Exceptions.LogException(ex);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage GetPaymentStatisticsByYear(int year)
        {
            try
            {
                var allPayments = _paymentRepository.GetAllPayments(ActiveModule.PortalID, "ServiceProvider");
                var yearStart = new DateTime(year, 1, 1);
                var yearEnd = new DateTime(year, 12, 31);

                decimal yearlyRevenue = 0m;
                decimal yearlyDiscounts = 0m;
                int yearlyPaymentCount = 0;

                foreach (var payment in allPayments)
                {
                    if (payment.PaymentDate >= yearStart && payment.PaymentDate <= yearEnd)
                    {
                        yearlyRevenue += payment.Amount;
                        yearlyDiscounts += payment.DiscountAmount;
                        yearlyPaymentCount++;
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, new
                {
                    yearlyRevenue = yearlyRevenue,
                    yearlyDiscounts = yearlyDiscounts,
                    yearlyPaymentCount = yearlyPaymentCount,
                    yearlyNetRevenue = yearlyRevenue - yearlyDiscounts
                });
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [HttpGet]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        public HttpResponseMessage GetAvailableYears()
        {
            try
            {
                var allPayments = _paymentRepository.GetAllPayments(ActiveModule.PortalID, "ServiceProvider");
                var years = allPayments
                    .Select(p => p.PaymentDate.Year)
                    .Distinct()
                    .OrderByDescending(y => y)
                    .ToList();

                // Always include current year even if no payments
                var currentYear = DateTime.Now.Year;
                if (!years.Contains(currentYear))
                {
                    years.Insert(0, currentYear);
                }

                return Request.CreateResponse(HttpStatusCode.OK, years);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
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

        public class UpdateUserRequest
        {
            public int UserId { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string DisplayName { get; set; }
            public string Phone { get; set; }
            public string Mobile { get; set; }
            public string Street { get; set; }
            public string City { get; set; }
            public string StateRegion { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }
            public DateTime? RoleExpirationDate { get; set; }
            public bool IsAuthorized { get; set; }
            public bool IsDeleted { get; set; }
        }
    }
}