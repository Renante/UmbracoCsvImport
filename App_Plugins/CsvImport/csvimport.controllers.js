function CsvImporterController($scope, Upload, csvImporterResource) {
    var vm = this;
    vm.upload = function (file) {
        csvImporterResource.upload(file).then(function (result) {
            console.log(result);
        });
    };
};

app.requires.push('ngFileUpload'); 
angular.module("umbraco").controller("CsvImporterController", CsvImporterController);