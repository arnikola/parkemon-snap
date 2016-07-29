var otterpopServices = angular.module('otterpopServices', []);

otterpopServices.factory("ajaxFactory", ["$http", function ($http) {
    var ajaxFactory = [];
    var baseUrl = window.location.origin ? window.location.origin : window.location.protocol + "//" + window.location.hostname + (window.location.port ? ':' + window.location.port : '');;

    ajaxFactory.useToken = true;
    ajaxFactory.ajaxRequest = function (url, method, data) {
        var fullUrl = baseUrl + "/api/" + url;
        method = method || "GET";
        var req = {
            method: method,
            url: fullUrl,
            headers: {
                "Authorization": "Basic " + ((ajaxFactory.useToken === true) ? localStorage["userId"] : localStorage["token"] + ((localStorage["password"]) ? ":" + localStorage["password"] : "")),
            },
            contentType: "application/json;charset=utf-8"
        }
        if (data) {
            req.data = data;
        }
        return $http(req);
    };

    ajaxFactory.imagePost = function (url, method, file) {
        var fd = new FormData();
        fd.append('file', file);
        var fullUrl = baseUrl + "/api/" + url;
        method = method || "GET";
        var req = {
            transformRequest: angular.identity,
            method: method,
            url: fullUrl,
            headers: {
                "Authorization": "Basic " + localStorage["token"]
            },
            headers: { 'Content-Type': undefined },
            data: fd
        }
        return $http(req);
    };

    return ajaxFactory;
}]);

otterpopServices.factory("offersStream", [function () {
    var offersStream = {};
    offersStream.offers = [];
    offersStream.mapOffers = [];

    // Get the user's location.
    navigator.geolocation.getCurrentPosition(function (result) {
        var updates = WebStream("/stream/location", { lat: result.coords.latitude, lon: result.coords.longitude });
        updates.subscribe(function (data) {
            var found = false;
            for (var i = 0; i < offersStream.offers.length; i++)
                if (data.offer.id === offersStream.offers[i].offer.id) {
                    offersStream.offers.splice(i, 1, data);
                    found = true;
                    break;
                }
            if (!found)
                offersStream.offers.push(data);
            if (offersStream.callback) //notify of update if necessary
                offersStream.callback(data);
        });
    }, function (error) {
        console.error("Error getting geolocation from user:");
        console.error(error);
    });

    offersStream.shiftMap = function(pos) {
        if (offersStream.subscription)
            offersStream.subscription.dispose();
        offersStream.mapUpdates = WebStream("/stream/location", pos);
        offersStream.subscription = offersStream.mapUpdates.subscribe(function (data) {
            var found = false;
            for (var i = 0; i < offersStream.mapOffers.length; i++)
                if (data.offer.id === offersStream.mapOffers[i].offer.id) {
                    offersStream.mapOffers.splice(i, 1, data);
                    found = true;
                    break;
                }
            if (!found)
                offersStream.mapOffers.push(data);
            if (offersStream.callback) //notify of update if necessary
                offersStream.callback(data);
        });
    }

    return offersStream;
}]);