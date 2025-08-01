using DotNetNuke.Data;
using Lemorange.Modules.FinHubAddOns.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace Lemorange.Modules.FinHubAddOns.Components
{
    public interface IPaymentRepository
    {
        List<SubscriptionPlan> GetSubscriptionPlans(string planType = null);
        SubscriptionPlan GetSubscriptionPlan(int planId);
        int RecordPayment(Payment payment);
        List<Payment> GetPaymentHistory(int userId, string paymentType = null);
        List<Payment> GetAllPayments(int portalId, string paymentType = null);
        bool UpdatePaymentStatus(int paymentId, string status, int modifiedByUserId);
    }

    public class PaymentRepository : IPaymentRepository
    {
        public List<SubscriptionPlan> GetSubscriptionPlans(string planType = null)
        {
            var plans = new List<SubscriptionPlan>();

            using (var dr = DataProvider.Instance().ExecuteReader("FinPayments_GetSubscriptionPlans", planType))
            {
                while (dr.Read())
                {
                    plans.Add(new SubscriptionPlan
                    {
                        PlanID = Convert.ToInt32(dr["PlanID"]),
                        PlanName = dr["PlanName"].ToString(),
                        PlanType = dr["PlanType"].ToString(),
                        Description = dr["Description"] != DBNull.Value ? dr["Description"].ToString() : string.Empty,
                        Amount = Convert.ToDecimal(dr["Amount"]),
                        Currency = dr["Currency"].ToString(),
                        DurationMonths = Convert.ToInt32(dr["DurationMonths"]),
                        IsActive = Convert.ToBoolean(dr["IsActive"]),
                        SortOrder = Convert.ToInt32(dr["SortOrder"])
                    });
                }
            }

            return plans;
        }

        public SubscriptionPlan GetSubscriptionPlan(int planId)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = "SELECT * FROM FinSubscriptionPlans WHERE PlanID = @PlanID";
                    command.Parameters.AddWithValue("@PlanID", planId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SubscriptionPlan
                            {
                                PlanID = Convert.ToInt32(reader["PlanID"]),
                                PlanName = reader["PlanName"].ToString(),
                                PlanType = reader["PlanType"].ToString(),
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : string.Empty,
                                Amount = Convert.ToDecimal(reader["Amount"]),
                                Currency = reader["Currency"].ToString(),
                                DurationMonths = Convert.ToInt32(reader["DurationMonths"]),
                                IsActive = Convert.ToBoolean(reader["IsActive"]),
                                SortOrder = Convert.ToInt32(reader["SortOrder"])
                            };
                        }
                    }
                }
            }

            return null;
        }

        public int RecordPayment(Payment payment)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        INSERT INTO FinPayments (
                            UserID, PaymentType, PlanID, Amount, DiscountAmount, Currency, 
                            PaymentDate, PaymentMethod, ReferenceNumber,
                            SubscriptionStartDate, SubscriptionEndDate,
                            Status, InvoiceNumber, Notes, PortalID,
                            CreatedDate, CreatedByUserID
                        ) VALUES (
                            @UserID, @PaymentType, @PlanID, @Amount, @DiscountAmount, @Currency,
                            @PaymentDate, @PaymentMethod, @ReferenceNumber,
                            @SubscriptionStartDate, @SubscriptionEndDate,
                            @Status, @InvoiceNumber, @Notes, @PortalID,
                            GETDATE(), @CreatedByUserID
                        );
                        SELECT SCOPE_IDENTITY();";

                    command.Parameters.AddWithValue("@UserID", payment.UserID);
                    command.Parameters.AddWithValue("@PaymentType", payment.PaymentType);
                    command.Parameters.AddWithValue("@PlanID", (object)payment.PlanID ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Amount", payment.Amount);
                    command.Parameters.AddWithValue("@DiscountAmount", payment.DiscountAmount);
                    command.Parameters.AddWithValue("@Currency", payment.Currency ?? "EUR");
                    command.Parameters.AddWithValue("@PaymentDate", payment.PaymentDate);
                    command.Parameters.AddWithValue("@PaymentMethod", (object)payment.PaymentMethod ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ReferenceNumber", (object)payment.ReferenceNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SubscriptionStartDate", (object)payment.SubscriptionStartDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@SubscriptionEndDate", (object)payment.SubscriptionEndDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Status", payment.Status ?? "Active");
                    command.Parameters.AddWithValue("@InvoiceNumber", (object)payment.InvoiceNumber ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Notes", (object)payment.Notes ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PortalID", payment.PortalID);
                    command.Parameters.AddWithValue("@CreatedByUserID", payment.CreatedByUserID);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public List<Payment> GetPaymentHistory(int userId, string paymentType = null)
        {
            var payments = new List<Payment>();

            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        SELECT p.*, sp.PlanName, u.DisplayName as UserDisplayName
                        FROM FinPayments p
                        LEFT JOIN FinSubscriptionPlans sp ON p.PlanID = sp.PlanID
                        INNER JOIN Users u ON p.UserID = u.UserID
                        WHERE p.UserID = @UserID
                        AND (@PaymentType IS NULL OR p.PaymentType = @PaymentType)
                        ORDER BY p.PaymentDate DESC";

                    command.Parameters.AddWithValue("@UserID", userId);
                    command.Parameters.AddWithValue("@PaymentType", (object)paymentType ?? DBNull.Value);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            payments.Add(ReadPaymentFromDataReader(reader));
                        }
                    }
                }
            }

            return payments;
        }

        public List<Payment> GetAllPayments(int portalId, string paymentType = null)
        {
            var payments = new List<Payment>();

            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        SELECT p.*, sp.PlanName, u.DisplayName as UserDisplayName
                        FROM FinPayments p
                        LEFT JOIN FinSubscriptionPlans sp ON p.PlanID = sp.PlanID
                        INNER JOIN Users u ON p.UserID = u.UserID
                        WHERE p.PortalID = @PortalID
                        AND (@PaymentType IS NULL OR p.PaymentType = @PaymentType)
                        ORDER BY p.PaymentDate DESC";

                    command.Parameters.AddWithValue("@PortalID", portalId);
                    command.Parameters.AddWithValue("@PaymentType", (object)paymentType ?? DBNull.Value);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            payments.Add(ReadPaymentFromDataReader(reader));
                        }
                    }
                }
            }

            return payments;
        }

        public bool UpdatePaymentStatus(int paymentId, string status, int modifiedByUserId)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        UPDATE FinPayments 
                        SET Status = @Status,
                            LastModifiedDate = GETDATE(),
                            LastModifiedByUserID = @ModifiedBy
                        WHERE PaymentID = @PaymentID";

                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@ModifiedBy", modifiedByUserId);
                    command.Parameters.AddWithValue("@PaymentID", paymentId);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        private Payment ReadPaymentFromDataReader(IDataReader reader)
        {
            var payment = new Payment
            {
                PaymentID = Convert.ToInt32(reader["PaymentID"]),
                UserID = Convert.ToInt32(reader["UserID"]),
                PaymentType = reader["PaymentType"].ToString(),
                PlanID = reader["PlanID"] != DBNull.Value ? Convert.ToInt32(reader["PlanID"]) : (int?)null,
                Amount = Convert.ToDecimal(reader["Amount"]),
                // ✅ FIXED: Added the missing DiscountAmount mapping
                DiscountAmount = reader["DiscountAmount"] != DBNull.Value ? Convert.ToDecimal(reader["DiscountAmount"]) : 0m,
                Currency = reader["Currency"].ToString(),
                PaymentDate = Convert.ToDateTime(reader["PaymentDate"]),
                PaymentMethod = reader["PaymentMethod"] != DBNull.Value ? reader["PaymentMethod"].ToString() : null,
                ReferenceNumber = reader["ReferenceNumber"] != DBNull.Value ? reader["ReferenceNumber"].ToString() : null,
                SubscriptionStartDate = reader["SubscriptionStartDate"] != DBNull.Value ? Convert.ToDateTime(reader["SubscriptionStartDate"]) : (DateTime?)null,
                SubscriptionEndDate = reader["SubscriptionEndDate"] != DBNull.Value ? Convert.ToDateTime(reader["SubscriptionEndDate"]) : (DateTime?)null,
                Status = reader["Status"].ToString(),
                InvoiceNumber = reader["InvoiceNumber"] != DBNull.Value ? reader["InvoiceNumber"].ToString() : null,
                Notes = reader["Notes"] != DBNull.Value ? reader["Notes"].ToString() : null,
                PortalID = Convert.ToInt32(reader["PortalID"]),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                CreatedByUserID = Convert.ToInt32(reader["CreatedByUserID"]),
                UserDisplayName = reader["UserDisplayName"] != DBNull.Value ? reader["UserDisplayName"].ToString() : null,
                PlanName = reader["PlanName"] != DBNull.Value ? reader["PlanName"].ToString() : null
            };

            // Calculate actual status based on dates
            if (payment.Status != "Cancelled" && payment.Status != "Refunded")
            {
                if (payment.SubscriptionStartDate.HasValue && payment.SubscriptionEndDate.HasValue)
                {
                    var today = DateTime.Today;
                    if (today < payment.SubscriptionStartDate.Value)
                    {
                        payment.Status = "Pending";
                    }
                    else if (today >= payment.SubscriptionStartDate.Value && today <= payment.SubscriptionEndDate.Value)
                    {
                        payment.Status = "Active";
                    }
                    else if (today > payment.SubscriptionEndDate.Value)
                    {
                        payment.Status = "Expired";
                    }
                }
                else
                {
                    // If no subscription dates, base it on the payment status in DB
                    payment.Status = payment.Status ?? "Completed";
                }
            }

            return payment;
        }
    }
}