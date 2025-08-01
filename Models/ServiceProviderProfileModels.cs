using System;

namespace Lemorange.Modules.FinHubAddOns.Models
{
    public class ServiceProviderProfileStats
    {
        public int TotalOffers { get; set; }
        public int AcceptedOffers { get; set; }
        public int RejectedOffers { get; set; }
        public int PendingOffers { get; set; }
        public int TotalProjectsBidOn { get; set; }
        public int ProjectsWon { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal AverageProjectValue { get; set; }
        public double SuccessRate { get; set; }
        public double AverageRating { get; set; }
    }

    public class ServiceProviderAction
    {
        public int ActionID { get; set; }
        public int ServiceProviderUserID { get; set; }
        public string ActionType { get; set; }
        public string ActionTitle { get; set; }
        public string ActionDescription { get; set; }
        public DateTime ActionDate { get; set; }
        public int CreatedByUserID { get; set; }
        public DateTime CreatedDate { get; set; }
        public int PortalID { get; set; }
        public bool IsPrivate { get; set; }
        public string Priority { get; set; }

        // Display properties
        public string CreatedByName { get; set; }
    }

    public class ServiceProviderRating
    {
        public int RatingID { get; set; }
        public int ServiceProviderUserID { get; set; }
        public int Rating { get; set; }
        public string RatingCategory { get; set; }
        public string Comments { get; set; }
        public int RatedByUserID { get; set; }
        public DateTime RatedDate { get; set; }
        public int PortalID { get; set; }

        // Display properties
        public string RatedByName { get; set; }
    }

    public class ServiceProviderTask
    {
        public int TaskID { get; set; }
        public int ServiceProviderUserID { get; set; }
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public int AssignedByUserID { get; set; }
        public int AssignedToUserID { get; set; }
        public DateTime? DueDate { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public int PortalID { get; set; }

        // Display properties
        public string AssignedByName { get; set; }
        public string AssignedToName { get; set; }
    }

    public class ServiceProviderProject
    {
        public int ProjectID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal? Budget { get; set; }
        public string BudgetCur { get; set; }
        public DateTime DateCreated { get; set; }

        // Offer details
        public int? OfferID { get; set; }
        public decimal? OfferPrice { get; set; }
        public string OfferCurrency { get; set; }
        public DateTime? DateOffered { get; set; }
        public string OfferStatus { get; set; }
        public string OfferMessage { get; set; }
    }

    // Request DTOs
    public class AddActionRequest
    {
        public int ServiceProviderUserID { get; set; }
        public string ActionType { get; set; }
        public string ActionTitle { get; set; }
        public string ActionDescription { get; set; }
        public DateTime ActionDate { get; set; }
        public bool IsPrivate { get; set; }
        public string Priority { get; set; }
    }

    public class AddRatingRequest
    {
        public int ServiceProviderUserID { get; set; }
        public int Rating { get; set; }
        public string RatingCategory { get; set; }
        public string Comments { get; set; }
    }

    public class AddTaskRequest
    {
        public int ServiceProviderUserID { get; set; }
        public string TaskTitle { get; set; }
        public string TaskDescription { get; set; }
        public int AssignedToUserID { get; set; }
        public DateTime? DueDate { get; set; }
        public string Priority { get; set; }
    }

    public class UpdateTaskStatusRequest
    {
        public int TaskID { get; set; }
        public string Status { get; set; }
    }
}