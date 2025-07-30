Vue.use(VueLoading);
Vue.component("loading", VueLoading);

var FinHubAddOns = FinHubAddOns || {};

FinHubAddOns.ServiceProviderComponent = {
    template: "#service-provider-template",

    data: function () {
        var service = {
            path: "FinHubAddOns",
            framework: $.ServicesFramework(this.moduleId)
        };
        service.baseUrl = service.framework.getServiceRoot(service.path) + "UserRole/";

        return {
            service: service,
            isLoading: false,
            users: [],
            filteredUsers: [],

            // Pagination
            currentPage: 1,
            pageSize: 100,

            // Sorting
            sortColumn: 'DisplayName',
            sortDirection: 'asc',

            // Filters
            filters: {
                search: '',
                city: '',
                state: '',
                country: '',
                paymentStatus: '',
                subscriptionStatus: ''
            },

            // Unique values for filters
            uniqueCities: [],
            uniqueStates: [],
            uniqueCountries: [],

            // Edit mode
            editingExpiration: null,
            newExpirationDate: '',

            // Edit user modal
            editingUser: null,
            editForm: {
                firstName: '',
                lastName: '',
                displayName: '',
                email: '',
                phone: '',
                mobile: '',
                street: '',
                city: '',
                stateRegion: '',
                postalCode: '',
                country: '',
                roleExpirationDate: '',
                isAuthorized: true,
                isDeleted: false
            },

            // Confirmation modal
            showConfirmModal: false,
            userToRemove: null,

            // Action menu
            actionMenuOpen: null,

            // Payment modal
            showPaymentModal: false,
            paymentForm: {
                userId: null,
                userName: '',
                planId: null,
                amount: 0,
                discountAmount: 0,
                amountPaid: 0,
                paymentDate: new Date().toISOString().split('T')[0],
                paymentMethod: 'BankTransfer',
                referenceNumber: '',
                notes: '',
                subscriptionStartDate: new Date().toISOString().split('T')[0],
                subscriptionEndDate: ''
            },
            subscriptionPlans: [],

            // Payment history modal
            showPaymentHistoryModal: false,
            paymentHistory: [],
            viewingPaymentHistoryUser: null,

            // Payment statistics
            paymentStats: {
                totalPayments: 0,
                totalRevenue: 0,
                totalDiscounts: 0,
                totalNetRevenue: 0,
                revenueThisMonth: 0,
                revenueThisYear: 0
            },

            // Year selection for statistics
            selectedYear: new Date().getFullYear(),
            selectedYearRevenue: 0,
            availableYears: []
        };
    },

    props: {
        moduleId: {
            required: true,
            type: Number
        }
    },

    computed: {
        totalPages: function () {
            return Math.ceil(this.filteredUsers.length / this.pageSize);
        },

        paginatedUsers: function () {
            var start = (this.currentPage - 1) * this.pageSize;
            var end = start + this.pageSize;
            return this.filteredUsers.slice(start, end);
        },

        expiringIn30Days: function () {
            var today = new Date();
            var thirtyDaysFromNow = new Date();
            thirtyDaysFromNow.setDate(today.getDate() + 30);

            return this.users.filter(function (user) {
                if (!user.RoleExpirationDate) return false;
                var expDate = new Date(user.RoleExpirationDate);
                return expDate > today && expDate <= thirtyDaysFromNow;
            }).length;
        },

        paidUsers: function () {
            return this.users.filter(function (user) {
                return user.PaymentStatus === 'Paid';
            }).length;
        },

        unpaidUsers: function () {
            var today = new Date();
            return this.users.filter(function (user) {
                // Unpaid if payment status is unpaid AND role hasn't expired
                if (!user.RoleExpirationDate) {
                    return user.PaymentStatus === 'Unpaid';
                }
                var expDate = new Date(user.RoleExpirationDate);
                return user.PaymentStatus === 'Unpaid' && expDate > today;
            }).length;
        },

        expiredUsers: function () {
            var today = new Date();
            return this.users.filter(function (user) {
                if (!user.RoleExpirationDate) return false;
                var expDate = new Date(user.RoleExpirationDate);
                return expDate <= today;
            }).length;
        }
    },

    created: function () {
        this.loadData();
        this.loadSubscriptionPlans();
        this.loadPaymentStatistics();
        this.initializeYearSelector();
    },

    methods: {
        get: function (url, data) {
            return $.ajax({
                method: "GET",
                url: this.service.baseUrl + url,
                data: data,
                dataType: "json",
                contentType: "application/json; charset=UTF-8",
                beforeSend: this.service.framework.setModuleHeaders
            });
        },

        post: function (url, data) {
            return $.ajax({
                method: "POST",
                url: this.service.baseUrl + url,
                data: JSON.stringify(data),
                dataType: "json",
                contentType: "application/json; charset=UTF-8",
                beforeSend: this.service.framework.setModuleHeaders
            });
        },

        loadData: function () {
            var self = this;
            self.isLoading = true;

            self.get("GetServiceProviders").done(function (response) {
                self.users = response;
                self.extractUniqueValues();
                self.applyFilters();
            }).fail(function (xhr, status, error) {
                console.error("API Error:", status, error);
                console.error("Response:", xhr.responseText);
                toastr.error("Error: Unable to load Service Provider users");
            }).always(function () {
                self.isLoading = false;
            });
        },

        refreshData: function () {
            this.loadData();
            this.loadPaymentStatistics();
            this.loadPaymentStatisticsForYear();
        },

        extractUniqueValues: function () {
            var cities = new Set();
            var states = new Set();
            var countries = new Set();

            this.users.forEach(function (user) {
                if (user.City) cities.add(user.City.trim());
                if (user.StateRegion) states.add(user.StateRegion);
                if (user.Country) countries.add(user.Country);
            });

            this.uniqueCities = Array.from(cities).sort();
            this.uniqueStates = Array.from(states).sort();
            this.uniqueCountries = Array.from(countries).sort();
        },

        applyFilters: function () {
            var self = this;
            var filtered = this.users;

            // Search filter
            if (this.filters.search) {
                var search = this.filters.search.toLowerCase();
                filtered = filtered.filter(function (user) {
                    return (user.DisplayName && user.DisplayName.toLowerCase().includes(search)) ||
                        (user.Username && user.Username.toLowerCase().includes(search)) ||
                        (user.Email && user.Email.toLowerCase().includes(search)) ||
                        (user.FirstName && user.FirstName.toLowerCase().includes(search)) ||
                        (user.LastName && user.LastName.toLowerCase().includes(search));
                });
            }

            // Location filters
            if (this.filters.city) {
                filtered = filtered.filter(function (user) {
                    return user.City === self.filters.city;
                });
            }

            if (this.filters.state) {
                filtered = filtered.filter(function (user) {
                    return user.StateRegion === self.filters.state;
                });
            }

            if (this.filters.country) {
                filtered = filtered.filter(function (user) {
                    return user.Country === self.filters.country;
                });
            }

            // Payment status filter
            if (this.filters.paymentStatus) {
                filtered = filtered.filter(function (user) {
                    return user.PaymentStatus === self.filters.paymentStatus;
                });
            }

            // Subscription status filter
            if (this.filters.subscriptionStatus) {
                var today = new Date();
                var thirtyDaysFromNow = new Date();
                thirtyDaysFromNow.setDate(today.getDate() + 30);

                filtered = filtered.filter(function (user) {
                    var status = self.getSubscriptionStatus(user);
                    return status === self.filters.subscriptionStatus;
                });
            }

            // Apply sorting
            filtered.sort(function (a, b) {
                var aVal = a[self.sortColumn];
                var bVal = b[self.sortColumn];

                if (aVal === null || aVal === undefined) aVal = '';
                if (bVal === null || bVal === undefined) bVal = '';

                if (typeof aVal === 'string') {
                    aVal = aVal.toLowerCase();
                    bVal = bVal.toLowerCase();
                }

                if (aVal < bVal) return self.sortDirection === 'asc' ? -1 : 1;
                if (aVal > bVal) return self.sortDirection === 'asc' ? 1 : -1;
                return 0;
            });

            this.filteredUsers = filtered;
            this.currentPage = 1; // Reset to first page when filters change
        },

        clearFilters: function () {
            this.filters = {
                search: '',
                city: '',
                state: '',
                country: '',
                paymentStatus: '',
                subscriptionStatus: ''
            };
            this.applyFilters();
        },

        sortBy: function (column) {
            if (this.sortColumn === column) {
                this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
            } else {
                this.sortColumn = column;
                this.sortDirection = 'asc';
            }
            this.applyFilters();
        },

        getSortIcon: function (column) {
            if (this.sortColumn !== column) return '↕';
            return this.sortDirection === 'asc' ? '↑' : '↓';
        },

        formatDate: function (dateString) {
            if (!dateString) return '-';
            var date = new Date(dateString);
            var day = ('0' + date.getDate()).slice(-2);
            var month = ('0' + (date.getMonth() + 1)).slice(-2);
            var year = date.getFullYear();
            return day + '/' + month + '/' + year;
        },

        formatExpiration: function (dateString) {
            if (!dateString) return 'Never';
            return this.formatDate(dateString);
        },

        getStatus: function (user) {
            var today = new Date();

            // Check if user is deleted or unauthorized first
            if (user.IsDeleted) {
                return 'Deleted';
            }

            if (!user.IsAuthorized) {
                return 'Unauthorized';
            }

            // Check if role has expired
            if (user.RoleExpirationDate) {
                var roleExpDate = new Date(user.RoleExpirationDate);
                if (roleExpDate <= today) {
                    return 'Expired';
                }
            }

            // If not expired, check payment status
            if (user.PaymentStatus === 'Unpaid') {
                return 'Unpaid';
            }

            // If paid, check subscription status
            if (!user.SubscriptionEndDate) {
                return 'Active';
            }

            var subEndDate = new Date(user.SubscriptionEndDate);
            var daysUntilExpiration = Math.floor((subEndDate - today) / (1000 * 60 * 60 * 24));

            if (subEndDate <= today) return 'Expired';
            if (daysUntilExpiration <= 30) return 'Expiring';
            return 'Active';
        },

        getSubscriptionStatus: function (user) {
            var today = new Date();

            // First check role expiration
            if (user.RoleExpirationDate) {
                var roleExpDate = new Date(user.RoleExpirationDate);
                if (roleExpDate <= today) {
                    return 'Expired';
                }
            }

            if (!user.SubscriptionEndDate) {
                return user.PaymentStatus === 'Paid' ? 'Active' : 'No Subscription';
            }

            var endDate = new Date(user.SubscriptionEndDate);
            var daysUntilExpiration = Math.floor((endDate - today) / (1000 * 60 * 60 * 24));

            if (endDate <= today) return 'Expired';
            if (daysUntilExpiration <= 30) return 'Expiring';
            return 'Active';
        },

        getStatusClass: function (user) {
            var status = this.getStatus(user);
            switch (status) {
                case 'Active': return 'status-active';
                case 'Expiring': return 'status-expiring';
                case 'Expired': return 'status-expired';
                case 'Unpaid': return 'status-unpaid';
                case 'Unauthorized': return 'status-unauthorized';
                case 'Deleted': return 'status-deleted';
                case 'No Subscription': return 'status-none';
                default: return '';
            }
        },

        getSubscriptionClass: function (user) {
            var status = this.getSubscriptionStatus(user);
            switch (status) {
                case 'Expired': return 'expired';
                case 'Expiring': return 'expiring-soon';
                default: return '';
            }
        },

        getRowClass: function (user) {
            // Check if user is soft deleted
            if (user.IsDeleted) {
                return 'deleted-row';
            }

            // Check if user is unauthorized
            if (!user.IsAuthorized) {
                return 'unauthorized-row';
            }

            if (user.PaymentStatus === 'Unpaid') {
                return 'unpaid-row';
            }

            var status = this.getSubscriptionStatus(user);
            if (status === 'Expired') return 'expired-row';
            if (status === 'Expiring') return 'expiring-soon-row';
            return '';
        },

        // Statistics click handlers
        filterByExpiring: function () {
            this.clearFilters();
            this.filters.subscriptionStatus = 'Expiring';
            this.applyFilters();
        },

        filterByPaid: function () {
            this.clearFilters();
            this.filters.paymentStatus = 'Paid';
            this.applyFilters();
        },

        filterByUnpaid: function () {
            this.clearFilters();
            this.filters.paymentStatus = 'Unpaid';
            this.applyFilters();
        },

        filterByExpired: function () {
            this.clearFilters();
            this.filters.subscriptionStatus = 'Expired';
            this.applyFilters();
        },

        // Action menu methods
        toggleActionMenu: function (userId) {
            if (this.actionMenuOpen === userId) {
                this.actionMenuOpen = null;
            } else {
                this.actionMenuOpen = userId;
            }
        },

        editUser: function (user) {
            this.actionMenuOpen = null;
            this.editingUser = user;

            // Populate edit form with user data
            this.editForm = {
                firstName: user.FirstName || '',
                lastName: user.LastName || '',
                displayName: user.DisplayName || '',
                email: user.Email || '',
                phone: user.Phone || '',
                mobile: user.Mobile || '',
                street: user.Street || '',
                city: user.City || '',
                stateRegion: user.StateRegion || '',
                postalCode: user.PostalCode || '',
                country: user.Country || '',
                roleExpirationDate: '',
                isAuthorized: user.IsAuthorized || false,
                isDeleted: false // Always start with false for safety
            };

            // Set role expiration date if exists
            if (user.RoleExpirationDate) {
                var date = new Date(user.RoleExpirationDate);
                var year = date.getFullYear();
                var month = ('0' + (date.getMonth() + 1)).slice(-2);
                var day = ('0' + date.getDate()).slice(-2);
                this.editForm.roleExpirationDate = year + '-' + month + '-' + day;
            }
        },

        cancelEdit: function () {
            this.editingUser = null;
            this.editForm = {
                firstName: '',
                lastName: '',
                displayName: '',
                email: '',
                phone: '',
                mobile: '',
                street: '',
                city: '',
                stateRegion: '',
                postalCode: '',
                country: '',
                roleExpirationDate: '',
                isAuthorized: true,
                isDeleted: false
            };
        },

        saveUserEdit: function () {
            var self = this;

            // Confirm if user is trying to delete
            if (this.editForm.isDeleted) {
                if (!confirm('Are you sure you want to soft delete this user? This action can be reversed by an administrator.')) {
                    return;
                }
            }

            self.isLoading = true;

            var data = {
                UserId: this.editingUser.UserId,
                FirstName: this.editForm.firstName,
                LastName: this.editForm.lastName,
                DisplayName: this.editForm.displayName,
                Phone: this.editForm.phone,
                Mobile: this.editForm.mobile,
                Street: this.editForm.street,
                City: this.editForm.city,
                StateRegion: this.editForm.stateRegion,
                PostalCode: this.editForm.postalCode,
                Country: this.editForm.country,
                RoleExpirationDate: this.editForm.roleExpirationDate || null,
                IsAuthorized: this.editForm.isAuthorized,
                IsDeleted: this.editForm.isDeleted
            };

            self.post("UpdateUser", data).done(function (response) {
                var message = "User updated successfully";
                if (data.IsDeleted) {
                    message = "User has been soft deleted";
                }
                toastr.success(message);
                self.cancelEdit();
                self.loadData();
            }).fail(function (xhr, status, error) {
                console.error("Update Error:", xhr.responseText);
                var errorMsg = "Error updating user";
                if (xhr.responseJSON && xhr.responseJSON.Message) {
                    errorMsg = xhr.responseJSON.Message;
                }
                toastr.error(errorMsg);
            }).always(function () {
                self.isLoading = false;
            });
        },

        insertPayment: function (user) {
            this.actionMenuOpen = null;
            this.paymentForm = {
                userId: user.UserId,
                userName: user.DisplayName,
                planId: null,
                amount: 0,
                discountAmount: 0,
                amountPaid: 0,
                paymentDate: new Date().toISOString().split('T')[0],
                paymentMethod: 'BankTransfer',
                referenceNumber: '',
                notes: '',
                subscriptionStartDate: new Date().toISOString().split('T')[0],
                subscriptionEndDate: ''
            };

            // Set default end date based on current subscription
            if (user.SubscriptionEndDate) {
                var currentEnd = new Date(user.SubscriptionEndDate);
                if (currentEnd > new Date()) {
                    // If subscription is still active, start from its end date
                    this.paymentForm.subscriptionStartDate = new Date(currentEnd.getTime() + 86400000).toISOString().split('T')[0];
                }
            }

            this.showPaymentModal = true;
        },

        viewPaymentHistory: function (user) {
            this.actionMenuOpen = null;
            this.viewingPaymentHistoryUser = user;
            this.loadPaymentHistory(user.UserId);
        },

        loadSubscriptionPlans: function () {
            var self = this;
            self.get("GetSubscriptionPlans", { planType: 'ServiceProvider' }).done(function (response) {
                self.subscriptionPlans = response;
            }).fail(function (xhr, status, error) {
                console.error("Error loading subscription plans:", xhr.responseText);
            });
        },

        loadPaymentStatistics: function () {
            var self = this;
            self.get("GetPaymentStatistics").done(function (response) {
                self.paymentStats = {
                    totalPayments: response.totalPayments || 0,
                    totalRevenue: response.totalRevenue || 0,
                    totalDiscounts: response.totalDiscounts || 0,
                    totalNetRevenue: response.totalNetRevenue || 0,
                    revenueThisMonth: response.monthlyRevenue || 0,
                    revenueThisYear: response.yearlyRevenue || 0
                };
                // Load current year revenue
                self.loadPaymentStatisticsForYear();
            }).fail(function (xhr, status, error) {
                console.error("Error loading payment statistics:", xhr.responseText);
            });
        },

        initializeYearSelector: function () {
            var currentYear = new Date().getFullYear();
            var startYear = 2020; // Adjust based on when your system started

            this.availableYears = [];
            for (var year = currentYear; year >= startYear; year--) {
                this.availableYears.push(year);
            }
        },

        loadPaymentStatisticsForYear: function () {
            var self = this;
            self.get("GetPaymentStatisticsByYear", { year: self.selectedYear }).done(function (response) {
                self.selectedYearRevenue = response.yearlyRevenue || 0;
            }).fail(function (xhr, status, error) {
                console.error("Error loading year statistics:", xhr.responseText);
                self.selectedYearRevenue = 0;
            });
        },

        loadPaymentHistory: function (userId) {
            var self = this;
            self.isLoading = true;

            self.get("GetPaymentHistory", { userId: userId, paymentType: 'ServiceProvider' }).done(function (response) {
                self.paymentHistory = response;
                self.showPaymentHistoryModal = true;
            }).fail(function (xhr, status, error) {
                console.error("Error loading payment history:", xhr.responseText);
                toastr.error("Error loading payment history");
            }).always(function () {
                self.isLoading = false;
            });
        },

        selectPlan: function (plan) {
            this.paymentForm.planId = plan.PlanID;
            this.paymentForm.amount = plan.Amount;
            this.calculateAmountPaid();

            // Calculate end date based on plan duration
            var startDate = new Date(this.paymentForm.subscriptionStartDate);
            var endDate = new Date(startDate);
            endDate.setMonth(endDate.getMonth() + plan.DurationMonths);

            // Format date for input
            var year = endDate.getFullYear();
            var month = ('0' + (endDate.getMonth() + 1)).slice(-2);
            var day = ('0' + endDate.getDate()).slice(-2);
            this.paymentForm.subscriptionEndDate = year + '-' + month + '-' + day;
        },

        calculateAmountPaid: function () {
            var amount = parseFloat(this.paymentForm.amount) || 0;
            var discount = parseFloat(this.paymentForm.discountAmount) || 0;
            this.paymentForm.amountPaid = Math.max(0, amount - discount);
        },

        cancelPayment: function () {
            this.showPaymentModal = false;
            this.paymentForm = {
                userId: null,
                userName: '',
                planId: null,
                amount: 0,
                discountAmount: 0,
                amountPaid: 0,
                paymentDate: new Date().toISOString().split('T')[0],
                paymentMethod: 'BankTransfer',
                referenceNumber: '',
                notes: '',
                subscriptionStartDate: new Date().toISOString().split('T')[0],
                subscriptionEndDate: ''
            };
        },

        savePayment: function () {
            var self = this;

            if (!this.paymentForm.planId) {
                toastr.warning("Please select a subscription plan");
                return;
            }

            if (!this.paymentForm.referenceNumber) {
                toastr.warning("Please enter a reference number");
                return;
            }

            self.isLoading = true;

            var data = {
                UserID: this.paymentForm.userId,
                PaymentType: 'ServiceProvider',
                PlanID: this.paymentForm.planId,
                Amount: this.paymentForm.amount,
                DiscountAmount: this.paymentForm.discountAmount,
                PaymentDate: this.paymentForm.paymentDate,
                PaymentMethod: this.paymentForm.paymentMethod,
                ReferenceNumber: this.paymentForm.referenceNumber,
                Notes: this.paymentForm.notes,
                SubscriptionStartDate: this.paymentForm.subscriptionStartDate,
                SubscriptionEndDate: this.paymentForm.subscriptionEndDate
            };

            self.post("RecordPayment", data).done(function (response) {
                toastr.success("Payment recorded successfully");
                self.cancelPayment();

                // Update role expiration date to match subscription end date
                var updateData = {
                    UserId: data.UserID,
                    ExpirationDate: data.SubscriptionEndDate
                };

                self.post("UpdateRoleExpiration", updateData).done(function () {
                    self.loadData();
                    self.loadPaymentStatistics();
                }).fail(function (xhr) {
                    console.error("Error updating role expiration:", xhr.responseText);
                });
            }).fail(function (xhr, status, error) {
                console.error("Payment Error:", xhr.responseText);
                var errorMsg = "Error recording payment";
                if (xhr.responseJSON && xhr.responseJSON.Message) {
                    errorMsg = xhr.responseJSON.Message;
                }
                toastr.error(errorMsg);
            }).always(function () {
                self.isLoading = false;
            });
        },

        closePaymentHistory: function () {
            this.showPaymentHistoryModal = false;
            this.paymentHistory = [];
            this.viewingPaymentHistoryUser = null;
        },

        getPaymentStatusClass: function (status) {
            switch (status) {
                case 'Active': return 'status-active';
                case 'Expired': return 'status-expired';
                case 'Cancelled': return 'status-cancelled';
                case 'Refunded': return 'status-refunded';
                case 'Pending': return 'status-pending';
                default: return '';
            }
        },

        startEditExpiration: function (user) {
            this.editingExpiration = user.UserId;
            if (user.RoleExpirationDate) {
                var date = new Date(user.RoleExpirationDate);
                // Use local date components instead of ISO string
                var year = date.getFullYear();
                var month = ('0' + (date.getMonth() + 1)).slice(-2);
                var day = ('0' + date.getDate()).slice(-2);
                this.newExpirationDate = year + '-' + month + '-' + day;
            } else {
                // Default to 1 year from today
                var date = new Date();
                date.setFullYear(date.getFullYear() + 1);
                var year = date.getFullYear();
                var month = ('0' + (date.getMonth() + 1)).slice(-2);
                var day = ('0' + date.getDate()).slice(-2);
                this.newExpirationDate = year + '-' + month + '-' + day;
            }
        },

        cancelExpiration: function () {
            this.editingExpiration = null;
            this.newExpirationDate = '';
        },

        saveExpiration: function (user) {
            var self = this;
            self.isLoading = true;

            var data = {
                UserId: user.UserId,
                ExpirationDate: this.newExpirationDate || null
            };

            self.post("UpdateRoleExpiration", data).done(function (response) {
                toastr.success("Expiration date updated successfully");
                self.cancelExpiration();
                self.loadData();
            }).fail(function (xhr, status, error) {
                console.error("Update Error - Status:", xhr.status);
                console.error("Response Text:", xhr.responseText);
                console.error("Response JSON:", xhr.responseJSON);

                var errorMsg = "Error updating expiration date";
                if (xhr.responseJSON && xhr.responseJSON.Message) {
                    errorMsg = xhr.responseJSON.Message;
                }
                toastr.error(errorMsg);
            }).always(function () {
                self.isLoading = false;
            });
        },

        confirmRemoveUser: function (user) {
            this.actionMenuOpen = null;
            this.userToRemove = user;
            this.showConfirmModal = true;
        },

        cancelRemove: function () {
            this.userToRemove = null;
            this.showConfirmModal = false;
        },

        removeUser: function () {
            var self = this;
            self.isLoading = true;

            var data = {
                UserId: this.userToRemove.UserId
            };

            self.post("RemoveUserFromRole", data).done(function (response) {
                toastr.success("User removed from Service Provider role");
                self.cancelRemove();
                self.loadData();
            }).fail(function (xhr, status, error) {
                console.error("Remove Error:", xhr.responseText);
                toastr.error("Error removing user from role");
            }).always(function () {
                self.isLoading = false;
            });
        }
    }
};