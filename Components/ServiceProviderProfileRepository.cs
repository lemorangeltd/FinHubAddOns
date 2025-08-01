using System;
using System.Collections.Generic;
using System.Data;
using DotNetNuke.Data;
using Lemorange.Modules.FinHubAddOns.Models;

namespace Lemorange.Modules.FinHubAddOns.Components
{
    public interface IServiceProviderProfileRepository
    {
        ServiceProviderProfileStats GetProfileStats(int serviceProviderUserId, int portalId);
        List<ServiceProviderAction> GetActions(int serviceProviderUserId, int portalId, int currentUserId);
        List<ServiceProviderTask> GetTasks(int serviceProviderUserId, int portalId);
        List<ServiceProviderRating> GetRatings(int serviceProviderUserId, int portalId);
        List<ServiceProviderProject> GetProjects(int serviceProviderUserId, int portalId);

        int AddAction(ServiceProviderAction action);
        int AddRating(ServiceProviderRating rating);
        int AddTask(ServiceProviderTask task);
        bool UpdateTaskStatus(int taskId, string status, int modifiedByUserId);

        List<int> GetManagerUserIds(int portalId);
    }

    public class ServiceProviderProfileRepository : IServiceProviderProfileRepository
    {
        public ServiceProviderProfileStats GetProfileStats(int serviceProviderUserId, int portalId)
        {
            var stats = new ServiceProviderProfileStats();

            using (var dr = DataProvider.Instance().ExecuteReader("FinServiceProvider_GetProfileStats", serviceProviderUserId, portalId))
            {
                if (dr.Read())
                {
                    stats = new ServiceProviderProfileStats
                    {
                        TotalOffers = Convert.ToInt32(dr["TotalOffers"]),
                        AcceptedOffers = Convert.ToInt32(dr["AcceptedOffers"]),
                        RejectedOffers = Convert.ToInt32(dr["RejectedOffers"]),
                        PendingOffers = Convert.ToInt32(dr["PendingOffers"]),
                        TotalProjectsBidOn = Convert.ToInt32(dr["TotalProjectsBidOn"]),
                        ProjectsWon = Convert.ToInt32(dr["ProjectsWon"]),
                        TotalRevenue = Convert.ToDecimal(dr["TotalRevenue"]),
                        AverageProjectValue = Convert.ToDecimal(dr["AverageProjectValue"]),
                        SuccessRate = Convert.ToDouble(dr["SuccessRate"]),
                        AverageRating = Convert.ToDouble(dr["AverageRating"])
                    };
                }
            }

            return stats;
        }

        public List<ServiceProviderAction> GetActions(int serviceProviderUserId, int portalId, int currentUserId)
        {
            var actions = new List<ServiceProviderAction>();

            using (var dr = DataProvider.Instance().ExecuteReader("FinServiceProvider_GetActions", serviceProviderUserId, portalId, currentUserId))
            {
                while (dr.Read())
                {
                    actions.Add(new ServiceProviderAction
                    {
                        ActionID = Convert.ToInt32(dr["ActionID"]),
                        ServiceProviderUserID = serviceProviderUserId,
                        ActionType = dr["ActionType"].ToString(),
                        ActionTitle = dr["ActionTitle"].ToString(),
                        ActionDescription = dr["ActionDescription"] != DBNull.Value ? dr["ActionDescription"].ToString() : null,
                        ActionDate = Convert.ToDateTime(dr["ActionDate"]),
                        Priority = dr["Priority"] != DBNull.Value ? dr["Priority"].ToString() : null,
                        IsPrivate = Convert.ToBoolean(dr["IsPrivate"]),
                        CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                        CreatedByName = dr["CreatedByName"].ToString()
                    });
                }
            }

            return actions;
        }

        public List<ServiceProviderTask> GetTasks(int serviceProviderUserId, int portalId)
        {
            var tasks = new List<ServiceProviderTask>();

            using (var dr = DataProvider.Instance().ExecuteReader("FinServiceProvider_GetTasks", serviceProviderUserId, portalId))
            {
                while (dr.Read())
                {
                    tasks.Add(new ServiceProviderTask
                    {
                        TaskID = Convert.ToInt32(dr["TaskID"]),
                        ServiceProviderUserID = serviceProviderUserId,
                        TaskTitle = dr["TaskTitle"].ToString(),
                        TaskDescription = dr["TaskDescription"] != DBNull.Value ? dr["TaskDescription"].ToString() : null,
                        DueDate = dr["DueDate"] != DBNull.Value ? Convert.ToDateTime(dr["DueDate"]) : (DateTime?)null,
                        Status = dr["Status"].ToString(),
                        Priority = dr["Priority"].ToString(),
                        CreatedDate = Convert.ToDateTime(dr["CreatedDate"]),
                        CompletedDate = dr["CompletedDate"] != DBNull.Value ? Convert.ToDateTime(dr["CompletedDate"]) : (DateTime?)null,
                        AssignedByName = dr["AssignedByName"].ToString(),
                        AssignedToName = dr["AssignedToName"].ToString()
                    });
                }
            }

            return tasks;
        }

        public List<ServiceProviderRating> GetRatings(int serviceProviderUserId, int portalId)
        {
            var ratings = new List<ServiceProviderRating>();

            using (var dr = DataProvider.Instance().ExecuteReader("FinServiceProvider_GetRatings", serviceProviderUserId, portalId))
            {
                while (dr.Read())
                {
                    ratings.Add(new ServiceProviderRating
                    {
                        RatingID = Convert.ToInt32(dr["RatingID"]),
                        ServiceProviderUserID = serviceProviderUserId,
                        Rating = Convert.ToInt32(dr["Rating"]),
                        RatingCategory = dr["RatingCategory"].ToString(),
                        Comments = dr["Comments"] != DBNull.Value ? dr["Comments"].ToString() : null,
                        RatedDate = Convert.ToDateTime(dr["RatedDate"]),
                        RatedByName = dr["RatedByName"].ToString()
                    });
                }
            }

            return ratings;
        }

        public List<ServiceProviderProject> GetProjects(int serviceProviderUserId, int portalId)
        {
            var projects = new List<ServiceProviderProject>();

            using (var dr = DataProvider.Instance().ExecuteReader("FinServiceProvider_GetProjects", serviceProviderUserId, portalId))
            {
                while (dr.Read())
                {
                    projects.Add(new ServiceProviderProject
                    {
                        ProjectID = Convert.ToInt32(dr["ProjectID"]),
                        Title = dr["Title"].ToString(),
                        Description = dr["Description"] != DBNull.Value ? dr["Description"].ToString() : null,
                        Budget = dr["Budget"] != DBNull.Value ? Convert.ToDecimal(dr["Budget"]) : (decimal?)null,
                        BudgetCur = dr["BudgetCur"] != DBNull.Value ? dr["BudgetCur"].ToString() : null,
                        DateCreated = Convert.ToDateTime(dr["DateCreated"]),
                        OfferID = dr["OfferID"] != DBNull.Value ? Convert.ToInt32(dr["OfferID"]) : (int?)null,
                        OfferPrice = dr["OfferPrice"] != DBNull.Value ? Convert.ToDecimal(dr["OfferPrice"]) : (decimal?)null,
                        OfferCurrency = dr["OfferCurrency"] != DBNull.Value ? dr["OfferCurrency"].ToString() : null,
                        DateOffered = dr["DateOffered"] != DBNull.Value ? Convert.ToDateTime(dr["DateOffered"]) : (DateTime?)null,
                        OfferStatus = dr["OfferStatus"] != DBNull.Value ? dr["OfferStatus"].ToString() : null,
                        OfferMessage = dr["OfferMessage"] != DBNull.Value ? dr["OfferMessage"].ToString() : null
                    });
                }
            }

            return projects;
        }

        public int AddAction(ServiceProviderAction action)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        INSERT INTO FinServiceProviderActions (
                            ServiceProviderUserID, ActionType, ActionTitle, ActionDescription, 
                            ActionDate, CreatedByUserID, PortalID, IsPrivate, Priority
                        ) VALUES (
                            @ServiceProviderUserID, @ActionType, @ActionTitle, @ActionDescription,
                            @ActionDate, @CreatedByUserID, @PortalID, @IsPrivate, @Priority
                        );
                        SELECT SCOPE_IDENTITY();";

                    command.Parameters.AddWithValue("@ServiceProviderUserID", action.ServiceProviderUserID);
                    command.Parameters.AddWithValue("@ActionType", action.ActionType);
                    command.Parameters.AddWithValue("@ActionTitle", action.ActionTitle);
                    command.Parameters.AddWithValue("@ActionDescription", (object)action.ActionDescription ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ActionDate", action.ActionDate);
                    command.Parameters.AddWithValue("@CreatedByUserID", action.CreatedByUserID);
                    command.Parameters.AddWithValue("@PortalID", action.PortalID);
                    command.Parameters.AddWithValue("@IsPrivate", action.IsPrivate);
                    command.Parameters.AddWithValue("@Priority", (object)action.Priority ?? DBNull.Value);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public int AddRating(ServiceProviderRating rating)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        INSERT INTO FinServiceProviderRatings (
                            ServiceProviderUserID, Rating, RatingCategory, Comments, 
                            RatedByUserID, PortalID
                        ) VALUES (
                            @ServiceProviderUserID, @Rating, @RatingCategory, @Comments,
                            @RatedByUserID, @PortalID
                        );
                        SELECT SCOPE_IDENTITY();";

                    command.Parameters.AddWithValue("@ServiceProviderUserID", rating.ServiceProviderUserID);
                    command.Parameters.AddWithValue("@Rating", rating.Rating);
                    command.Parameters.AddWithValue("@RatingCategory", rating.RatingCategory);
                    command.Parameters.AddWithValue("@Comments", (object)rating.Comments ?? DBNull.Value);
                    command.Parameters.AddWithValue("@RatedByUserID", rating.RatedByUserID);
                    command.Parameters.AddWithValue("@PortalID", rating.PortalID);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public int AddTask(ServiceProviderTask task)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        INSERT INTO FinServiceProviderTasks (
                            ServiceProviderUserID, TaskTitle, TaskDescription, 
                            AssignedByUserID, AssignedToUserID, DueDate, 
                            Status, Priority, PortalID
                        ) VALUES (
                            @ServiceProviderUserID, @TaskTitle, @TaskDescription,
                            @AssignedByUserID, @AssignedToUserID, @DueDate,
                            @Status, @Priority, @PortalID
                        );
                        SELECT SCOPE_IDENTITY();";

                    command.Parameters.AddWithValue("@ServiceProviderUserID", task.ServiceProviderUserID);
                    command.Parameters.AddWithValue("@TaskTitle", task.TaskTitle);
                    command.Parameters.AddWithValue("@TaskDescription", (object)task.TaskDescription ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AssignedByUserID", task.AssignedByUserID);
                    command.Parameters.AddWithValue("@AssignedToUserID", task.AssignedToUserID);
                    command.Parameters.AddWithValue("@DueDate", (object)task.DueDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Status", task.Status ?? "Pending");
                    command.Parameters.AddWithValue("@Priority", task.Priority ?? "Medium");
                    command.Parameters.AddWithValue("@PortalID", task.PortalID);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool UpdateTaskStatus(int taskId, string status, int modifiedByUserId)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        UPDATE FinServiceProviderTasks 
                        SET Status = @Status,
                            CompletedDate = CASE WHEN @Status = 'Completed' THEN GETDATE() ELSE NULL END
                        WHERE TaskID = @TaskID";

                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@TaskID", taskId);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<int> GetManagerUserIds(int portalId)
        {
            var managers = new List<int>();

            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        SELECT DISTINCT u.UserID
                        FROM Users u
                        INNER JOIN UserRoles ur ON u.UserID = ur.UserID
                        INNER JOIN Roles r ON ur.RoleID = r.RoleID
                        WHERE r.PortalID = @PortalID
                        AND r.RoleName IN ('Administrators', 'Managers', 'Staff')
                        AND (ur.ExpiryDate IS NULL OR ur.ExpiryDate > GETDATE())
                        ORDER BY u.UserID";

                    command.Parameters.AddWithValue("@PortalID", portalId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            managers.Add(Convert.ToInt32(reader["UserID"]));
                        }
                    }
                }
            }

            return managers;
        }
    }
}