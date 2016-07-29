var logRetrievalControllers = angular.module("logRetrievalControllers", ["services"]);

logRetrievalControllers.controller("RootCtrl", ["$scope", "ajaxFactory", "$filter", function ($scope, ajaxFactory, $filter) {
    $scope.showRow = function (fileInfo) {
        var emptyShow = $scope.showEmpty || fileInfo.size > 0;

        var nodeNumber = $scope.nodes.indexOf(fileInfo.nodeName);

        var nodeShow = nodeNumber < 0 || nodeNumber > $scope.showNodes.length || $scope.showNodes[nodeNumber];
        return emptyShow && nodeShow;
    }

    $scope.reverse = true;
    $scope.orderBy = "modified";

    $scope.order = function (orderBy) {
        $scope.reverse = ($scope.orderBy === orderBy) ? !$scope.reverse : true;
        $scope.orderBy = orderBy;
    }

    //Grab the log file names
    ajaxFactory.ajaxRequest("./api/logs/local")
        .success(function (result) {
            var editedResults = [];
            var nodes = [];
            var bytes = $filter("bytes");
            angular.forEach(result, function (value) {
                    value.modified = new Date(value.modified);
                    value.readableSize = bytes(value.size);
                    if (nodes.indexOf(value.nodeName) === -1) nodes.push(value.nodeName);
                    this.push(value);
            }, editedResults);
            $scope.results = editedResults;
            $scope.nodes = nodes;
            var size = nodes.length;
            var showNodes = [];
            while (size--) showNodes[size] = true;
            $scope.showNodes = showNodes;
        })
    .error(function (data, status, headers, config) {
    });
}]);

logRetrievalControllers.filter("bytes", function () {
    return function (bytes, precision) {
        if (isNaN(parseFloat(bytes)) || !isFinite(bytes)) {
            return "0 bytes";
        }

        if (typeof precision === "undefined") {
            precision = 1;
        }

        var units = ["bytes", "kB", "MB", "GB", "TB", "PB"],
        number = Math.floor(Math.log(bytes) / Math.log(1024));

        return (bytes / Math.pow(1024, Math.floor(number))).toFixed(precision) + " " + units[number];
    }
});