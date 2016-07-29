var otterpopControllers = angular.module("otterpopControllers", ["otterpopServices"]);

otterpopControllers.controller("RootCtrl", ["$scope", "ajaxFactory", function ($scope, ajaxFactory) {
    //Stuff to be global, if anything
    $scope.global = {
        stopbodyoverflow: false
    };

    ajaxFactory.useToken = false;
    if (!localStorage["userId"])
        localStorage["userId"] = 'xxxxxxxxxxxx4xxxyxxxxxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
            var r = Math.random() * 16 | 0, v = c == 'x' ? r : (r & 0x3 | 0x8);
            return v.toString(16);
        });
    $scope.global.userId = localStorage["userId"];

    $scope.back = function() {
        window.history.back();
    }

    //Test offer populator
    //ajaxFactory.ajaxRequest("test/search?lat=-37.8136&lon=144.9631&keyword=chinese");

    $scope.global.userCoupons = [];
    $scope.makeStarRating = function(stars) {
        var val = '';
        for (var i = 0; i < Math.floor(stars); i++)
            val += '★';
        if (Math.floor(stars) < stars)
            val += '✫';
        for (var i = Math.floor(stars + 0.5); i < 5; i++)
            val += '☆';
        return val;
    }
    $scope.convertTimeStampToUTC = function (timeStamp) {
        return (new Date(timeStamp)).getTime();
    }

    //From http://stackoverflow.com/questions/27928/calculate-distance-between-two-latitude-longitude-points-haversine-formula
    $scope.latLonDist = function (lat1, lon1, lat2, lon2) {
        var p = 0.017453292519943295;    // Math.PI / 180
        var c = Math.cos;
        var a = 0.5 - c((lat2 - lat1) * p)/2 + 
                c(lat1 * p) * c(lat2 * p) * 
                (1 - c((lon2 - lon1) * p))/2;

        return 12742 * Math.asin(Math.sqrt(a)); // 2 * R; R = 6371 km
    }
}]);

otterpopControllers.controller("OfferCtrl", ["$scope", "offersStream", "$interval", function ($scope, offersStream, $interval) {
    $scope.global.stopbodyoverflow = $scope.global.mobileDevice;
    $scope.global.currentPage = "offer";

    //Keep track of offers
    $scope.offers = offersStream.offers;
    $scope.global.interval = $interval(function () {
    }, 1000);
}]);

otterpopControllers.controller("ListCtrl", ["$scope", "offersStream", "$interval", function ($scope, offersStream, $interval) {
    $scope.global.stopbodyoverflow = false;
    $scope.global.currentPage = "list";

    //Keep track of offers
    $scope.offers = offersStream.mapOffers;
    $scope.global.interval = $interval(function () {
    }, 1000);
}]);

otterpopControllers.controller("MapCtrl", ["$scope", "ajaxFactory", "$routeParams", "offersStream", "$interval", function ($scope, ajaxFactory, $routeParams, offersStream, $interval) {
    $scope.global.stopbodyoverflow = true;
    $scope.draggedABit = false;
    $scope.global.currentPage = "map";

    //Keep track of offers - for map, actually need to be passing up the region we can see (so changing the stream a lot I suppose)
    $scope.offers = [];
    $scope.offerStreamOffers = offersStream.offers;
    $scope.offerStreamMapOffers = offersStream.mapOffers;
    $scope.global.interval = $interval(function () {
    }, 1000);
    $scope.markers = [];
    $scope.infoWindows = [];
    $scope.contentStart = '<div id="content" style="padding: 20px !important; width: 140px">';
    $scope.contentEnd = '</div>';

    //Maptastic
    var mapCanvas = document.getElementById('map');
    $scope.mapOptions = {
        center: new google.maps.LatLng(-37.8136, 144.9631),
        zoom: 14,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    }
    $scope.googleMap = new google.maps.Map(mapCanvas, $scope.mapOptions);
    // Get the user's location.
    navigator.geolocation.getCurrentPosition(function (result) {
        $scope.googleMap.setCenter(new google.maps.LatLng(result.coords.latitude, result.coords.longitude));
        $scope.unlockCallback = true;
        $scope.callback();
    }, function (error) {
        console.error("Error getting geolocation from user:");
        console.error(error);
    });

    $scope.checkFunction = function () {
        offersStream.shiftMap({ lat: $scope.googleMap.getCenter().lat(), lon: $scope.googleMap.getCenter().lng() });
    };
    $scope.debouncedCheck = $.debounce(1000, $scope.checkFunction);
    $scope.googleMap.addListener('center_changed', function () {
        var closeDist = 0;
        if ($scope.offers.length > 0)
            closeDist = $scope.latLonDist($scope.offers[$scope.closest].restaurant.location.lat, $scope.offers[$scope.closest].restaurant.location.lng, $scope.googleMap.getCenter().lat(), $scope.googleMap.getCenter().lng());
        for (var i = 0; i < $scope.offers.length; i++)
            if ($scope.latLonDist($scope.offers[i].restaurant.location.lat, $scope.offers[i].restaurant.location.lng, $scope.googleMap.getCenter().lat(), $scope.googleMap.getCenter().lng()) < closeDist) {
                closeDist = $scope.latLonDist($scope.offers[i].restaurant.location.lat, $scope.offers[i].restaurant.location.lng, $scope.googleMap.getCenter().lat(), $scope.googleMap.getCenter().lng());;
                $scope.closest = i;
            }
        $scope.$apply();

        $scope.debouncedCheck();
    });

    $scope.closest = 0;

    $scope.callback = 
    function () {
        //Merge offersStream.offers into $scope.offers
        for (var j = 0; j < $scope.offerStreamOffers.length; j++) {
            var found = false;
            for (var i = 0; i < $scope.offers.length; i++)
                if ($scope.offerStreamOffers[j].id === $scope.offers[i].offer.id) {
                    $scope.offers.splice(i, 1, $scope.offerStreamOffers[j]);
                    found = true;
                    break;
                }
            if (!found)
                $scope.offers.push($scope.offerStreamOffers[j]);
        }
        for (var j = 0; j < $scope.offerStreamMapOffers.length; j++) {
            var found = false;
            for (var i = 0; i < $scope.offers.length; i++)
                if ($scope.offerStreamMapOffers[j].id === $scope.offers[i].offer.id) {
                    $scope.offers.splice(i, 1, $scope.offerStreamMapOffers[j]);
                    found = true;
                    break;
                }
            if (!found)
                $scope.offers.push($scope.offerStreamMapOffers[j]);
        }
        //Do map stuff
        if ($scope.unlockCallback) {
            //Need to be clever and only remove markers that no longer have an offer associated
            var newMarkers = [];
            for (var i = 0; i < $scope.offers.length; i++) {
                var id = $scope.offers[i].restaurant.id;
                var offer = $scope.offers[i];
                if ($scope.markers[id]) {
                    newMarkers[id] = $scope.markers[id];
                    $scope.markers[id] = null;
                } else if (!newMarkers[id]) {
                    newMarkers[id] = new google.maps.Marker({
                        position: new google.maps.LatLng(offer.restaurant.location.lat, offer.restaurant.location.lng),
                        map: $scope.googleMap,
                        title: 'Hello World!'
                    });
                    $scope.infoWindows[id] = new InfoBox({
                        maxWidth: 0,
                        pixelOffset: new google.maps.Size(-70, -44),
                        content: $scope.contentStart + '<a href="' + "http://" + window.location.host + "/#/deal/" + offer.id + '">'
                            + offer.restaurant.name + '<br>' + offer.offer.title + "</a>" + $scope.contentEnd,
                        disableAutoPan: true,
                        alignBottom: true,
                        boxStyle: {
                            'border-radius': "8px",
                            opacity: 1.0,
                            background: 'white',
                            border: '1px solid rgb(200, 200, 200)',
                           'box-shadow': '0 0 2px rgba(0, 0, 0, 0.2), 1px 1px 1px rgba(0, 0, 0, 0.2)'
                        }
                    });
                    newMarkers[id].addListener('click', function () {
                        window.location = "http://" + window.location.host + "/#/deal/" + this.id;
                    });
                    newMarkers[id].offer = offer;
                    $scope.infoWindows[id].open($scope.googleMap, newMarkers[id]);
                }
            }

            for (var marker in $scope.markers) {
                if ($scope.markers[marker] !== null) {
                    $scope.markers[marker].setMap(null);
                    $scope.infoWindows[marker.offer.offer.id] = null;
                }
            }

            $scope.markers = newMarkers;
        }
    };
    offersStream.callback = $scope.callback;
}]);

otterpopControllers.controller("CouponsCtrl", ["$scope", "ajaxFactory", "$routeParams", "$modal", function ($scope, ajaxFactory, $routeParams, $modal) {
    $scope.global.stopbodyoverflow = false; //scrollable
    $scope.draggedABit = false;
    $scope.global.currentPage = "coupons";

    $scope.now = Date.now();

    //Get my current coupons
    ajaxFactory.ajaxRequest("user/" + $scope.global.userId + "/coupons", "GET").then(function (result) {
        //Result should be coupons
        $scope.global.userCoupons = result.data;
        for (var i = 0; i < result.data.length; i++) {
            $scope.global.userCoupons[i].expiryMillis = $scope.convertTimeStampToUTC($scope.global.userCoupons[i].expiry);
            var tilExpired = (new Date(result.data[i].expiry)).getTime() - (new Date()).getTime();
            result.data[i].prettyExpiry = ((tilExpired / (60 * 60 * 1000) > 1) ? Math.floor(tilExpired / (60 * 60 * 1000)) + " hours, " : "") + (((tilExpired / (60 * 1000)) % 60 > 1) ? (Math.floor(tilExpired / (60 * 1000)) % 60) + " minutes" : "1 minutes");
        }
    });

    //Can redeem a coupon
    $scope.redeemCoupon = function (index) {
        //Tell the backend we're redeeming an offer
        ajaxFactory.ajaxRequest("coupon/" + $scope.global.userCoupons[index].id + "/redeem?name=" + $scope.global.userId, "POST").success(function(result) {
            if (result) {
                $scope.global.userCoupons[index].redeemed = true;
            }
        });
    }

    //Can delete a coupon
    $scope.deleteCoupon = function (index) {
        //Tell the backend we don't want the offer any more?
        //ajaxFactory.ajaxRequest("coupon/" + $scope.global.userCoupons[index].id + "/redeem?name=" + $scope.global.userId, "POST");
    }

    //Open settings modal
    $scope.reviewCoupon = function (index) {
        var modalInstance = $modal.open({
            templateUrl: "fragments/review.html",
            controller: "ReviewCtrl",
            size: "review"
        });

        modalInstance.result.then(function () {
            //Probably update the coupon so it says 'reviewed'
            /**
            ajaxFactory.ajaxRequest("restaurant/" + $routeParams.restaurantId + "/settings")
                  .success(function (result) {
                      $scope.restaurant = result;
                      $scope.slider.options.ceil = $scope.restaurant.offerSettings.seats;
                      $scope.$apply();
                  })
                  .error(function (data, status, headers, config) {
                      // HACKATHON, NO ERROR HANDLING, AHAHAHAHAA
                  });
             */
        });
    };
}]);

otterpopControllers.controller("ReviewCtrl", ["$scope", "ajaxFactory", "$modalInstance", "$routeParams", function ($scope, ajaxFactory, $modalInstance, $routeParams) {
    $scope.cancel = function () {
        $modalInstance.dismiss("cancel");
    };

    $scope.saveChanges = function () {
        //Submit the review
        //ajaxFactory.ajaxRequest("restaurant/" + $routeParams.restaurantId + "/settings", "PUT", $scope.restaurant)
        //      .success(function (result) {
                  $modalInstance.close();
        //      })
        //      .error(function (data, status, headers, config) {
                  // HACKATHON, NO ERROR HANDLING, AHAHAHAHAA
        //      });
    }
}]);

otterpopControllers.controller("ViewCouponCtrl", ["$scope", "ajaxFactory", "$routeParams", "offersStream", "$interval", function($scope, ajaxFactory, $routeParams, offersStream, $interval) {
    $scope.showingDeal = false;
    $scope.global.currentPage = "viewcoupon";

    //Get my current coupons
    ajaxFactory.ajaxRequest("user/" + $scope.global.userId + "/coupons", "GET").then(function(result) {
        //Result should be a coupon, add it to users list
        $scope.global.userCoupons = result.data;
        for (var i = 0; i < result.data.length; i++)
            if (result.data[i].id === $routeParams.couponId) {
                $scope.shownOffer = result.data[i].offer;
                $scope.shownCoupon = result.data[i];
            }
        //$scope.$apply();
        if (!$scope.shownCoupon)
            window.location = "http://" + window.location.host + "/#/";
    });

    //Can redeem a coupon
    $scope.redeemCoupon = function () {
        //Tell the backend we're redeeming an offer
        ajaxFactory.ajaxRequest("coupon/" + $scope.shownCoupon.id + "/redeem?name=" + $scope.global.userId, "POST");
    }

    //Can delete a coupon
    $scope.deleteCoupon = function () {
        //Tell the backend we don't want the offer any more?
        //ajaxFactory.ajaxRequest("coupon/" + $scope.global.activeCoupon.id + "/redeem?name=" + $scope.global.userId, "POST");
    }
}]);

otterpopControllers.controller("ViewDealCtrl", ["$scope", "ajaxFactory", "$routeParams", "offersStream", "$interval", "$modal", function ($scope, ajaxFactory, $routeParams, offersStream, $interval, $modal) {
    $scope.global.stopbodyoverflow = false;
    $scope.global.currentPage = "viewdeal";
    $scope.showingDeal = true;
    //Check if the offersStream has the offer with the id we want (this will normally just check the known offers, but after a refresh or something we'll need to find it)
    $scope.checkForMyDeal = function (data) {
        //Do nothing if we've already found it
        if ($scope.shownOffer && $scope.shownOffer.offer.id === $routeParams.dealId)
            return;
        //Check if stream has deal
        for (var i = 0; i < offersStream.offers.length; i++) {
            if (offersStream.offers[i].offer.id === $routeParams.dealId) {
                $scope.shownOffer = offersStream.offers[i];
                $scope.timeDifference = (new Date($scope.shownOffer.expiry)).getTime() - (new Date()).getTime();
                return;
            }
        }
        if (!$scope.shownOffer)
            window.location = "http://" + window.location.host + "/#/";
    };
    $scope.checkForMyDeal();
    //Yes, this is an insane way of observing a stream
    offersStream.callback = $scope.checkForMyDeal;

    //Can decline an offer
    $scope.declineOffer = function() {
        var offer = $scope.shownOffer;
        //Remove the offer
        for (var i = 0; i < offersStream.offers.length; i++)
            if (offersStream.offers[i].offer.id === offer.offer.id) {
                offersStream.offers.splice(i, 1);
                break;
            }
        //Tell the backend we declined an offer
        ajaxFactory.ajaxRequest("restaurant/" + offer.restaurant.id + "/offer/" + offer.offer.id + "/status/reject", "POST");
        //Redirect to home base
        window.location = "http://" + window.location.host + "/#/";
    }

    $scope.isClaiming = false;
    //Can accept an offer
    $scope.acceptOffer = function () {
        //Set the active offer to be the offer (possibly update user here... ?)
        var offer = $scope.shownOffer;
        //Tell the backend we accepted an offer
        ajaxFactory.ajaxRequest("restaurant/" + offer.restaurant.id + "/offer/" + offer.offer.id + "/status/claim?userId=" + $scope.global.userId, "POST").success(function (result) {
            //Result should be a coupon, but w/e
            window.location = "http://" + window.location.host + "/#/coupons";
        }).error(function () {
            var modalInstance = $modal.open({
                templateUrl: "fragments/sorry.html",
                size: "sorry"
            });

                var func = function() {
                    window.location = "http://" + window.location.host + "/#/";
                };
            modalInstance.result.then(func , func , func);
        }
        );
    }
}]);