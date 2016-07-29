var services = angular.module('services', []);

services.factory("ajaxFactory", ["$http", function ($http) {
    var ajaxFactory = [];

    var getFullUrl = function (url) {
        var base = '';
        if (url.startsWith('./')) {
            base = window.location;
            url = url.substr(2);
        } else if (url[0] === '/') {
            base = window.location.origin;
            url = url.substr(1);
        }

        if (base.length > 0 && base[base.length - 1] !== '/' ) {
            base += '/';
        }

        return base + url;
    }

    ajaxFactory.ajaxRequest = function (url, method, data) {
        //Check if we have a defined token
        var fullUrl = getFullUrl(url);
        method = method || "GET";
        var req = {
            method: method,
            url: fullUrl,
            contentType: "application/json;charset=utf-8"
        }
        if (data) {
            req.data = data;
        }
        return $http(req);
    };

    ajaxFactory.imagePost = function (url, method, file) {
        //Check if we have a defined token
        var fd = new FormData();
        fd.append("file", file);
        var fullUrl = getFullUrl(url);
        method = method || "GET";
        var req = {
            transformRequest: angular.identity,
            method: method,
            url: fullUrl,
            headers: {
                'Content-Type': undefined
            },
            data: fd
        }
        return $http(req);
    };

   return ajaxFactory;
}]);