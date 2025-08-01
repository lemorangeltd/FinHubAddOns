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

            // Filters - UPDATED to use userStatus instead of separate payment/subscription
            filters: {
                search: '',
                registrationPeriod: '',
                userStatus: '',
                city: '',
                state: '',
                country: '',
                hideDeleted: true  // Default to hiding deleted users
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
            availableYears: [],

            // Geographic breakdown toggle
            showGeographicBreakdown: false,

            // Service Provider Profile
            showProfilePanel: false,
            selectedProfile: null,
            activeProfileTab: 'overview',
            profileData: {
                stats: null,
                actions: [],
                tasks: [],
                ratings: [],
                projects: []
            },

            // Action form
            showActionForm: false,
            actionForm: {
                actionType: 'Note',
                actionTitle: '',
                actionDescription: '',
                actionDate: new Date().toISOString().split('T')[0],
                isPrivate: false,
                priority: 'Medium'
            },

            // Rating form
            showRatingForm: false,
            ratingForm: {
                rating: 5,
                ratingCategory: 'Overall',
                comments: ''
            },

            // Task form
            showTaskForm: false,
            taskForm: {
                taskTitle: '',
                taskDescription: '',
                assignedToUserId: null,
                dueDate: '',
                priority: 'Medium'
            },
            managers: []
        };
    },

    props: {
        moduleId: {
            required: true,
            type: Number
        }
    },

    watch: {
        'editForm.isAuthorized': function (newVal, oldVal) {
            console.log('Authorization checkbox changed from', oldVal, 'to', newVal);
        },
        'editForm.isDeleted': function (newVal, oldVal) {
            console.log('Deleted checkbox changed from', oldVal, 'to', newVal);
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

        // Updated to use new status system
        expiringIn30Days: function () {
            var self = this;
            return this.users.filter(function (user) {
                return self.isExpiringWithin30Days(user);
            }).length;
        },

        // Updated KPIs with new 5-status system
        trialUsers: function () {
            var self = this;
            return this.users.filter(function (user) {
                return self.getStatus(user) === 'Trial';
            }).length;
        },

        activeUsers: function () {
            var self = this;
            return this.users.filter(function (user) {
                return self.getStatus(user) === 'Active';
            }).length;
        },

        activeUnpaidUsers: function () {
            var self = this;
            return this.users.filter(function (user) {
                return self.getStatus(user) === 'Active + Unpaid';
            }).length;
        },

        expiredNeverPaidUsers: function () {
            var self = this;
            return this.users.filter(function (user) {
                return self.getStatus(user) === 'Expired + Never Paid';
            }).length;
        },

        lapsedUsers: function () {
            var self = this;
            return this.users.filter(function (user) {
                return self.getStatus(user) === 'Lapsed';
            }).length;
        },

        // Legacy computed properties for backwards compatibility
        paidUsers: function () {
            return this.activeUsers;
        },

        unpaidUsers: function () {
            return this.activeUnpaidUsers + this.expiredNeverPaidUsers + this.lapsedUsers;
        },

        expiredUsers: function () {
            return this.expiredNeverPaidUsers + this.lapsedUsers;
        },

        // Updated computed properties
        newRegistrationsThisMonth: function () {
            var today = new Date();
            var thisMonth = new Date(today.getFullYear(), today.getMonth(), 1);
            return this.users.filter(function (user) {
                if (!user.RoleStartDate) return false;
                var registrationDate = new Date(user.RoleStartDate);
                return registrationDate >= thisMonth;
            }).length;
        },

        paidPercentage: function () {
            if (this.users.length === 0) return 0;
            return Math.round((this.activeUsers / this.users.length) * 100);
        },

        averageRevenuePerUser: function () {
            if (this.activeUsers === 0) return 0;
            return this.paymentStats.totalRevenue / this.activeUsers;
        },

        conversionRate: function () {
            var totalWithPaymentHistory = this.activeUsers + this.activeUnpaidUsers + this.lapsedUsers;
            var totalUsers = this.users.length;
            if (totalUsers === 0) return 0;
            return Math.round((totalWithPaymentHistory / totalUsers) * 100);
        },

        topCountriesCount: function () {
            var countries = new Set();
            this.users.forEach(function (user) {
                if (user.Country) countries.add(user.Country);
            });
            return countries.size;
        },

        topCountries: function () {
            var countryCount = {};
            var total = this.users.length;

            this.users.forEach(function (user) {
                var country = user.Country || 'Unknown';
                countryCount[country] = (countryCount[country] || 0) + 1;
            });

            return Object.keys(countryCount)
                .map(function (country) {
                    return {
                        name: country,
                        count: countryCount[country],
                        percentage: Math.round((countryCount[country] / total) * 100)
                    };
                })
                .sort(function (a, b) { return b.count - a.count; })
                .slice(0, 8); // Top 8 countries
        },

        activeUsersPercent: function () {
            if (this.users.length === 0) return 0;
            return Math.round((this.activeUsers / this.users.length) * 100);
        },

        registrationTrend: function () {
            var today = new Date();
            var last90Days = new Date();
            last90Days.setDate(today.getDate() - 90);
            var previous90Days = new Date();
            previous90Days.setDate(today.getDate() - 180);

            var recent = this.users.filter(function (user) {
                if (!user.RoleStartDate) return false;
                var regDate = new Date(user.RoleStartDate);
                return regDate >= last90Days;
            }).length;

            var previous = this.users.filter(function (user) {
                if (!user.RoleStartDate) return false;
                var regDate = new Date(user.RoleStartDate);
                return regDate >= previous90Days && regDate < last90Days;
            }).length;

            if (previous === 0) return recent > 0 ? 100 : 0;
            return Math.round(((recent - previous) / previous) * 100);
        },

        monthlyRecurringRevenue: function () {
            // Calculate MRR based on active subscriptions
            var today = new Date();
            var mrr = 0;

            this.users.forEach(function (user) {
                if (user.PaymentStatus === 'Paid' && user.SubscriptionEndDate) {
                    var endDate = new Date(user.SubscriptionEndDate);
                    if (endDate > today) {
                        // Estimate monthly revenue - this is simplified
                        // In reality, you'd want to track actual subscription amounts
                        mrr += 50; // Assume average €50/month per active user
                    }
                }
            });

            return mrr;
        }
    },

    created: function () {
        this.loadData();
        this.loadSubscriptionPlans();
        this.loadPaymentStatistics();
        this.initializeYearSelector();
        this.loadManagers();
    },

    // MOUNTED MOVED HERE - OUT OF METHODS
    mounted: function () {
        var self = this;

        // Global click handler
        document.addEventListener('click', function (e) {
            // Close menu if clicking outside
            if (!e.target.closest('.action-dropdown') && !e.target.closest('.action-menu-portal')) {
                var existingMenu = document.querySelector('.action-menu-portal');
                if (existingMenu) {
                    existingMenu.remove();
                }
                self.actionMenuOpen = null;
            }
        });

        // Handle scroll/resize to reposition or close menu
        var closePortalMenu = function () {
            var existingMenu = document.querySelector('.action-menu-portal');
            if (existingMenu) {
                existingMenu.remove();
                self.actionMenuOpen = null;
            }
        };

        window.addEventListener('scroll', closePortalMenu, true);
        window.addEventListener('resize', closePortalMenu);

        // Add handler for ESC key to close profile panel
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape' && self.showProfilePanel) {
                self.closeProfile();
            }
        });
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

        // ADDED formatCurrency method
        formatCurrency: function (amount) {
            if (amount === null || amount === undefined) return '€0.00';

            // Convert to number if it's not already
            var num = parseFloat(amount);

            // Format with thousands separator and 2 decimal places
            return '€' + num.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ',');
        },

        // Format rating stars
        formatRatingStars: function (rating) {
            var stars = '';
            for (var i = 1; i <= 5; i++) {
                stars += i <= rating ? '★' : '☆';
            }
            return stars;
        },

        // Get user initials for avatar
        getInitials: function (displayName) {
            if (!displayName) return '??';
            var names = displayName.split(' ');
            if (names.length >= 2) {
                return names[0].charAt(0).toUpperCase() + names[1].charAt(0).toUpperCase();
            }
            return displayName.charAt(0).toUpperCase() + (displayName.charAt(1) || '').toUpperCase();
        },

        // NEW: Check if user has payment history (for distinguishing Never Paid vs Unpaid)
        hasPaymentHistory: function (user) {
            // This should check if user has any payment records
            // For now, we'll use a heuristic based on available data
            // In a real implementation, you'd check against a payment history table
            return user.PaymentStatus === 'Paid' ||
                user.SubscriptionEndDate ||
                user.LastPaymentDate ||
                (user.TotalPayments && user.TotalPayments > 0);
        },

        // NEW: Check if subscription is expiring within 30 days
        isExpiringWithin30Days: function (user) {
            var today = new Date();
            var thirtyDaysFromNow = new Date();
            thirtyDaysFromNow.setDate(today.getDate() + 30);

            // Check role expiration date
            if (user.RoleExpirationDate) {
                var roleExpDate = new Date(user.RoleExpirationDate);
                if (roleExpDate > today && roleExpDate <= thirtyDaysFromNow) {
                    return true;
                }
            }

            // Also check subscription end date for active users
            if (user.SubscriptionEndDate && this.getStatus(user) === 'Active') {
                var subExpDate = new Date(user.SubscriptionEndDate);
                if (subExpDate > today && subExpDate <= thirtyDaysFromNow) {
                    return true;
                }
            }

            return false;
        },

        // NEW: Simplified 5-status system
        getStatus: function (user) {
            var today = new Date();
            var registrationDate = new Date(user.RoleStartDate);
            var hoursSinceRegistration = (today - registrationDate) / (1000 * 60 * 60);

            // Check if user is in trial period (≤48 hours since registration AND never paid)
            if (hoursSinceRegistration <= 48 && !this.hasPaymentHistory(user)) {
                return 'Trial';
            }

            // Check if subscription is currently valid (not expired)
            var hasValidSubscription = this.hasValidSubscription(user);

            if (hasValidSubscription) {
                // Subscription dates are valid
                if (user.PaymentStatus === 'Paid') {
                    return 'Active';
                } else {
                    return 'Active + Unpaid';
                }
            } else {
                // Subscription has expired
                if (this.hasPaymentHistory(user)) {
                    return 'Lapsed';
                } else {
                    return 'Expired + Never Paid';
                }
            }
        },

        // NEW: Check if user has valid subscription dates
        hasValidSubscription: function (user) {
            var today = new Date();

            // Check role expiration date
            if (user.RoleExpirationDate) {
                var roleExpDate = new Date(user.RoleExpirationDate);
                if (roleExpDate > today) {
                    return true;
                }
            }

            // Check subscription end date
            if (user.SubscriptionEndDate) {
                var subEndDate = new Date(user.SubscriptionEndDate);
                if (subEndDate > today) {
                    return true;
                }
            }

            return false;
        },

        // NEW: Get status tooltip text for simplified 5-status system
        getStatusTooltip: function (user) {
            var status = this.getStatus(user);
            var today = new Date();

            switch (status) {
                case 'Trial':
                    var registrationDate = new Date(user.RoleStartDate);
                    var hoursLeft = 48 - ((today - registrationDate) / (1000 * 60 * 60));
                    return 'Trial period - ' + Math.round(hoursLeft) + ' hours remaining. New user, never made a payment. NURTURE & CONVERT!';

                case 'Active':
                    var tooltip = 'Active paid subscription - everything is good!';
                    if (user.SubscriptionEndDate) {
                        var daysUntilExp = Math.floor((new Date(user.SubscriptionEndDate) - today) / (1000 * 60 * 60 * 24));
                        tooltip += ' Expires in ' + daysUntilExp + ' days.';
                        if (this.isExpiringWithin30Days(user)) {
                            tooltip += ' ⚠️ EXPIRING SOON - RETENTION NEEDED!';
                        }
                    }
                    return tooltip;

                case 'Active + Unpaid':
                    var tooltip = 'Subscription dates are valid but no payment received. Could be grace period or payment issue.';
                    if (user.SubscriptionEndDate) {
                        var daysUntilExp = Math.floor((new Date(user.SubscriptionEndDate) - today) / (1000 * 60 * 60 * 24));
                        tooltip += ' Subscription expires in ' + daysUntilExp + ' days.';
                    }
                    tooltip += ' URGENT PAYMENT FOLLOW-UP REQUIRED!';
                    return tooltip;

                case 'Expired + Never Paid':
                    var tooltip = 'Subscription period ended and user never made any payment.';
                    if (user.SubscriptionEndDate) {
                        var daysExpired = Math.floor((today - new Date(user.SubscriptionEndDate)) / (1000 * 60 * 60 * 24));
                        tooltip += ' Expired ' + daysExpired + ' days ago.';
                    }
                    tooltip += ' CONVERSION CAMPAIGN TARGET!';
                    return tooltip;

                case 'Lapsed':
                    var tooltip = 'Had active subscription before but has not renewed.';
                    if (user.SubscriptionEndDate) {
                        var daysExpired = Math.floor((today - new Date(user.SubscriptionEndDate)) / (1000 * 60 * 60 * 24));
                        tooltip += ' Expired ' + daysExpired + ' days ago.';
                    }
                    tooltip += ' WIN-BACK CAMPAIGN OPPORTUNITY!';
                    return tooltip;

                default:
                    return 'Status: ' + status;
            }
        },

        // ADDED method to set registration period via buttons
        setRegistrationPeriod: function (period) {
            this.filters.registrationPeriod = period;
            this.applyFilters();
        },

        // ADDED method to set user status via buttons
        setUserStatus: function (status) {
            this.filters.userStatus = status;
            this.applyFilters();
        },

        // ADDED method to get registration period label
        getRegistrationPeriodLabel: function (period) {
            switch (period) {
                case 'last7days': return 'Last 7 days';
                case 'last30days': return 'Last 30 days';
                case 'last90days': return 'Last 90 days';
                case 'thisyear': return 'This year';
                case 'lastyear': return 'Last year';
                case 'last2years': return 'Last 2 years';
                default: return 'All Time';
            }
        },

        // ADDED method to check if user matches registration period filter
        matchesRegistrationPeriod: function (user, period) {
            if (!period || !user.RoleStartDate) return true;

            var today = new Date();
            var registrationDate = new Date(user.RoleStartDate);

            switch (period) {
                case 'last7days':
                    var sevenDaysAgo = new Date();
                    sevenDaysAgo.setDate(today.getDate() - 7);
                    return registrationDate >= sevenDaysAgo;

                case 'last30days':
                    var thirtyDaysAgo = new Date();
                    thirtyDaysAgo.setDate(today.getDate() - 30);
                    return registrationDate >= thirtyDaysAgo;

                case 'last90days':
                    var ninetyDaysAgo = new Date();
                    ninetyDaysAgo.setDate(today.getDate() - 90);
                    return registrationDate >= ninetyDaysAgo;

                case 'thisyear':
                    var thisYearStart = new Date(today.getFullYear(), 0, 1);
                    return registrationDate >= thisYearStart;

                case 'lastyear':
                    var lastYearStart = new Date(today.getFullYear() - 1, 0, 1);
                    var lastYearEnd = new Date(today.getFullYear() - 1, 11, 31, 23, 59, 59);
                    return registrationDate >= lastYearStart && registrationDate <= lastYearEnd;

                case 'last2years':
                    var twoYearsAgo = new Date();
                    twoYearsAgo.setFullYear(today.getFullYear() - 2);
                    return registrationDate >= twoYearsAgo;

                default:
                    return true;
            }
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

        // UPDATED applyFilters to handle registration period and hideDeleted
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

            // Registration period filter - ADDED
            if (this.filters.registrationPeriod) {
                filtered = filtered.filter(function (user) {
                    return self.matchesRegistrationPeriod(user, self.filters.registrationPeriod);
                });
            }

            // Hide deleted filter
            if (this.filters.hideDeleted) {
                filtered = filtered.filter(function (user) {
                    return !user.IsDeleted;
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

            // User status filter
            if (this.filters.userStatus) {
                filtered = filtered.filter(function (user) {
                    var status = self.getStatus(user);
                    return status === self.filters.userStatus;
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

        // UPDATED clearFilters to include userStatus and maintain hideDeleted state
        clearFilters: function () {
            this.filters = {
                search: '',
                registrationPeriod: '',
                userStatus: '',
                city: '',
                state: '',
                country: '',
                hideDeleted: this.filters.hideDeleted  // Maintain the current hideDeleted state
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

        getSubscriptionStatus: function (user) {
            var today = new Date();

            // First check role expiration - this should be the primary check for "expiring"
            if (user.RoleExpirationDate) {
                var roleExpDate = new Date(user.RoleExpirationDate);
                if (roleExpDate <= today) {
                    return 'Expired';
                }

                // Check if role is expiring within 30 days
                var daysUntilRoleExpiration = Math.floor((roleExpDate - today) / (1000 * 60 * 60 * 24));
                if (daysUntilRoleExpiration <= 30) {
                    return 'Expiring';
                }
            }

            // If role is not expiring, check subscription status
            if (!user.SubscriptionEndDate) {
                return user.PaymentStatus === 'Paid' ? 'Active' : 'No Subscription';
            }

            var subEndDate = new Date(user.SubscriptionEndDate);
            var daysUntilSubExpiration = Math.floor((subEndDate - today) / (1000 * 60 * 60 * 24));

            if (subEndDate <= today) return 'Expired';
            if (daysUntilSubExpiration <= 30) return 'Expiring';
            return 'Active';
        },

        // UPDATED: New status class mapping for 5-status system
        getStatusClass: function (user) {
            var status = this.getStatus(user);
            switch (status) {
                case 'Trial': return 'status-trial';
                case 'Active': return 'status-active';
                case 'Active + Unpaid': return 'status-active-unpaid';
                case 'Expired + Never Paid': return 'status-expired-never-paid';
                case 'Lapsed': return 'status-lapsed';
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

            // Add expiring warning to row class
            if (this.isExpiringWithin30Days(user)) {
                return 'expiring-soon-row';
            }

            var status = this.getStatus(user);
            switch (status) {
                case 'Trial': return 'trial-row';
                case 'Active': return 'active-row';
                case 'Active + Unpaid': return 'active-unpaid-row';
                case 'Expired + Never Paid': return 'expired-never-paid-row';
                case 'Lapsed': return 'lapsed-row';
                default: return '';
            }
        },

        // Statistics click handlers - UPDATED with new 5-status system
        filterByExpiring: function () {
            this.clearFilters();
            // Filter to show users expiring within 30 days
            this.filteredUsers = this.users.filter(function (user) {
                return this.isExpiringWithin30Days(user);
            }.bind(this));
            this.currentPage = 1;
        },

        filterByTrial: function () {
            this.clearFilters();
            this.filters.userStatus = 'Trial';
            this.applyFilters();
        },

        filterByActive: function () {
            this.clearFilters();
            this.filters.userStatus = 'Active';
            this.applyFilters();
        },

        filterByActiveUnpaid: function () {
            this.clearFilters();
            this.filters.userStatus = 'Active + Unpaid';
            this.applyFilters();
        },

        filterByExpiredNeverPaid: function () {
            this.clearFilters();
            this.filters.userStatus = 'Expired + Never Paid';
            this.applyFilters();
        },

        filterByLapsed: function () {
            this.clearFilters();
            this.filters.userStatus = 'Lapsed';
            this.applyFilters();
        },

        // Legacy methods for backwards compatibility
        filterByFreeTrials: function () {
            this.filterByTrial();
        },

        filterByNeverPaid: function () {
            this.filterByExpiredNeverPaid();
        },

        filterByUnpaid: function () {
            this.filterByActiveUnpaid();
        },

        filterByExpired: function () {
            // Show both expired categories
            this.clearFilters();
            this.filteredUsers = this.users.filter(function (user) {
                var status = this.getStatus(user);
                return status === 'Expired + Never Paid' || status === 'Lapsed';
            }.bind(this));
            this.currentPage = 1;
        },

        // NEW: Filter by new registrations this month
        filterByNewRegistrations: function () {
            this.clearFilters();
            this.setRegistrationPeriod('last30days');
        },

        // Tooltip functionality
        showTooltip: function (event, text) {
            var tooltip = document.getElementById('global-tooltip');
            if (!tooltip) return;

            tooltip.textContent = text;
            tooltip.style.display = 'block';

            // Position tooltip
            var rect = event.target.getBoundingClientRect();
            var tooltipRect = tooltip.getBoundingClientRect();
            var scrollLeft = window.pageXOffset || document.documentElement.scrollLeft;
            var scrollTop = window.pageYOffset || document.documentElement.scrollTop;

            var left = rect.left + scrollLeft + (rect.width / 2) - (tooltipRect.width / 2);
            var top = rect.top + scrollTop - tooltipRect.height - 8;

            // Ensure tooltip stays within viewport
            if (left < 10) left = 10;
            if (left + tooltipRect.width > window.innerWidth - 10) {
                left = window.innerWidth - tooltipRect.width - 10;
            }
            if (top < 10) {
                top = rect.bottom + scrollTop + 8;
            }

            tooltip.style.left = left + 'px';
            tooltip.style.top = top + 'px';
        },

        hideTooltip: function () {
            var tooltip = document.getElementById('global-tooltip');
            if (tooltip) {
                tooltip.style.display = 'none';
            }
        },

        // Add this method to handle the action menu as a portal (append to body)

        toggleActionMenu: function (userId) {
            var self = this;

            // Close previous menu if exists
            var existingMenu = document.querySelector('.action-menu-portal');
            if (existingMenu) {
                existingMenu.remove();
            }

            if (this.actionMenuOpen === userId) {
                this.actionMenuOpen = null;
                return;
            }

            this.actionMenuOpen = userId;

            this.$nextTick(function () {
                // Find the button using the data-userid attribute
                var button = document.querySelector('.action-dots[data-userid="' + userId + '"]');

                if (!button) {
                    // Fallback: try to find button by matching UserId in the row
                    var user = self.paginatedUsers.find(function (u) { return u.UserId === userId; });
                    if (user) {
                        // Try to find button by checking each row
                        var buttons = document.querySelectorAll('.action-dots');
                        for (var i = 0; i < buttons.length; i++) {
                            var btn = buttons[i];
                            var row = btn.closest('tr');
                            // Check if this row contains any unique identifier for the user
                            if (row) {
                                // Try multiple ways to identify the correct row
                                var emailCell = row.querySelector('td:nth-child(2) a');
                                if (emailCell && emailCell.textContent === user.Email) {
                                    button = btn;
                                    break;
                                }
                                // If email doesn't work, try display name but be more specific
                                var nameCell = row.querySelector('td:first-child strong');
                                if (nameCell && nameCell.textContent === user.DisplayName) {
                                    button = btn;
                                    break;
                                }
                            }
                        }
                    }

                    if (!button) {
                        console.error('Could not find action button for user ID:', userId);
                        self.actionMenuOpen = null;
                        return;
                    }
                }

                // Get the user data
                var user = self.paginatedUsers.find(function (u) { return u.UserId === userId; });
                if (!user) {
                    console.error('Could not find user data for ID:', userId);
                    self.actionMenuOpen = null;
                    return;
                }

                // Create menu element
                var menu = document.createElement('div');
                menu.className = 'action-menu action-menu-portal';
                menu.innerHTML = '<a class="action-item" data-action="profile" data-userid="' + userId + '">' +
                    '<span class="action-icon">👤</span> View Profile' +
                    '</a>' +
                    '<a class="action-item" data-action="edit" data-userid="' + userId + '">' +
                    '<span class="action-icon">✏️</span> Edit User' +
                    '</a>' +
                    '<a class="action-item" data-action="payment" data-userid="' + userId + '">' +
                    '<span class="action-icon">💳</span> Insert Payment' +
                    '</a>' +
                    '<a class="action-item" data-action="history" data-userid="' + userId + '">' +
                    '<span class="action-icon">📜</span> Payment History' +
                    '</a>' +
                    '<a class="action-item danger" data-action="remove" data-userid="' + userId + '">' +
                    '<span class="action-icon">🗑️</span> Remove from Role' +
                    '</a>';

                // Position the menu
                var rect = button.getBoundingClientRect();
                menu.style.position = 'fixed';
                menu.style.top = (rect.bottom + 5) + 'px';
                menu.style.right = (window.innerWidth - rect.right) + 'px';
                menu.style.zIndex = '999999';

                // Append to body
                document.body.appendChild(menu);

                // Attach event listeners
                menu.addEventListener('click', function (e) {
                    e.preventDefault();
                    e.stopPropagation();

                    var item = e.target.closest('.action-item');
                    if (!item) return;

                    var action = item.getAttribute('data-action');
                    var targetUserId = parseInt(item.getAttribute('data-userid'));
                    var targetUser = self.paginatedUsers.find(function (u) { return u.UserId === targetUserId; });

                    if (!targetUser) {
                        console.error('Could not find target user for action:', action, targetUserId);
                        return;
                    }

                    switch (action) {
                        case 'profile':
                            self.viewProfile(targetUser);
                            break;
                        case 'edit':
                            self.editUser(targetUser);
                            break;
                        case 'payment':
                            self.insertPayment(targetUser);
                            break;
                        case 'history':
                            self.viewPaymentHistory(targetUser);
                            break;
                        case 'remove':
                            self.confirmRemoveUser(targetUser);
                            break;
                    }

                    // Close menu after action
                    menu.remove();
                    self.actionMenuOpen = null;
                });

                // Adjust position if menu goes off-screen
                setTimeout(function () {
                    if (!menu.parentElement) return; // Menu was already removed

                    var menuRect = menu.getBoundingClientRect();

                    if (menuRect.bottom > window.innerHeight) {
                        menu.style.top = (rect.top - menuRect.height - 5) + 'px';
                    }

                    if (menuRect.right > window.innerWidth) {
                        menu.style.right = '10px';
                    }
                }, 10);
            });
        },

        // View Profile method
        viewProfile: function (user) {
            var self = this;
            self.selectedProfile = user;
            self.showProfilePanel = true;
            self.activeProfileTab = 'overview';
            self.isLoading = true;

            // Reset forms
            self.showActionForm = false;
            self.showRatingForm = false;
            self.showTaskForm = false;

            self.get("GetServiceProviderProfile", { userId: user.UserId }).done(function (response) {
                self.profileData = response;
            }).fail(function (xhr, status, error) {
                console.error("Error loading profile:", xhr.responseText);
                toastr.error("Error loading profile data");
            }).always(function () {
                self.isLoading = false;
            });
        },

        closeProfile: function () {
            this.showProfilePanel = false;
            this.selectedProfile = null;
            this.activeProfileTab = 'overview';
            this.profileData = {
                stats: null,
                actions: [],
                tasks: [],
                ratings: [],
                projects: []
            };
            // Reset forms
            this.showActionForm = false;
            this.showRatingForm = false;
            this.showTaskForm = false;
        },

        // Action/Note methods
        toggleActionForm: function () {
            this.showActionForm = !this.showActionForm;
            if (this.showActionForm) {
                this.showRatingForm = false;
                this.showTaskForm = false;
            }
        },

        saveAction: function () {
            var self = this;

            if (!this.actionForm.actionTitle) {
                toastr.warning("Please enter an action title");
                return;
            }

            self.isLoading = true;

            var data = {
                ServiceProviderUserID: this.selectedProfile.UserId,
                ActionType: this.actionForm.actionType,
                ActionTitle: this.actionForm.actionTitle,
                ActionDescription: this.actionForm.actionDescription,
                ActionDate: this.actionForm.actionDate,
                IsPrivate: this.actionForm.isPrivate,
                Priority: this.actionForm.priority
            };

            self.post("AddAction", data).done(function (response) {
                toastr.success("Action added successfully");
                self.resetActionForm();
                self.showActionForm = false;
                // Reload profile data
                self.viewProfile(self.selectedProfile);
            }).fail(function (xhr, status, error) {
                console.error("Error adding action:", xhr.responseText);
                toastr.error("Error adding action");
            }).always(function () {
                self.isLoading = false;
            });
        },

        resetActionForm: function () {
            this.actionForm = {
                actionType: 'Note',
                actionTitle: '',
                actionDescription: '',
                actionDate: new Date().toISOString().split('T')[0],
                isPrivate: false,
                priority: 'Medium'
            };
        },

        // Rating methods
        toggleRatingForm: function () {
            this.showRatingForm = !this.showRatingForm;
            if (this.showRatingForm) {
                this.showActionForm = false;
                this.showTaskForm = false;
            }
        },

        saveRating: function () {
            var self = this;

            self.isLoading = true;

            var data = {
                ServiceProviderUserID: this.selectedProfile.UserId,
                Rating: this.ratingForm.rating,
                RatingCategory: this.ratingForm.ratingCategory,
                Comments: this.ratingForm.comments
            };

            self.post("AddRating", data).done(function (response) {
                toastr.success("Rating added successfully");
                self.resetRatingForm();
                self.showRatingForm = false;
                // Reload profile data
                self.viewProfile(self.selectedProfile);
                // Also reload main data to update average rating
                self.loadData();
            }).fail(function (xhr, status, error) {
                console.error("Error adding rating:", xhr.responseText);
                toastr.error("Error adding rating");
            }).always(function () {
                self.isLoading = false;
            });
        },

        resetRatingForm: function () {
            this.ratingForm = {
                rating: 5,
                ratingCategory: 'Overall',
                comments: ''
            };
        },

        // Task methods
        toggleTaskForm: function () {
            this.showTaskForm = !this.showTaskForm;
            if (this.showTaskForm) {
                this.showActionForm = false;
                this.showRatingForm = false;
            }
        },

        saveTask: function () {
            var self = this;

            if (!this.taskForm.taskTitle) {
                toastr.warning("Please enter a task title");
                return;
            }

            if (!this.taskForm.assignedToUserId) {
                toastr.warning("Please select who to assign the task to");
                return;
            }

            self.isLoading = true;

            var data = {
                ServiceProviderUserID: this.selectedProfile.UserId,
                TaskTitle: this.taskForm.taskTitle,
                TaskDescription: this.taskForm.taskDescription,
                AssignedToUserID: this.taskForm.assignedToUserId,
                DueDate: this.taskForm.dueDate || null,
                Priority: this.taskForm.priority
            };

            self.post("AddTask", data).done(function (response) {
                toastr.success("Task assigned successfully");
                self.resetTaskForm();
                self.showTaskForm = false;
                // Reload profile data
                self.viewProfile(self.selectedProfile);
            }).fail(function (xhr, status, error) {
                console.error("Error adding task:", xhr.responseText);
                toastr.error("Error adding task");
            }).always(function () {
                self.isLoading = false;
            });
        },

        resetTaskForm: function () {
            this.taskForm = {
                taskTitle: '',
                taskDescription: '',
                assignedToUserId: null,
                dueDate: '',
                priority: 'Medium'
            };
        },

        updateTaskStatus: function (task) {
            var self = this;
            var newStatus = '';

            switch (task.Status) {
                case 'Pending':
                    newStatus = 'InProgress';
                    break;
                case 'InProgress':
                    newStatus = 'Completed';
                    break;
                case 'Completed':
                    newStatus = 'Pending';
                    break;
                default:
                    return;
            }

            self.isLoading = true;

            var data = {
                TaskID: task.TaskID,
                Status: newStatus
            };

            self.post("UpdateTaskStatus", data).done(function (response) {
                toastr.success("Task status updated");
                // Reload profile data
                self.viewProfile(self.selectedProfile);
            }).fail(function (xhr, status, error) {
                console.error("Error updating task:", xhr.responseText);
                toastr.error("Error updating task status");
            }).always(function () {
                self.isLoading = false;
            });
        },

        loadManagers: function () {
            var self = this;
            self.get("GetManagers").done(function (response) {
                self.managers = response;
            }).fail(function (xhr, status, error) {
                console.error("Error loading managers:", xhr.responseText);
            });
        },

        getTaskStatusClass: function (status) {
            switch (status) {
                case 'Pending': return 'task-pending';
                case 'InProgress': return 'task-inprogress';
                case 'Completed': return 'task-completed';
                case 'Cancelled': return 'task-cancelled';
                default: return '';
            }
        },

        getPriorityClass: function (priority) {
            switch (priority) {
                case 'Urgent': return 'priority-urgent';
                case 'High': return 'priority-high';
                case 'Medium': return 'priority-medium';
                case 'Low': return 'priority-low';
                default: return '';
            }
        },

        getActionIcon: function (actionType) {
            switch (actionType) {
                case 'Note': return '📝';
                case 'Call': return '📞';
                case 'Email': return '✉️';
                case 'Meeting': return '🤝';
                case 'Task': return '📋';
                case 'StatusChange': return '🔄';
                default: return '📌';
            }
        },

        // Replace your existing editUser method with this version that uses Vue.$set for proper reactivity:

        editUser: function (user) {
            this.actionMenuOpen = null;
            this.editingUser = user;

            // Reset the form first
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

            // Then populate with user data
            this.$set(this.editForm, 'firstName', user.FirstName || '');
            this.$set(this.editForm, 'lastName', user.LastName || '');
            this.$set(this.editForm, 'displayName', user.DisplayName || '');
            this.$set(this.editForm, 'email', user.Email || '');
            this.$set(this.editForm, 'phone', user.Phone || '');
            this.$set(this.editForm, 'mobile', user.Mobile || '');
            this.$set(this.editForm, 'street', user.Street || '');
            this.$set(this.editForm, 'city', user.City || '');
            this.$set(this.editForm, 'stateRegion', user.StateRegion || '');
            this.$set(this.editForm, 'postalCode', user.PostalCode || '');
            this.$set(this.editForm, 'country', user.Country || '');

            // Use Vue.$set for boolean values to ensure reactivity
            var isAuthorized = user.IsAuthorized === true || user.IsAuthorized === 1 || user.IsAuthorized === '1';
            var isDeleted = user.IsDeleted === true || user.IsDeleted === 1 || user.IsDeleted === '1';

            this.$set(this.editForm, 'isAuthorized', isAuthorized);
            this.$set(this.editForm, 'isDeleted', isDeleted);

            // Set role expiration date if exists
            if (user.RoleExpirationDate) {
                var date = new Date(user.RoleExpirationDate);
                var year = date.getFullYear();
                var month = ('0' + (date.getMonth() + 1)).slice(-2);
                var day = ('0' + date.getDate()).slice(-2);
                this.$set(this.editForm, 'roleExpirationDate', year + '-' + month + '-' + day);
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
                isAuthorized: true,    // Default should be true for new users
                isDeleted: false       // Default should be false for new users
            };
        },

        saveUserEdit: function () {
            var self = this;

            try {
                console.log("Save button clicked", this.editForm);

                // Store the previous values
                var wasDeleted = this.editingUser.IsDeleted === true || this.editingUser.IsDeleted === 1;
                var wasAuthorized = this.editingUser.IsAuthorized === true || this.editingUser.IsAuthorized === 1;

                // Convert form values to proper booleans
                var isNowDeleted = this.editForm.isDeleted === true;
                var isNowAuthorized = this.editForm.isAuthorized === true;

                // Only confirm if changing deletion status from false to true
                if (isNowDeleted && !wasDeleted) {
                    if (!confirm('Are you sure you want to soft delete this user? This action can be reversed by an administrator.')) {
                        return;
                    }
                }

                // Only confirm if changing authorization status from true to false
                if (!isNowAuthorized && wasAuthorized) {
                    if (!confirm('Are you sure you want to unauthorize this user? They will not be able to access the portal.')) {
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
                    IsAuthorized: isNowAuthorized,  // Use the properly converted boolean
                    IsDeleted: isNowDeleted         // Use the properly converted boolean
                };

                self.post("UpdateUser", data).done(function (response) {
                    var message = "User updated successfully";

                    // Check what actually changed
                    if (isNowDeleted && !wasDeleted) {
                        message = "User has been soft deleted";
                    } else if (!isNowDeleted && wasDeleted) {
                        message = "User has been restored";
                    } else if (!isNowAuthorized && wasAuthorized) {
                        message = "User has been unauthorized";
                    } else if (isNowAuthorized && !wasAuthorized) {
                        message = "User has been authorized";
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
            } catch (error) {
                console.error("Error in saveUserEdit:", error);
                toastr.error("An error occurred while saving: " + error.message);
                self.isLoading = false;
            }
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
                console.log("Payment Statistics Response:", response); // Debug log
                self.paymentStats = {
                    totalPayments: response.totalPayments || response.TotalPayments || 0,
                    totalRevenue: response.totalRevenue || response.TotalRevenue || 0,
                    totalDiscounts: response.totalDiscounts || response.TotalDiscounts || 0,
                    totalNetRevenue: response.totalNetRevenue || response.TotalNetRevenue || 0,
                    revenueThisMonth: response.monthlyRevenue || response.MonthlyRevenue || response.revenueThisMonth || 0,
                    revenueThisYear: response.yearlyRevenue || response.YearlyRevenue || response.revenueThisYear || 0
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
                console.log("Year Statistics Response:", response); // Debug log
                self.selectedYearRevenue = response.yearlyRevenue || response.YearlyRevenue || 0;
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