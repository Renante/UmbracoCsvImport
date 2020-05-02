function csvImportResource($http, umbRequestHelper) {

    return {
        getFields: function (contentTypeId) {
            return umbRequestHelper.resourcePromise(
                $http.get(`/umbraco/backoffice/api/csvimportapi/GetModel?contentTypeId=${contentTypeId}`)
                ,'Failed to get fields');
        },

        publish: function (data) {
            return umbRequestHelper.resourcePromise(
                $http.post(`/umbraco/backoffice/api/csvimportapi/publish`, data)
                , 'Import failed');
        },
    }
}

angular.module("umbraco.resources").factory('csvImportResource', csvImportResource);