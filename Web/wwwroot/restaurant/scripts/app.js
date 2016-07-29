var otterpopApp = angular.module("otterpopApp", [
  "ngRoute",
  "ui.bootstrap",
  "otterpopDirectives",
  "otterpopControllers",
  "ngImgCrop",
  "rzModule",
  "ui.sortable"]);

otterpopApp.config(["$routeProvider",
  function ($routeProvider) {
      $routeProvider.
        when("/", {
            templateUrl: "fragments/customers.html",
            controller: "CustomersCtrl"
        }).
        when("/pending", {
            templateUrl: "fragments/customers.html",
            controller: "CustomersCtrl"
        }).
        when("/archive", {
            templateUrl: "fragments/customers.html",
            controller: "CustomersCtrl"
        }).
        when("/offers", {
            templateUrl: "fragments/offers.html",
            controller: "OffersCtrl"
        }).
        when("/createoffer", {
            templateUrl: "fragments/createofferphone.html",
            controller: "OfferDetailsCtrl"
        }).
        when("/createoffer/:offerId", {
            templateUrl: "fragments/createofferphone.html",
            controller: "OfferDetailsCtrl"
        }).
        when("/restaurant", {
            templateUrl: "fragments/settings.html",
            controller: "SettingsCtrl"
        }).
        when("/images", {
            templateUrl: "fragments/restaurantimages.html",
            controller: "SettingsCtrl"
        }).
        when("/login", {
            templateUrl: "fragments/login.html",
            controller: "LoginCtrl"
        }).
        otherwise({
            redirectTo: "/login"
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