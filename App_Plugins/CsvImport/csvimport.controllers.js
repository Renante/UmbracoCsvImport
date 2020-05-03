function CsvImportController(
    contentTypeResource,
    editorService,
    csvImportResource) {

    var vm = this;

    vm.window = {
        _currentIndex: 0,
        steps: [true, false],
        moveTo: function (index) {
            angular.forEach(this.steps, (val, idx) => this.steps[idx] = false);
            this.steps[index] = true;
            this._currentIndex = index;
        },
        next: function () {
            this.moveTo(++this._currentIndex);
        },
        prev: function () {
            this.moveTo(--this._currentIndex);
        }

    }

    vm.isCsvReady = false;
    vm.upload = function () {
        var file = document.getElementById('csvImportFile').files[0];
        Papa.parse(file, {
            header: true,
            skipEmptyLines: true,
            complete: function (results) {
                vm.csvHeaders = results.meta.fields;
                vm.csvData = results.data;
                vm.totalCsvRows = vm.csvData.length;
                vm.isCsvReady = true;
            }
        });

        vm.window.next();
    };

    vm.selectParentNode = function () {
        var contentPickerConfig = {
            submit: function (model) {
                vm.parentNodeId = model.selection[0].id;
                contentTypeResource.getAllowedTypes(vm.parentNodeId)
                    .then(function (result) {
                        vm.contentTypes = result.map(item => ({ id: item.id, name: item.name, alias: item.alias }));
                        editorService.close();
                        vm.window.moveTo(2);
                    });
            },
            close: function () {
                editorService.close();
            }
        }
        editorService.contentPicker(contentPickerConfig);
    }

    vm.processingContentType = false;
    vm.processContentType = function () {
        contentTypeResource.getById(vm.selectedContentType.id).then(function (result) {
            csvImportResource.getFields(vm.selectedContentType.id)
                .then(function (result) {
                    vm.page = result;
                    vm.page.Variants[0].active = true;
                    vm.window.next();
                });
        });
    }


    vm.processing = false;
    vm.totalItemsImported = 0;
    vm.totalItemsFailed = 0;
    vm.completed = false;
    vm.submit = function () {
        vm.currentItem = 0;
        if (vm.importForm.$valid) {
            vm.processing = true;
            vm.window.next();
            angular.forEach(vm.csvData, function (row) {
                importRow(row);
            });
        }
    }

    vm.changeTab = function (variant) {
        angular.forEach(vm.page.Variants, function (v) {
            v.active = false;
        });
        variant.active = true;
    }

    vm.logs = [];

    function importRow(row) {

        var page = angular.copy(vm.page);

        angular.forEach(page.Variants, function (variant) {
            variant.Language.Value = row[variant.Language.CsvHeader];
            angular.forEach(variant.PropertyTypes, function (propType) {
                propType.Value = row[propType.CsvHeader];
            });

            // remove empty fields before sending to server
            variant.PropertyTypes = variant.PropertyTypes.filter(p => p.Value);
        });

        var data = {
            ContentTypeAlias: vm.selectedContentType.alias,
            ParentId: vm.parentNodeId,
            Page: page
        }

        csvImportResource.publish(data)
            .then(function () {
                vm.totalItemsImported++;
                checkIfDone();

            }, function (error) {
                vm.logs.push({ Success: false, Message: error.data.Message });
                vm.totalItemsFailed++;
                checkIfDone();
            }, );
    }

    function checkIfDone() {
        if (vm.csvData.length === (vm.totalItemsImported + vm.totalItemsFailed)) {
            vm.completed = true;
        }
    }
};

angular.module("umbraco").controller("CsvImportController", CsvImportController);