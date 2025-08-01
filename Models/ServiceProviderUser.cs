using System;

namespace Lemorange.Modules.FinHubAddOns.Models
{
    // Update existing ServiceProviderUser model
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
        public bool IsDeleted { get; set; }

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

        // Payment Information
        public string PaymentStatus { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public string CurrentPlan { get; set; }

        // Rating Information
        public decimal AverageRating { get; set; }
    }

    // Action/Activity model
    public class SPAction
    {
        public int ActionID { get; set; }
        public int ServiceProviderID { get; set; }
        public string ActionType { get; set; }
        public string ActionTitle { get; set; }
        public string ActionDescription { get; set; }
        public DateTime ActionDate { get; set; }
        public int CreatedByUserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public int PortalID { get; set; }
        public string CreatedByName { get; set; }
    }

    // Rating model
    public class SPRating
    {
        public int RatingID { get; set; }
        public int ServiceProviderID { get; set; }
        public int Rating { get; set; }
        public string RatingCategory { get; set; }
        public string Comments { get; set; }
        public int RatedByUserID { get; set; }
        public DateTime RatedDate { get; set; }
        public int PortalID { get; set; }
        public string RatedByName { get; set; }
    }

    // Task model
    public class SPTask
    {
        public int TaskID { get; set; }
        public int ServiceProviderID { get; set; }
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public int AssignedByUserID { get; set; }
        public int AssignedToUserID { get; set; }
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public int PortalID { get; set; }
        public string AssignedByName { get; set; }
        public string AssignedToName { get; set; }
    }

    // Note model
    public class SPNote
    {
        public int NoteID { get; set; }
        public int ServiceProviderID { get; set; }
        public string NoteType { get; set; }
        public string NoteContent { get; set; }
        public bool IsPrivate { get; set; }
        public int CreatedByUserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? LastModifiedDate { get; set; }
        public int PortalID { get; set; }
        public string CreatedByName { get; set; }
    }

    // Timeline item model
    public class SPTimelineItem
    {
        public string ItemType { get; set; }
        public int ItemID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime ItemDate { get; set; }
        public string CreatedByName { get; set; }
        public int CreatedByUserID { get; set; }
        public string SubType { get; set; }
        public int? Rating { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
    }

    // Profile statistics model
    public class SPProfileStats
    {
        public int TotalOffers { get; set; }
        public int OffersWon { get; set; }
        public decimal TotalOfferValue { get; set; }
        public decimal AverageOfferValue { get; set; }
        public int ProjectsBidOn { get; set; }
        public int ProjectsAwarded { get; set; }
        public decimal TotalProjectValue { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalRatings { get; set; }
        public int TotalActions { get; set; }
        public int RecentActions { get; set; }
        public int OpenTasks { get; set; }
        public int CompletedTasks { get; set; }
        public decimal WinRate { get; set; }
        public decimal EstimatedProfit { get; set; }
    }

    // Request models
    public class CreateActionRequest
    {
        public int ServiceProviderID { get; set; }
        public string ActionType { get; set; }
        public string ActionTitle { get; set; }
        public string ActionDescription { get; set; }
        public DateTime ActionDate { get; set; }
    }

    public class CreateRatingRequest
    {
        public int ServiceProviderID { get; set; }
        public int Rating { get; set; }
        public string RatingCategory { get; set; }
        public string Comments { get; set; }
    }

    public class CreateTaskRequest
    {
        public int ServiceProviderID { get; set; }
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public int AssignedToUserID { get; set; }
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; }
    }

    public class UpdateTaskRequest
    {
        public int TaskID { get; set; }
        public string Status { get; set; }
        public DateTime? CompletedDate { get; set; }
    }

    public class CreateNoteRequest
    {
        public int ServiceProviderID { get; set; }
        public string NoteType { get; set; }
        public string NoteContent { get; set; }
        public bool IsPrivate { get; set; }
    }
}