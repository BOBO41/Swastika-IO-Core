﻿'use strict';
var app = angular.module('SwastikaPortal', ['ngRoute', 'components', 'ngFileUpload', 'LocalStorageModule', 'bw.paging']);
var serviceBase = "/";

app.directive('ngEnter', function () {
    return function (scope, element, attrs) {
        element.bind("keydown keypress", function (event) {
            if (event.which === 13) {
                scope.$apply(function () {
                    scope.$eval(attrs.ngEnter);
                });

                event.preventDefault();
            }
        });
    };
}).directive('file', function () {
    return {
        scope: {
            file: '='
        },
        link: function (scope, el, attrs) {
            el.bind('change', function (event) {
                var files = event.target.files;
                var file = files[0];
                scope.file = file;
                scope.$apply();
            });
        }
    };
}).directive('imageonload', function () {
    return {
        restrict: 'A',
        link: function (scope, element, attrs) {
            element.bind('load', function () {
            });
            element.bind('error', function () {
            });
        }
    };
}).filter('utcToLocal', Filter)
    .constant('ngAuthSettings', {
        apiServiceBaseUri: '/',
        clientId: 'ngAuthApp',
        facebookAppId: '464285300363325'
    });

app.run(['$rootScope', '$location', 'commonServices', 'authService', function ($rootScope, $location, commonServices, authService) {
    authService.fillAuthData();
    commonServices.fillSettings().then(function (response) {
        $rootScope.settings = response;
    });

    $rootScope.currentContext = $rootScope;
    $rootScope.errors = [];
    $rootScope.message = {
        title: '',
        content: '',
        errors: [],
        okFuncName: null,
        okArgs: [],
        cancelFuncName: null,
        cancelArgs: [],
        lblOK: 'OK',
        lblCancel: 'Cancel',
        context: $rootScope
    };

    $rootScope.authentication = authService.authentication;

    $rootScope.swStatus = [
        'Deleted',
        'Preview',
        'Published',
        'Draft',
        'Schedule'
    ]
    $rootScope.orders = [
        {
            value: 'CreatedDateTime',
            title: 'Created Date'
        }
        ,
        {
            value: 'Priority',
            title: 'Priority'
        },

        {
            value: 'Title',
            title: 'Title'
        }
    ];
    $rootScope.directions = [
        {
            value: '0',
            title: 'Asc'
        },
        {
            value: '1',
            title: 'Desc'
        }
    ]
    $rootScope.pageSizes = [
        '5',
        '10',
        '15',
        '20'
    ]
    $rootScope.request = {
        pageSize: '10',
        pageIndex: 0,
        status: $rootScope.swStatus[1],
        orderBy: 'CreatedDateTime',
        direction: '1',
        fromDate: null,
        toDate: null,
        keyword: ''
    };
    $rootScope.configurations = {
        core: {},
        plugins: {
            btnsDef: {
                // Customizables dropdowns
                image: {
                    dropdown: ['insertImage', 'upload', 'base64', 'noembed'],
                    ico: 'insertImage'
                }
            },
            btns: [
                ['viewHTML'],
                ['undo', 'redo'],
                ['formatting'],
                ['strong', 'em', 'del', 'underline'],
                ['link'],
                ['image'],
                ['justifyLeft', 'justifyCenter', 'justifyRight', 'justifyFull'],
                ['unorderedList', 'orderedList'],
                ['foreColor', 'backColor'],
                ['preformatted'],
                ['horizontalRule'],
                ['fullscreen']
            ],
            plugins: {
                // Add imagur parameters to upload plugin
                upload: {
                    serverPath: 'https://api.imgur.com/3/image',
                    fileFieldName: 'image',
                    headers: {
                        'Authorization': 'Client-ID 9e57cb1c4791cea'
                    },
                    urlPropertyName: 'data.link'
                }
            }
        }
    };
    $rootScope.range = function (max) {
        var input = [];
        for (var i = 1; i <= max; i += 1) input.push(i);
        return input;
    };

    $rootScope.$watch('isBusy', function (newValue, oldValue) {
        if (newValue) {
            $rootScope.message.content = '';
            $rootScope.errors = [];
        }
    });

    $rootScope.showErrors = function (errors) {
        $rootScope.message.title = 'Errors';
        $rootScope.message.errors = errors;
        $rootScope.message.content = '';
        $rootScope.message.class = 'danger';
        $('#dlg-msg').modal("show");
    }

    //type: success / info / danger / warning - bootstrap 
    $rootScope.showMessage = function (content, type) {
        type = type || 'info';
        $rootScope.message.title = 'Result';
        $rootScope.message.content = content;
        $rootScope.message.errors = [];
        $rootScope.message.class = type;
        $('#dlg-msg').modal("show");
    }

    $rootScope.logOut = function () {
        authService.logOut();
        $location.path('/backend/login');
    };
    if (!authService.authentication.isAuth || !authService.authentication.isAdmin) {
        authService.authentication.referredUrl = $location.path();
        $location.path('/backend/login');
    }

    $rootScope.executeFunctionByName = function (functionName, args, context) {
        if (functionName !== null) {
            var namespaces = functionName.split(".");
            var func = namespaces.pop();
            for (var i = 0; i < namespaces.length; i++) {
                context = context[namespaces[i]];
            }
            functionName = null;
            return context[func].apply(this, args);
        }
    };

    $rootScope.showConfirm = function (context, okFuncName, okArgs, cancelFuncName, title, msg, cancelArgs, lblOK, lblCancel) {
        $rootScope.message = {
            title: title,
            value: msg,
            context: $scope,
            okFuncName: okFuncName,
            okArgs: okArgs,
            cancelFuncName: cancelArgs,
            cancelArgs: cancelArgs,
            lblOK: lblOK,
            lblCancel: lblCancel
        };
        $('#modal-confirm').modal('show');
    };
    $rootScope.initEditor = function () {
        setTimeout(function () {
            // Init Code editor
            $.each($('.code-editor'), function (i, e) {
                var container = $(this);
                var editor = ace.edit(e);
                if (container.hasClass('json')) {
                    editor.session.setMode("ace/mode/json");
                }
                else {
                    editor.session.setMode("ace/mode/razor");
                }
                editor.setTheme("ace/theme/chrome");
                //editor.setReadOnly(true);

                editor.session.setUseWrapMode(true);
                editor.setOptions({
                    maxLines: Infinity
                });
                editor.getSession().on('change', function (e) {
                    // e.type, etc
                    $(container).parent().find('.code-content').val(editor.getValue());
                });
            })
            $.each($('.editor-content'), function (i, e) {
                var $demoTextarea = $(e);
                $demoTextarea.trumbowyg($rootScope.configurations.plugins);
            });
        }, 200)
    }
}]);


function Filter($filter) {
    return function (utcDateString, format) {
        // return if input date is null or undefined
        if (!utcDateString) {
            return;
        }

        // append 'Z' to the date string to indicate UTC time if the timezone isn't already specified
        if (utcDateString.indexOf('Z') === -1 && utcDateString.indexOf('+') === -1) {
            utcDateString += 'Z';
        }

        // convert and format date using the built in angularjs date filter
        return $filter('date')(utcDateString, format);
    };
}