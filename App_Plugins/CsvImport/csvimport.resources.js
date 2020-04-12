function csvImportResource($http, umbRequestHelper, Upload) {
    return {
        upload: function (file) {
            return umbRequestHelper.resourcePromise(Upload.upload({
                url: '/umbraco/backoffice/csv/csvapi/getHeaders',
                file: file
            }), 'Error uploading file');
        },
    }
}

angular.module("umbraco.resources").service("csvImportResource", csvImportResource);
