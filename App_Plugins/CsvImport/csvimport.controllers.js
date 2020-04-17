function CsvImportController(
    Upload,
    contentTypeResource,
    contentResource,
    editorService,
    $timeout) {

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
        Papa.parse(file, {
            header: true,
            complete: function (results) {
                vm.csvHeaders = results.meta.fields;
                vm.csvData = results.data;
                vm.window.next();
            }
        });
    };

    vm.selectParentNode = function () {
        var contentPickerConfig = {
            submit: function(model) {
                vm.parentNodeId = model.selection[0].id;
                contentTypeResource.getAllowedTypes(vm.parentNodeId)
                    .then(function (result) {
                        vm.contentTypes = result.map(item => ({ id: item.id, name: item.name, alias: item.alias }));
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
            vm.contentTypeProps = {
                name: '',
                groups: [] 
            };
            angular.forEach(result.groups, function (group) {
                var newGroup = {
                    name: group.name,
                    id: group.id,
                    properties: []
                }
                angular.forEach(group.properties, function (prop) {
                    newGroup.properties.push(
                        {
                            label: prop.label,
                            alias: prop.alias,
                            editor: prop.editor,
                            isMandatory: prop.validation.mandatory
                        }
                    );
                });
                vm.contentTypeProps.groups.push(newGroup);
            });
            vm.window.next();
        });
    }

    vm.processing = false;
    vm.submit = function () {
        if (vm.importForm.$valid) {
            vm.processing = true;
            vm.currentItem = 0;
            angular.forEach(vm.csvData.slice(0,5), function (row) {
                contentResource.getScaffold(vm.parentNodeId, vm.selectedContentType.alias)
                    .then(function (scaffold) {
                        var myDoc = scaffold;
                        
                        myDoc.variants[0].name = row[vm.contentTypeProps.name].substring(0, 250);
                        angular.forEach(vm.contentTypeProps.groups, function (group) {
                            angular.forEach(group.properties, function (prop) {
                                var fieldValue = row[prop.csvHeader];
                                switch (prop.editor) {
                                    case 'Umbraco.TextBox':
                                        fieldValue = fieldValue.substring(0, 250);
                                        break;

                                    default:
                                }

                                myDoc.variants[0].tabs.find(t => t.id === group.id).properties.find(p => p.alias === prop.alias).value = fieldValue;
                            });
                        });

                        myDoc.variants[0].save = true;

                        contentResource.publish(myDoc, true, [''])
                            .then(function (content) {
                                vm.currentItem++;
                            });
                        
                    });
            });
            vm.processing = false;
            vm.window.next();
        }
        else {
            console.log('Invalid form');
        }
    }
};

app.requires.push('ngFileUpload'); 
angular.module("umbraco").controller("CsvImportController", CsvImportController);