﻿'use strict';
app.controller('PageController', ['$scope', '$rootScope', '$routeParams', '$timeout', '$location', 'authService', 'PageServices',
    function ($scope, $rootScope, $routeParams, $timeout, $location, authService, pageServices) {
        $scope.request = {
            pageSize: '10',
            pageIndex: 0,
            status: $rootScope.swStatus[1],
            orderBy: 'Priority',
            direction: '0',
            fromDate: null,
            toDate: null,
            keyword: ''
        };

        $scope.activedPage = null;
        $scope.relatedPages = [];
        $rootScope.isBusy = false;
        $scope.data = {
            pageIndex: 0,
            pageSize: 1,
            totalItems: 0
        };
        $scope.errors = [];
        
        $scope.range = function (max) {
            var input = [];
            for (var i = 1; i <= max; i += 1) input.push(i);
            return input;
        };

        $scope.getPage = async function (id) {
            $rootScope.isBusy = true;
            var resp = await pageServices.getPage(id, 'be');
            if (resp.isSucceed) {
                $scope.activedPage = resp.data;
                $rootScope.initEditor();
                $scope.$apply();
            }
            else {
                $rootScope.showErrors(resp.errors);
                $scope.$apply();
            }
        };

        $scope.initPage = function () {
            if (!$rootScope.isBusy) {
                $rootScope.isBusy = true;
                pageServices.initPage('be').then(function (response) {
                    if (response.isSucceed) {
                        $scope.activedPage = response.data;
                        $rootScope.initEditor();
                    }
                    $rootScope.isBusy = false;
                    $scope.$apply();
                }).error(function (a, b, c) {
                    errors.push(a, b, c);
                    $rootScope.isBusy = false;
                    //$("html, body").animate({ "scrollTop": "0px" }, 500);
                });
            }
        };

        $scope.loadPage = async function () {
            $rootScope.isBusy = true;
            var id = $routeParams.id;
            var response = await pageServices.getPage(id, 'be');
            if (response.isSucceed) {
                $scope.activedPage = response.data;
                $rootScope.initEditor();
                $scope.$apply();
            }
            else {
                $rootScope.showErrors(response.errors);
                $scope.$apply();
            }
        };
        $scope.loadPages = async function (pageIndex) {
            if (pageIndex != undefined) {
                $scope.request.pageIndex = pageIndex;
            }
            if ($scope.request.fromDate != null) {
                var d = new Date($scope.request.fromDate);
                $scope.request.fromDate = d.toISOString();
            }
            if ($scope.request.toDate != null) {
                $scope.request.toDate = $scope.request.toDate.toISOString();
            }
            var resp = await pageServices.getPages($scope.request);
            if (resp.isSucceed) {

                ($scope.data = resp.data);
                //$("html, body").animate({ "scrollTop": "0px" }, 500);
                $.each($scope.data.items, function (i, page) {

                    $.each($scope.activedPages, function (i, e) {
                        if (e.pageId == page.id) {
                            page.isHidden = true;
                        }
                    })
                })
                setTimeout(function () {
                    $('[data-toggle="popover"]').popover({
                        html: true,
                        content: function () {
                            var content = $(this).next('.popover-body');
                            return $(content).html();
                        },
                        title: function () {
                            var title = $(this).attr("data-popover-content");
                            return $(title).children(".popover-heading").html();
                        }
                    });
                }, 200);
                $scope.$apply();
            }
            else {
                $rootScope.showErrors(resp.errors);
                $scope.$apply();
            }
        };

        $scope.removePage = async function (id) {
            if (confirm("Are you sure!")) {
                var resp = await pageServices.removePage(id);
                if (resp.isSucceed) {
                    $scope.loadPages();
                }
                else {
                    $rootScope.showErrors(resp.errors);
                }
            }
        };

        $scope.savePage = async function (page) {
            page.content = $('.editor-content').val();
            var resp = await pageServices.savePage(page);
            if (resp.isSucceed) {
                $scope.activedPage = resp.data;
                $rootScope.showMessage('Thành công', 'success');
                $rootScope.isBusy = false;
                $scope.$apply();
                //$location.path('/backend/page/details/' + resp.data.id);
            }
            else {
                $rootScope.showErrors(resp.errors);
                $scope.$apply();
            }
        };

    }]);
