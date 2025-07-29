using System;

namespace Lemorange.Modules.FinHubAddOns.Models
{
    public class SubscriptionPlan
    {
        public int PlanID { get; set; }
        public string PlanName { get; set; }
        public string PlanType { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public int DurationMonths { get; set; }
        public bool IsActive { get; set; }
        public int SortOrder { get; set; }
    }

    public class Payment
    {
        public int PaymentID { get; set; }
        public int UserID { get; set; }
        public string PaymentType { get; set; }
        public int? PlanID { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public DateTime PaymentDate { get; set; }
        public string PaymentMethod { get; set; }
        public string ReferenceNumber { get; set; }
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }
        public string Status { get; set; }
        public string InvoiceNumber { get; set; }
        public string InvoiceFileName { get; set; }
        public string InvoiceFilePath { get; set; }
        public int PortalID { get; set; }
        public string Notes { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedByUserID { get; set; }

        // Additional properties for display
        public string UserDisplayName { get; set; }
        public string PlanName { get; set; }
    }

    public class PaymentRequest
    {
        public int UserID { get; set; }
        public string PaymentType { get; set; }
        public int? PlanID { get; set; }
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; }
        public DateTime SubscriptionStartDate { get; set; }
        public DateTime SubscriptionEndDate { get; set; }
        public string PaymentMethod { get; set; }
        public string ReferenceNumber { get; set; }
        public string Notes { get; set; }
    }
}