var otterpopApp = angular.module("otterpopApp", [
  "ngRoute",
  "ui.bootstrap",
  "otterpopDirectives",
  "otterpopControllers",
  "ngImgCrop"]);

otterpopApp.config(["$routeProvider",
  function ($routeProvider) {
      $routeProvider.
        when("/", {
            templateUrl: "fragments/offer.html",
            controller: "OfferCtrl"
        }).
        when("/deal/:dealId", {
            templateUrl: "fragments/deal.html",
            controller: "ViewDealCtrl"
        }).
        when("/map", {
            templateUrl: "fragments/map.html",
            controller: "MapCtrl"
        }).
        when("/listing", {
            templateUrl: "fragments/offer.html",
            controller: "ListCtrl"
        }).
        when("/coupons", {
            templateUrl: "fragments/coupons.html",
            controller: "CouponsCtrl"
        }).
        when("/coupons/:couponId", {
            templateUrl: "fragments/deal.html",
            controller: "ViewCouponCtrl"
        }).
        otherwise({
            redirectTo: "/"
        });
  }]);

otterpopApp.filter("newline", ["$sce", function ($sce) {
    return function (text) {
        if (text)
            return $sce.trustAsHtml(text.replace(/\n/g, "<br/>"));
        else
            return "";
    }
}]);

otterpopApp.filter("encodeURL", function () {
    return window.encodeURIComponent;
});