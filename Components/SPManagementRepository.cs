using DotNetNuke.Data;
using Lemorange.Modules.FinHubAddOns.Models;
using System;
using System.Collections.Generic;
using System.Data;
using static System.Collections.Specialized.BitVector32;

namespace Lemorange.Modules.FinHubAddOns.Components
{
    public interface ISPManagementRepository
    {
        // Actions
        int CreateAction(SPAction action);
        List<SPAction> GetActions(int serviceProviderId, int portalId);

        // Ratings
        int CreateRating(SPRating rating);
        List<SPRating> GetRatings(int serviceProviderId, int portalId);
        decimal GetAverageRating(int serviceProviderId, int portalId, string category = "Overall");

        // Tasks
        int CreateTask(SPTask task);
        bool UpdateTask(int taskId, string status, DateTime? completedDate, int modifiedByUserId);
        List<SPTask> GetTasks(int serviceProviderId, int portalId);
        List<SPTask> GetTasksAssignedTo(int userId, int portalId);

        // Notes
        int CreateNote(SPNote note);
        List<SPNote> GetNotes(int serviceProviderId, int portalId);

        // Timeline
        List<SPTimelineItem> GetTimeline(int serviceProviderId, int portalId);

        // Profile Statistics
        SPProfileStats GetProfileStats(int serviceProviderId, int portalId);
    }

    public class SPManagementRepository : ISPManagementRepository
    {
        // Actions
        public int CreateAction(SPAction action)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        INSERT INTO FinSPActions (
                            ServiceProviderID, ActionType, ActionTitle, ActionDescription,
                            ActionDate, CreatedByUserID, CreatedDate, PortalID
                        ) VALUES (
                            @ServiceProviderID, @ActionType, @ActionTitle, @ActionDescription,
                            @ActionDate, @CreatedByUserID, GETDATE(), @PortalID
                        );
                        SELECT SCOPE_IDENTITY();";

                    command.Parameters.AddWithValue("@ServiceProviderID", action.ServiceProviderID);
                    command.Parameters.AddWithValue("@ActionType", action.ActionType);
                    command.Parameters.AddWithValue("@ActionTitle", action.ActionTitle);
                    command.Parameters.AddWithValue("@ActionDescription", (object)action.ActionDescription ?? DBNull.Value);
                    command.Parameters.AddWithValue("@ActionDate", action.ActionDate);
                    command.Parameters.AddWithValue("@CreatedByUserID", action.CreatedByUserID);
                    command.Parameters.AddWithValue("@PortalID", action.PortalID);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public List<SPAction> GetActions(int serviceProviderId, int portalId)
        {
            var actions = new List<SPAction>();

            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        SELECT a.*, u.DisplayName as CreatedByName
                        FROM FinSPActions a
                        INNER JOIN Users u ON a.CreatedByUserID = u.UserID
                        WHERE a.ServiceProviderID = @ServiceProviderID 
                        AND a.PortalID = @PortalID
                        ORDER BY a.ActionDate DESC";

                    command.Parameters.AddWithValue("@ServiceProviderID", serviceProviderId);
                    command.Parameters.AddWithValue("@PortalID", portalId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            actions.Add(ReadActionFromDataReader(reader));
                        }
                    }
                }
            }

            return actions;
        }

        // Ratings
        public int CreateRating(SPRating rating)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        INSERT INTO FinSPRatings (
                            ServiceProviderID, Rating, RatingCategory, Comments,
                            RatedByUserID, RatedDate, PortalID
                        ) VALUES (
                            @ServiceProviderID, @Rating, @RatingCategory, @Comments,
                            @RatedByUserID, GETDATE(), @PortalID
                        );
                        SELECT SCOPE_IDENTITY();";

                    command.Parameters.AddWithValue("@ServiceProviderID", rating.ServiceProviderID);
                    command.Parameters.AddWithValue("@Rating", rating.Rating);
                    command.Parameters.AddWithValue("@RatingCategory", rating.RatingCategory);
                    command.Parameters.AddWithValue("@Comments", (object)rating.Comments ?? DBNull.Value);
                    command.Parameters.AddWithValue("@RatedByUserID", rating.RatedByUserID);
                    command.Parameters.AddWithValue("@PortalID", rating.PortalID);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public List<SPRating> GetRatings(int serviceProviderId, int portalId)
        {
            var ratings = new List<SPRating>();

            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        SELECT r.*, u.DisplayName as RatedByName
                        FROM FinSPRatings r
                        INNER JOIN Users u ON r.RatedByUserID = u.UserID
                        WHERE r.ServiceProviderID = @ServiceProviderID 
                        AND r.PortalID = @PortalID
                        ORDER BY r.RatedDate DESC";

                    command.Parameters.AddWithValue("@ServiceProviderID", serviceProviderId);
                    command.Parameters.AddWithValue("@PortalID", portalId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            ratings.Add(ReadRatingFromDataReader(reader));
                        }
                    }
                }
            }

            return ratings;
        }

        public decimal GetAverageRating(int serviceProviderId, int portalId, string category = "Overall")
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        SELECT AVG(CAST(Rating AS DECIMAL(3,2)))
                        FROM FinSPRatings
                        WHERE ServiceProviderID = @ServiceProviderID 
                        AND PortalID = @PortalID
                        AND RatingCategory = @Category";

                    command.Parameters.AddWithValue("@ServiceProviderID", serviceProviderId);
                    command.Parameters.AddWithValue("@PortalID", portalId);
                    command.Parameters.AddWithValue("@Category", category);

                    var result = command.ExecuteScalar();
                    return result == DBNull.Value ? 0 : Convert.ToDecimal(result);
                }
            }
        }

        // Tasks
        public int CreateTask(SPTask task)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        INSERT INTO FinSPTasks (
                            ServiceProviderID, TaskTitle, TaskDescription,
                            AssignedByUserID, AssignedToUserID, DueDate,
                            Priority, Status, CreatedDate, PortalID
                        ) VALUES (
                            @ServiceProviderID, @TaskTitle, @TaskDescription,
                            @AssignedByUserID, @AssignedToUserID, @DueDate,
                            @Priority, 'Pending', GETDATE(), @PortalID
                        );
                        SELECT SCOPE_IDENTITY();";

                    command.Parameters.AddWithValue("@ServiceProviderID", task.ServiceProviderID);
                    command.Parameters.AddWithValue("@TaskTitle", task.TaskTitle);
                    command.Parameters.AddWithValue("@TaskDescription", (object)task.TaskDescription ?? DBNull.Value);
                    command.Parameters.AddWithValue("@AssignedByUserID", task.AssignedByUserID);
                    command.Parameters.AddWithValue("@AssignedToUserID", task.AssignedToUserID);
                    command.Parameters.AddWithValue("@DueDate", (object)task.DueDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Priority", task.Priority ?? "Normal");
                    command.Parameters.AddWithValue("@PortalID", task.PortalID);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool UpdateTask(int taskId, string status, DateTime? completedDate, int modifiedByUserId)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        UPDATE FinSPTasks 
                        SET Status = @Status,
                            CompletedDate = @CompletedDate,
                            LastModifiedDate = GETDATE()
                        WHERE TaskID = @TaskID";

                    command.Parameters.AddWithValue("@Status", status);
                    command.Parameters.AddWithValue("@CompletedDate", (object)completedDate ?? DBNull.Value);
                    command.Parameters.AddWithValue("@TaskID", taskId);

                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public List<SPTask> GetTasks(int serviceProviderId, int portalId)
        {
            var tasks = new List<SPTask>();

            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        SELECT t.*, 
                               u1.DisplayName as AssignedByName,
                               u2.DisplayName as AssignedToName
                        FROM FinSPTasks t
                        INNER JOIN Users u1 ON t.AssignedByUserID = u1.UserID
                        INNER JOIN Users u2 ON t.AssignedToUserID = u2.UserID
                        WHERE t.ServiceProviderID = @ServiceProviderID 
                        AND t.PortalID = @PortalID
                        ORDER BY t.CreatedDate DESC";

                    command.Parameters.AddWithValue("@ServiceProviderID", serviceProviderId);
                    command.Parameters.AddWithValue("@PortalID", portalId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(ReadTaskFromDataReader(reader));
                        }
                    }
                }
            }

            return tasks;
        }

        public List<SPTask> GetTasksAssignedTo(int userId, int portalId)
        {
            var tasks = new List<SPTask>();

            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        SELECT t.*, 
                               u1.DisplayName as AssignedByName,
                               u2.DisplayName as AssignedToName
                        FROM FinSPTasks t
                        INNER JOIN Users u1 ON t.AssignedByUserID = u1.UserID
                        INNER JOIN Users u2 ON t.AssignedToUserID = u2.UserID
                        WHERE t.AssignedToUserID = @UserID 
                        AND t.PortalID = @PortalID
                        AND t.Status IN ('Pending', 'InProgress')
                        ORDER BY t.DueDate, t.Priority DESC";

                    command.Parameters.AddWithValue("@UserID", userId);
                    command.Parameters.AddWithValue("@PortalID", portalId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            tasks.Add(ReadTaskFromDataReader(reader));
                        }
                    }
                }
            }

            return tasks;
        }

        // Notes
        public int CreateNote(SPNote note)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        INSERT INTO FinSPNotes (
                            ServiceProviderID, NoteType, NoteContent, IsPrivate,
                            CreatedByUserID, CreatedDate, PortalID
                        ) VALUES (
                            @ServiceProviderID, @NoteType, @NoteContent, @IsPrivate,
                            @CreatedByUserID, GETDATE(), @PortalID
                        );
                        SELECT SCOPE_IDENTITY();";

                    command.Parameters.AddWithValue("@ServiceProviderID", note.ServiceProviderID);
                    command.Parameters.AddWithValue("@NoteType", note.NoteType);
                    command.Parameters.AddWithValue("@NoteContent", note.NoteContent);
                    command.Parameters.AddWithValue("@IsPrivate", note.IsPrivate);
                    command.Parameters.AddWithValue("@CreatedByUserID", note.CreatedByUserID);
                    command.Parameters.AddWithValue("@PortalID", note.PortalID);

                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public List<SPNote> GetNotes(int serviceProviderId, int portalId)
        {
            var notes = new List<SPNote>();

            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandText = @"
                        SELECT n.*, u.DisplayName as CreatedByName
                        FROM FinSPNotes n
                        INNER JOIN Users u ON n.CreatedByUserID = u.UserID
                        WHERE n.ServiceProviderID = @ServiceProviderID 
                        AND n.PortalID = @PortalID
                        ORDER BY n.CreatedDate DESC";

                    command.Parameters.AddWithValue("@ServiceProviderID", serviceProviderId);
                    command.Parameters.AddWithValue("@PortalID", portalId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            notes.Add(ReadNoteFromDataReader(reader));
                        }
                    }
                }
            }

            return notes;
        }

        // Timeline
        public List<SPTimelineItem> GetTimeline(int serviceProviderId, int portalId)
        {
            var items = new List<SPTimelineItem>();

            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "FinHubAddOns_GetSPTimeline";

                    command.Parameters.AddWithValue("@ServiceProviderID", serviceProviderId);
                    command.Parameters.AddWithValue("@PortalID", portalId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            items.Add(new SPTimelineItem
                            {
                                ItemType = reader["ItemType"].ToString(),
                                ItemID = Convert.ToInt32(reader["ItemID"]),
                                Title = reader["Title"].ToString(),
                                Description = reader["Description"] != DBNull.Value ? reader["Description"].ToString() : null,
                                ItemDate = Convert.ToDateTime(reader["ItemDate"]),
                                CreatedByName = reader["CreatedByName"].ToString(),
                                CreatedByUserID = Convert.ToInt32(reader["CreatedByUserID"]),
                                SubType = reader["SubType"] != DBNull.Value ? reader["SubType"].ToString() : null,
                                Rating = reader["Rating"] != DBNull.Value ? Convert.ToInt32(reader["Rating"]) : (int?)null,
                                Status = reader["Status"] != DBNull.Value ? reader["Status"].ToString() : null,
                                Priority = reader["Priority"] != DBNull.Value ? reader["Priority"].ToString() : null
                            });
                        }
                    }
                }
            }

            return items;
        }

        // Profile Statistics
        public SPProfileStats GetProfileStats(int serviceProviderId, int portalId)
        {
            using (var connection = new System.Data.SqlClient.SqlConnection(DataProvider.Instance().ConnectionString))
            {
                connection.Open();
                using (var command = new System.Data.SqlClient.SqlCommand())
                {
                    command.Connection = connection;
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = "FinHubAddOns_GetSPProfileStats";

                    command.Parameters.AddWithValue("@ServiceProviderID", serviceProviderId);
                    command.Parameters.AddWithValue("@PortalID", portalId);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new SPProfileStats
                            {
                                TotalOffers = Convert.ToInt32(reader["TotalOffers"]),
                                OffersWon = Convert.ToInt32(reader["OffersWon"]),
                                TotalOfferValue = Convert.ToDecimal(reader["TotalOfferValue"]),
                                AverageOfferValue = Convert.ToDecimal(reader["AverageOfferValue"]),
                                ProjectsBidOn = Convert.ToInt32(reader["ProjectsBidOn"]),
                                ProjectsAwarded = Convert.ToInt32(reader["ProjectsAwarded"]),
                                TotalProjectValue = Convert.ToDecimal(reader["TotalProjectValue"]),
                                AverageRating = Convert.ToDecimal(reader["AverageRating"]),
                                TotalRatings = Convert.ToInt32(reader["TotalRatings"]),
                                TotalActions = Convert.ToInt32(reader["TotalActions"]),
                                RecentActions = Convert.ToInt32(reader["RecentActions"]),
                                OpenTasks = Convert.ToInt32(reader["OpenTasks"]),
                                CompletedTasks = Convert.ToInt32(reader["CompletedTasks"]),
                                WinRate = Convert.ToDecimal(reader["WinRate"]),
                                EstimatedProfit = Convert.ToDecimal(reader["EstimatedProfit"])
                            };
                        }
                    }
                }
            }

            return new SPProfileStats();
        }

        // Helper methods
        private SPAction ReadActionFromDataReader(IDataReader reader)
        {
            return new SPAction
            {
                ActionID = Convert.ToInt32(reader["ActionID"]),
                ServiceProviderID = Convert.ToInt32(reader["ServiceProviderID"]),
                ActionType = reader["ActionType"].ToString(),
                ActionTitle = reader["ActionTitle"].ToString(),
                ActionDescription = reader["ActionDescription"] != DBNull.Value ? reader["ActionDescription"].ToString() : null,
                ActionDate = Convert.ToDateTime(reader["ActionDate"]),
                CreatedByUserID = Convert.ToInt32(reader["CreatedByUserID"]),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                PortalID = Convert.ToInt32(reader["PortalID"]),
                CreatedByName = reader["CreatedByName"].ToString()
            };
        }

        private SPRating ReadRatingFromDataReader(IDataReader reader)
        {
            return new SPRating
            {
                RatingID = Convert.ToInt32(reader["RatingID"]),
                ServiceProviderID = Convert.ToInt32(reader["ServiceProviderID"]),
                Rating = Convert.ToInt32(reader["Rating"]),
                RatingCategory = reader["RatingCategory"].ToString(),
                Comments = reader["Comments"] != DBNull.Value ? reader["Comments"].ToString() : null,
                RatedByUserID = Convert.ToInt32(reader["RatedByUserID"]),
                RatedDate = Convert.ToDateTime(reader["RatedDate"]),
                PortalID = Convert.ToInt32(reader["PortalID"]),
                RatedByName = reader["RatedByName"].ToString()
            };
        }

        private SPTask ReadTaskFromDataReader(IDataReader reader)
        {
            return new SPTask
            {
                TaskID = Convert.ToInt32(reader["TaskID"]),
                ServiceProviderID = Convert.ToInt32(reader["ServiceProviderID"]),
                TaskTitle = reader["TaskTitle"].ToString(),
                TaskDescription = reader["TaskDescription"] != DBNull.Value ? reader["TaskDescription"].ToString() : null,
                AssignedByUserID = Convert.ToInt32(reader["AssignedByUserID"]),
                AssignedToUserID = Convert.ToInt32(reader["AssignedToUserID"]),
                DueDate = reader["DueDate"] != DBNull.Value ? Convert.ToDateTime(reader["DueDate"]) : (DateTime?)null,
                Priority = reader["Priority"].ToString(),
                Status = reader["Status"].ToString(),
                CompletedDate = reader["CompletedDate"] != DBNull.Value ? Convert.ToDateTime(reader["CompletedDate"]) : (DateTime?)null,
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                LastModifiedDate = reader["LastModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["LastModifiedDate"]) : (DateTime?)null,
                PortalID = Convert.ToInt32(reader["PortalID"]),
                AssignedByName = reader["AssignedByName"].ToString(),
                AssignedToName = reader["AssignedToName"].ToString()
            };
        }

        private SPNote ReadNoteFromDataReader(IDataReader reader)
        {
            return new SPNote
            {
                NoteID = Convert.ToInt32(reader["NoteID"]),
                ServiceProviderID = Convert.ToInt32(reader["ServiceProviderID"]),
                NoteType = reader["NoteType"].ToString(),
                NoteContent = reader["NoteContent"].ToString(),
                IsPrivate = Convert.ToBoolean(reader["IsPrivate"]),
                CreatedByUserID = Convert.ToInt32(reader["CreatedByUserID"]),
                CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                LastModifiedDate = reader["LastModifiedDate"] != DBNull.Value ? Convert.ToDateTime(reader["LastModifiedDate"]) : (DateTime?)null,
                PortalID = Convert.ToInt32(reader["PortalID"]),
                CreatedByName = reader["CreatedByName"].ToString()
            };
        }
    }
}