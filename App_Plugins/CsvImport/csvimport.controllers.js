function CsvImportController(
    $scope,
    Upload,
    csvImportResource,
    editorService) {

    var vm = this;
    
    vm.window = {
        _currentIndex: 0,
        steps: [true, false],
        moveTo: function(index) {
            angular.forEach(this.steps, (val, idx) => this.steps[idx] = false);
            this.steps[index] = true;
            this._currentIndex = index;
        },
        nextStep: function() {
            this.moveTo(++this._currentIndex);
        }
    }

    vm.upload = function (file) {
        csvImportResource.upload(file).then(function (result) {
            vm.window.nextStep();
        });
    };

    vm.selectParentNode = function () {
        var contentPickerConfig = {
            submit: function(model) {
                console.log(model.selection[0].id);
                editorService.close();
            },
            close: function() {
                editorService.close();
            }
        }
        editorService.contentPicker(contentPickerConfig);
    }
};

app.requires.push('ngFileUpload'); 
angular.module("umbraco").controller("CsvImportController", CsvImportController);