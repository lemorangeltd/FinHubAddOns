using System;

namespace Lemorange.Modules.FinHubAddOns.Models
{
    public class ServiceProviderUser
    {
        // Basic User Info
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }

        // Role Information
        public DateTime? RoleStartDate { get; set; }
        public DateTime? RoleExpirationDate { get; set; }
        public bool IsRoleOwner { get; set; }

        // Account Status & Dates
        public DateTime AccountCreated { get; set; }
        public DateTime? AccountLastModified { get; set; }
        public DateTime? PortalJoinDate { get; set; }
        public bool IsAuthorized { get; set; }
        public bool IsSuperUser { get; set; }

        // Address Information
        public string Street { get; set; }
        public string City { get; set; }
        public string StateRegion { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        // Contact Info
        public string Phone { get; set; }
        public string Mobile { get; set; }

        // Calculated Fields
        public int DaysInRole { get; set; }
        public string OtherRoles { get; set; }
    }
}