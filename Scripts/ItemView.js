Vue.use(VueLoading);
Vue.component("loading", VueLoading);

var FinHubAddOns = FinHubAddOns || {};

FinHubAddOns.FinHubAddOnsComponent = {
    props: {
        moduleId: {
            required: true,
            type: Number
        },
        resx: {
            required: true,
            type: Object
        },
        userId: {
            required: false,
            type: Number
        }
    },

    template: "#fin-hub-add-ons-template",

    data: function () {
        var service = {
            path: "FinHubAddOns",
            framework: $.ServicesFramework(this.moduleId)
        };
        service.baseUrl = service.framework.getServiceRoot(service.path) + "Item/";

        return {
            service: service,
            isLoading: false
        };
    },

    created: function () {
        this.loadData();
    },

    methods: {
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

        loadData: function() {
            //TODO implement
            /* var self = this;

            self.isLoading = true;
            
            self.get("url").done(function (response) {
                if (!response.success) {
                    toastr.error(response.message);
                    return;
                }

                // do something
            }).fail(function () {                
                toastr.error("Error: can't load data");
            }).always(function () {
                self.isLoading = false;
            });*/
        }
    }
};