function CsvImportController(
    Upload,
    csvImportResource,
    contentTypeResource,
    editorService,
    notificationsService) {

    var vm = this;
    
    vm.window = {
        _currentIndex: 0,
        steps: [true, false],
        moveTo: function(index) {
            angular.forEach(this.steps, (val, idx) => this.steps[idx] = false);
            this.steps[index] = true;
            this._currentIndex = index;
        },
        next: function() {
            this.moveTo(++this._currentIndex);
        }
    }

    vm.upload = function (file) {
        csvImportResource.upload(file).then(function (result) {
            vm.headers = result;
            vm.window.next();
        });
    };

    vm.selectParentNode = function () {
        var contentPickerConfig = {
            submit: function(model) {
                vm.parentNodeId = model.selection[0].id;
                contentTypeResource.getAllowedTypes(vm.parentNodeId)
                    .then(function (result) {
                        vm.contentTypes = result.map(item => ({ id: item.id, name: item.name }));
                        editorService.close();
                        vm.window.next();
                    });
            },
            close: function() {
                editorService.close();
            }
        }
        editorService.contentPicker(contentPickerConfig);
    }

    vm.processContentType = function () {
        contentTypeResource.getById(vm.selectedContentType.id).then(function (result) {
            vm.contentTypeProps = [];
            vm.contentTypeProps.push({ label: 'Name', alias: '__name', editor: '' });
            angular.forEach(result.groups, function (group) {
                angular.forEach(group.properties, function (prop) {
                    vm.contentTypeProps.push({ label: prop.label, alias: prop.alias, editor: prop.editor });
                });
            });
            vm.window.next();
        });
    }

    vm.processing = false;
    vm.submit = function () {
        var data = {
            ParentId: vm.parentNodeId,
            ContentTypeId: vm.selectedContentType.id,
            Fields: []
        }

        angular.forEach(vm.contentTypeProps, function (prop) {
            data.Fields.push({ PropertyTypeAlias: prop.alias, Header: prop.csvHeader });
        });

        vm.processing = true;
        csvImportResource.submit(data).then(function () {
            vm.processing = false;
            notificationsService.success('Done importing data');
        });
    }
};


app.requires.push('ngFileUpload'); 
angular.module("umbraco").controller("CsvImportController", CsvImportController);