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
                daysInRole: '',
                expirationStatus: ''
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
                roleExpirationDate: ''
            },

            // Confirmation modal
            showConfirmModal: false,
            userToRemove: null,

            // Action menu
            actionMenuOpen: null
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

        activeUsers: function () {
            var today = new Date();
            return this.users.filter(function (user) {
                if (!user.RoleExpirationDate) return true;
                return new Date(user.RoleExpirationDate) > today;
            }).length;
        },

        expiredUsers: function () {
            var today = new Date();
            return this.users.filter(function (user) {
                if (!user.RoleExpirationDate) return false;
                return new Date(user.RoleExpirationDate) <= today;
            }).length;
        }
    },

    created: function () {
        this.loadData();
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
        },

        extractUniqueValues: function () {
            var cities = new Set();
            var states = new Set();
            var countries = new Set();

            this.users.forEach(function (user) {
                // Normalize city names to handle case differences
                if (user.City) {
                    var normalizedCity = user.City.trim();
                    // Store both original and normalized
                    cities.add(normalizedCity);
                    user.NormalizedCity = normalizedCity;
                }
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

            // Days in role filter
            if (this.filters.daysInRole) {
                filtered = filtered.filter(function (user) {
                    if (self.filters.daysInRole === '365+') {
                        return user.DaysInRole > 365;
                    } else {
                        var days = parseInt(self.filters.daysInRole);
                        return user.DaysInRole < days;
                    }
                });
            }

            // Expiration status filter
            if (this.filters.expirationStatus) {
                var today = new Date();
                var thirtyDaysFromNow = new Date();
                thirtyDaysFromNow.setDate(today.getDate() + 30);

                filtered = filtered.filter(function (user) {
                    if (!user.RoleExpirationDate) {
                        return self.filters.expirationStatus === 'never';
                    }

                    var expDate = new Date(user.RoleExpirationDate);

                    switch (self.filters.expirationStatus) {
                        case 'active':
                            return expDate > today;
                        case 'expiring30':
                            return expDate > today && expDate <= thirtyDaysFromNow;
                        case 'expired':
                            return expDate <= today;
                        case 'never':
                            return false;
                    }
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
                daysInRole: '',
                expirationStatus: ''
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

        getRowClass: function (user) {
            if (!user.RoleExpirationDate) return '';

            var today = new Date();
            var expDate = new Date(user.RoleExpirationDate);
            var daysUntilExpiration = Math.floor((expDate - today) / (1000 * 60 * 60 * 24));

            if (expDate <= today) return 'expired-row';
            if (daysUntilExpiration <= 30) return 'expiring-soon-row';
            return '';
        },

        getExpirationClass: function (user) {
            if (!user.RoleExpirationDate) return '';

            var today = new Date();
            var expDate = new Date(user.RoleExpirationDate);
            var daysUntilExpiration = Math.floor((expDate - today) / (1000 * 60 * 60 * 24));

            if (expDate <= today) return 'expired';
            if (daysUntilExpiration <= 30) return 'expiring-soon';
            return '';
        },

        // Statistics click handlers
        filterByExpiring: function () {
            this.clearFilters();
            this.filters.expirationStatus = 'expiring30';
            this.applyFilters();
        },

        filterByActive: function () {
            this.clearFilters();
            this.filters.expirationStatus = 'active';
            this.applyFilters();
        },

        filterByExpired: function () {
            this.clearFilters();
            this.filters.expirationStatus = 'expired';
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
                roleExpirationDate: ''
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
                roleExpirationDate: ''
            };
        },

        saveUserEdit: function () {
            var self = this;

            // Currently, only role expiration date can be updated
            // To update other fields, additional API endpoints would need to be created
            if (this.editForm.roleExpirationDate !== '') {
                self.isLoading = true;

                var data = {
                    UserId: this.editingUser.UserId,
                    ExpirationDate: this.editForm.roleExpirationDate || null
                };

                self.post("UpdateRoleExpiration", data).done(function (response) {
                    toastr.success("Role expiration date updated successfully");
                    self.cancelEdit();
                    self.loadData();
                }).fail(function (xhr, status, error) {
                    console.error("Update Error:", xhr.responseText);
                    toastr.error("Error updating expiration date");
                }).always(function () {
                    self.isLoading = false;
                });
            } else {
                toastr.info("Please set an expiration date to save changes");
            }
        },

        insertPayment: function (user) {
            this.actionMenuOpen = null;
            // Add your payment insertion logic here
            // For example, open a payment modal or redirect to payment page
            toastr.info("Insert Payment functionality for " + user.DisplayName + " - Coming soon!");
            // You can implement this based on your payment system
            // window.location.href = '/payment/insert?userId=' + user.UserId;
        },

        viewPaymentHistory: function (user) {
            this.actionMenuOpen = null;
            // Add your payment history logic here
            // For example, open a modal or redirect to history page
            toastr.info("Payment History for " + user.DisplayName + " - Coming soon!");
            // You can implement this based on your payment system
            // window.location.href = '/payment/history?userId=' + user.UserId;
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