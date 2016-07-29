var otterpopControllers = angular.module("otterpopControllers", ["otterpopServices"]);

otterpopControllers.controller("RootCtrl", ["$scope", "ajaxFactory", "$modal", function ($scope, ajaxFactory, $modal) {
    //Stuff to be global, if anything
    $scope.global = {};

    $scope.logout = function () {
        delete localStorage["token"];
        delete localStorage["password"];
        window.location = "http://" + window.location.host + "/restaurant/#/login";
    }

    if ((!localStorage["token"] || !localStorage["password"]) && window.location !== "http://" + window.location.host + "/restaurant/#/login")
        window.location = "http://" + window.location.host + "/restaurant/#/login";
    else {
        //Ajax call goes here, should get restaurantId in return
        ajaxFactory.ajaxRequest("user/" + localStorage["token"] + "/restaurant")
              .success(function (result) {
                  ajaxFactory.ajaxRequest("restaurant/" + result + "/settings")
                        .success(function (result) {
                            $scope.updates = WebStream("/stream/restaurant", { restaurantId: result.details.id });
                            $scope.global.restaurant = result;
                            //Listen to updates
                          if ($scope.global.doIfRestaurant)
                              $scope.global.doIfRestaurant();
                      })
                        .error(function (data, status, headers, config) {
                            // :(
                        });
              })
              .error(function (data, status, headers, config) {
                $scope.logout();
            });
    }
    
    //Time functions
    $scope.convertTimeStampToDate = function (timeStamp) {
        return new Date(timeStamp);
    }
    $scope.convertTimeStampToUTC = function (timeStamp) {
        return (new Date(timeStamp)).getTime();
    }

    //Replace restaurant image stuff
    $scope.global.replaceImage = function (replacewhich) {
        if ($scope.global.currentPage !== "settings")
            return;

        var modalInstance = $modal.open({
            templateUrl: "fragments/imagecrop.html",
            controller: "CropperCtrl",
            size: window.innerWidth < 780 ? "fullsize" : "vlarge"
        });

        modalInstance.result.then(function (imageblob) {
            $scope.busy = true;
            ajaxFactory.imagePost("restaurant/" + $scope.global.restaurant.details.id + "/photo", "POST", imageblob)
              .success(function (result) {
                  if (!$scope.global.restaurant.details.imageUrls)
                      $scope.global.restaurant.details.imageUrls = [];
                  if (replacewhich === -2)
                      $scope.global.restaurant.details.logoUrl = result;
                  else if (replacewhich === -1)
                      $scope.global.restaurant.details.imageUrls.push(result);
                  else
                      $scope.global.restaurant.details.imageUrls[replacewhich] = result;
                  $scope.busy = false;
                  $scope.saveImageChanges();
              })
                .error(function (data, status, headers, config) {
                    $scope.busy = false;
                    $scope.uploadfailure = true;
                    document.getElementById("createfile").value = "";
                    if (status === 415)
                        $scope.showErrorModal({ title: "Bad Image Format", message: f.name + " is not an image of a supported format. Please upload a png or jpg file." });
                    else
                        $scope.showErrorModal({ title: "Error During Upload", message: "An error occurred while uploading the image " + f.name + " - " + status + "." });
                });
        });
    };

    $scope.saveImageChanges = function () {
        ajaxFactory.ajaxRequest("restaurant/" + $scope.global.restaurant.details.id + "/settings", "PUT", $scope.global.restaurant)
              .success(function (result) {
              })
              .error(function (data, status, headers, config) {
                  // HACKATHON, NO ERROR HANDLING, AHAHAHAHAA
              });
    }
}]);

otterpopControllers.controller("SettingsCtrl", ["$scope", "ajaxFactory", "$modal", function ($scope, ajaxFactory, $modal) {
    //Track which page we're on
    $scope.global.currentPage = "settings";

    $scope.saverChanges = function () {
        ajaxFactory.ajaxRequest("restaurant/" + $scope.global.restaurant.details.id + "/settings", "PUT", $scope.global.restaurant)
              .success(function (result) {
              })
              .error(function (data, status, headers, config) {
                  // HACKATHON, NO ERROR HANDLING, AHAHAHAHAA
              });
    }
    $scope.saveChanges = $.debounce(1000, $scope.saverChanges);
}]);

otterpopControllers.controller("RestaurantImagesCtrl", ["$scope", "ajaxFactory", "$modal", function ($scope, ajaxFactory, $modal) {
    //Track which page we're on
    $scope.global.currentPage = "settings";

    $scope.hoverval = [];
    $scope.hovervale = false;

    $scope.sortableOptions = {
        update: function (e, ui) {

        },
        stop: function (e, ui) {
            $scope.saveChanges();
        },
        items: "> div.candrag",
        cancel: ".disabledrag"
    };

    $scope.sortableOptions = {
        update: function (e, ui) {

        },
        stop: function (e, ui) {
            $scope.saveImageChanges();
        },
        items: "> div.candrag",
        cancel: ".disabledrag"
    };
}]);

otterpopControllers.controller("CustomersCtrl", ["$scope", "ajaxFactory", function ($scope, ajaxFactory) {
    //Track which page we're on
    $scope.global.currentPage = "coupons";

    //Figure out what we're showing
    $scope.showRedeemed = window.location.hash === ("#/");
    $scope.now = Date.now();
    $scope.expanded = [];
    $scope.sortedCoupons = [];

    $scope.$watch("coupons", function () {
        if (!$scope.coupons)
            return;
        $scope.sortedCoupons = [];
        for (var i = 0; i < $scope.coupons.length; i++)
            if ($scope.showArchived || $scope.coupons[i].accepted === 'none')
                $scope.sortedCoupons.push($scope.coupons[i]);
    });

    //Listen to updates
    $scope.updateFunction = function (update) {
        //Update stuff
        if (update.redeemingCoupon) {
            //Update coupon
            for (var i = 0; i < $scope.coupons.length; i++)
                if ($scope.coupons[i].id === update.redeemingCoupon.id) {
                    $scope.coupons[i].redeemed = true;
                    $scope.$apply();
                    break;
                }
        }
        if (update.customerCoupon) {
            for (var i = 0; i < $scope.coupons.length; i++)
                if ($scope.coupons[i].id === update.customerCoupon.id) {
                    $scope.coupons[i].accepted = update.customerCoupon.status;
                    if ($scope.coupons[i].accepted !== "none")
                        for (var j = 0; j < $scope.sortedCoupons.length; j++)
                            if ($scope.sortedCoupons[j].id === update.customerCoupon.id) {
                                $scope.sortedCoupons.splice(j, 1);
                                break;
                            }
                    $scope.$apply();
                    break;
                }
        }
        if (update.newCoupon) {
            $scope.sortedCoupons.push(update.newCoupon.coupon);
            $scope.coupons.push(update.newCoupon.coupon);
            $scope.coupons[$scope.coupons.length - 1].expiryMillis = $scope.convertTimeStampToUTC(update.newCoupon.coupon.expiry);
            $scope.$apply();
        }
    };

    //Get some coupons
    $scope.getCoupons = function() {
        if (window.location.hash === ("#/archive")) {
            $scope.showArchived = true;
            ajaxFactory.ajaxRequest("restaurant/" + $scope.global.restaurant.details.id + "/archivedcoupons")
                  .success(function (result) {
                      $scope.coupons = result;
                      for (var i = 0; i < result.length; i++) {
                          $scope.coupons[i].expiryMillis = $scope.convertTimeStampToUTC($scope.coupons[i].expiry);
                      }
                      //Subscribe after we get the offers
                    if ($scope.global.subscription)
                        $scope.global.subscription.dispose();
                    $scope.global.subscription = $scope.updates.subscribe($scope.updateFunction);
                  })
                  .error(function (data, status, headers, config) {
                      // HACKATHON, NO ERROR HANDLING, AHAHAHAHAA
                  });
        } else {
            $scope.showArchived = false;
            ajaxFactory.ajaxRequest("restaurant/" + $scope.global.restaurant.details.id + "/coupons")
                  .success(function (result) {
                      $scope.coupons = result;
                      for (var i = 0; i < result.length; i++)
                          $scope.coupons[i].expiryMillis = $scope.convertTimeStampToUTC($scope.coupons[i].expiry);
                      //Subscribe after we get the offers
                      if ($scope.global.subscription)
                          $scope.global.subscription.dispose();
                      $scope.global.subscription = $scope.updates.subscribe($scope.updateFunction);
                  })
                  .error(function (data, status, headers, config) {
                      // HACKATHON, NO ERROR HANDLING, AHAHAHAHAA
                  });
        }
    };
    $scope.global.doIfRestaurant = function () {
        if ($scope.global.restaurant)
            $scope.getCoupons();
    }
    if ($scope.global.restaurant)
        $scope.global.doIfRestaurant();

    //Need functions for accept/reject; below should work
    $scope.rejectCoupon = function (index) {
        $scope.coupons[index].accepted = "rejected";
        ajaxFactory.ajaxRequest("coupon/" + $scope.coupons[index].id + "/reject", "POST").finally(function () { });
    }
    $scope.acceptCoupon = function (index) {
        $scope.coupons[index].accepted = "accepted";
        ajaxFactory.ajaxRequest("coupon/" + $scope.coupons[index].id + "/accept", "POST").finally(function () { });
    }
}]);

otterpopControllers.controller("OffersCtrl", ["$scope", "ajaxFactory", "$modal", function($scope, ajaxFactory, $modal) {
    //Track which page we're on
    $scope.global.currentPage = "offers";
    $scope.updateOffer = [];
    $scope.isopen = [];

    //Grab offers that have been issued
    $scope.global.doIfRestaurant = function () {
        if ($scope.global.restaurant)
        ajaxFactory.ajaxRequest("restaurant/" + $scope.global.restaurant.details.id + "/offer/all")
            .success(function(result) {
                for (var i = 0; i < result.length; i++) {
                    if (result[i].hidden) {
                        result.splice(i, 1);
                        i--;
                    }
                }
                $scope.offers = result;
                //Subscribe after we get the offers
                if ($scope.global.subscription)
                    $scope.global.subscription.dispose();
                $scope.global.subscription = $scope.updates.subscribe($scope.updateFunction);
                //Create update functions
                for (var i = 0; i < result.length; i++) {
                    var func = function (id) {
                        return function () {
                            var index = null;
                            for (var j = 0; j < $scope.offers.length; j++)
                                if ($scope.offers[j].id === id) {
                                    index = j;
                                    break;
                                }
                            if (index) {
                                var offer = $scope.offers[index];
                                if (offer.hidden === true)
                                    $scope.offers.splice(index, 1);
                                ajaxFactory.ajaxRequest("restaurant/" + $scope.global.restaurant.details.id + "/offer/update", "POST", offer).success(function (result) {
                                    if (!offer.hidden)
                                        $scope.offers[index] = result;
                                    $scope.$apply();
                                }).error(function (data, status, headers, config) { });
                            }
                        };
                    };
                    $scope.updateOffer[i] = $.debounce(1000, func($scope.offers[i].id));
                    $scope.isopen[i] = false;
                }
            })
              .error(function (data, status, headers, config) {
                  // HACKATHON, NO ERROR HANDLING, AHAHAHAHAA
              });
    }
    if ($scope.global.restaurant)
        $scope.global.doIfRestaurant();

    //Listen to updates
    $scope.updateFunction = function(update) {
        //Update stuff
        if (update.offer) {
            //Update offer
            for (var i = 0; i < $scope.offers.length; i++)
                if ($scope.offers[i].id === update.offer.id) {
                    if (update.offer.hidden === true)
                        $scope.offers.splice(i, 1);
                    else
                        $scope.offers[i] = update.offer;
                    $scope.$apply();
                    break;
                }
        }
    };

    $scope.toggleDropdown = function (index) {
        $scope.isopen[index] = !$scope.isopen[index];
    };

    //Delete (hide) offer
    $scope.deleteOffer = function (index) {
        if (!$scope.offers[index].active) {
            $scope.offers[index].hidden = true;
            //Send it to the server
            $scope.updateOffer[index]();
        }
    };

    //Publish/unpublish offer
    $scope.publishOffer = function (index) {
        $scope.offers[index].active = !$scope.offers[index].active;
        //Send it to the server
        $scope.updateOffer[index]();
    };

    //Open offer settings modals
    $scope.global.offerSettings = function (index) {
        if (index >= 0)
            $scope.isopen[index] = false;

        if (window.innerWidth < 710) {
            window.location = "http://" + window.location.host + "/restaurant/#/createoffer/" + ((index >= 0) ? $scope.offers[index].id : "");
            return;
        }

        var modalInstance = $modal.open({
            templateUrl: "fragments/createoffer.html",
            controller: "OfferDetailsModalCtrl",
            size: "offersettings",
            resolve: {
                offer: function () {
                    var obj = {
                        published: "draft",
                        expires: new Date()
                    };
                    if (index >= 0)
                        return JSON.parse(JSON.stringify($scope.offers[index]));
                    else
                        return obj;
                },
                restaurant: function() {
                    return $scope.global.restaurant;
                }
            }
        });

        modalInstance.result.then(function (offer) {
            var modalInstance2 = $modal.open({
                templateUrl: "fragments/createofferimages.html",
                controller: "OfferDetailsModalCtrl",
                size: "offersettingsimages",
                resolve: {
                    offer: function () { return offer; },
                    restaurant: function () {
                        return $scope.global.restaurant;
                    }
                }
            });

            modalInstance2.result.then(function (aoffer) {
                $scope.offers[index] = aoffer;
                if (index > 0)
                    $scope.updateOffer[index]();
                else
                    ajaxFactory.ajaxRequest("restaurant/" + $scope.global.restaurant.details.id + "/offer/update", "POST", aoffer).success(function (result) {
                        $scope.offers.push(result);
                        $scope.$apply();
                    }).error(function (data, status, headers, config) { });
            });
        });
    };
}]);

otterpopControllers.controller("OfferDetailsCtrl", ["$scope", "ajaxFactory", "$routeParams", "$modal", function ($scope, ajaxFactory, $routeParams, $modal) {
    $scope.shortMonth = function (m) {
        if (m === 0)
            return "Jan";
        if (m === 1)
            return "Feb";
        if (m === 2)
            return "Mar";
        if (m === 3)
            return "Apr";
        if (m === 4)
            return "May";
        if (m === 5)
            return "Jun";
        if (m === 6)
            return "Jul";
        if (m === 7)
            return "Aug";
        if (m === 8)
            return "Sep";
        if (m === 9)
            return "Oct";
        if (m === 10)
            return "Nov";
        return "Dec";
    }
    $scope.readableDate = function (d) {
        return ((d.getDay() < 10) ? '0' + d.getDay() : d.getDay()) + " " + $scope.shortMonth(d.getMonth()) + " " + d.getFullYear()
            + " " + ((d.getHours() < 10) ? '0' + d.getHours() : d.getHours()) + ':'
            + ((d.getMinutes() < 10) ? '0' + d.getMinutes() : d.getMinutes()) + ':'
            + ((d.getSeconds() < 10) ? '0' + d.getSeconds() : d.getSeconds());
    }
    $scope.convertDateToTimeStamp = function (d) {
        return ((d.getUTCDay() < 10) ? '0' + d.getUTCDay() : d.getUTCDay()) + " " + $scope.shortMonth(d.getUTCMonth()) + " " + d.getUTCFullYear()
            + " " + ((d.getUTCHours() < 10) ? '0' + d.getUTCHours() : d.getUTCHours()) + ':'
            + ((d.getUTCMinutes() < 10) ? '0' + d.getUTCMinutes() : d.getUTCMinutes()) + ':'
            + ((d.getUTCSeconds() < 10) ? '0' + d.getUTCSeconds() : d.getUTCSeconds()) + " +00:00";
    }

    $scope.handleHours = function () {
        $scope.hours = (((new Date($scope.offer.expires)).getTime() - Date.now()) - ((new Date($scope.offer.expires)).getTime() - Date.now()) % 3600000) / 3600000;
        if ($scope.hours < 0)
            $scope.hours = 0;
        $scope.offer.readableExpiry = $scope.readableDate(new Date($scope.offer.expires));
        $scope.hours = 3;
    }

    if (!$scope.offer) {
        $scope.global.doIfRestaurant = function () {
            $scope.restaurant = $scope.global.restaurant;
            if ($routeParams.offerId)
                ajaxFactory.ajaxRequest("restaurant/" + $scope.global.restaurant.details.id + "/offer/" + $routeParams.offerId, "GET").success(function (result) {
                    $scope.offer = result;
                    $scope.handleHours();
                    $scope.$apply();
                }).error(function (data, status, headers, config) { });
            else {
                $scope.offer = {
                    published: "draft",
                    expires: new Date()
                };
                $scope.handleHours();
            }
        };
        if ($scope.global.restaurant)
            $scope.global.doIfRestaurant();
    } else
        $scope.handleHours();

    $scope.hoverval = [];
    $scope.hovervale = false;
    $scope.replaceImage = function (replacewhich) {
        var modalInstance = $modal.open({
            templateUrl: "fragments/imagecrop.html",
            controller: "CropperCtrl",
            size: $scope.inModal ? "vlarge" : "fullsize"
        });

        modalInstance.result.then(function (imageblob) {
            $scope.busy = true;
            ajaxFactory.imagePost("restaurant/" + $scope.restaurant.details.id + "/photo", "POST", imageblob)
              .success(function (result) {
                  if (!$scope.offer.imageUrls)
                      $scope.offer.imageUrls = [];
                  if (replacewhich === -2)
                      $scope.offer.logoUrl = result;
                  else if (replacewhich === -1)
                      $scope.offer.imageUrls.push(result);
                  else
                      $scope.offer.imageUrls[replacewhich] = result;
                  $scope.busy = false;
              })
                .error(function (data, status, headers, config) {
                    $scope.busy = false;
                    $scope.uploadfailure = true;
                    document.getElementById("createfile").value = "";
                    if (status === 415)
                        $scope.showErrorModal({ title: "Bad Image Format", message: f.name + " is not an image of a supported format. Please upload a png or jpg file." });
                    else
                        $scope.showErrorModal({ title: "Error During Upload", message: "An error occurred while uploading the image " + f.name + " - " + status + "." });
                });
        });
    };

    $scope.sortableOptions = {
        update: function (e, ui) {
        },
        stop: function (e, ui) {
        },
        cancel: ".disabledrag"
    };

    $scope.$watch("hours", function () {
        if (!$scope.offer)
            return;
        var expdate = new Date();
        expdate.setTime(expdate.getTime() + $scope.hours * 3600000);
        $scope.offer.expires = $scope.convertDateToTimeStamp(expdate);
        $scope.offer.readableExpiry = $scope.readableDate(expdate);
    });

    $scope.saveChanges = function () {
        ajaxFactory.ajaxRequest("restaurant/" + $scope.global.restaurant.details.id + "/offer/update", "POST", $scope.offer).success(function (result) {
            window.location = "http://" + window.location.host + "/restaurant/#/offers";
        }).error(function (data, status, headers, config) { });
    }

    $scope.cancel = function () {
        window.location = "http://" + window.location.host + "/restaurant/#/offers";
    }
}]);

otterpopControllers.controller("OfferDetailsModalCtrl", ["$scope", "ajaxFactory", "$routeParams", "$modalInstance", "$modal", "$controller", "offer", "restaurant", function ($scope, ajaxFactory, $routeParams, $modalInstance, $modal, $controller, offer, restaurant) {
    $scope.offer = offer;
    $scope.restaurant = restaurant;
    $scope.inModal = true;
    angular.extend(this, $controller('OfferDetailsCtrl', { $scope: $scope }));

    $scope.done = function () {
        $modalInstance.close($scope.offer);
    };

    $scope.cancel = function () {
        $modalInstance.dismiss("cancel");
    };
}]);

otterpopControllers.controller("LoginCtrl", ["$scope", "ajaxFactory", function ($scope, ajaxFactory) {
    //Track which page we're on
    $scope.global.currentPage = "login";

    $scope.login = function () {
        //Update these
        localStorage["token"] = $scope.username;
        localStorage["password"] = $scope.password;

        //Sneakily make it mcdo
        ajaxFactory.ajaxRequest("user/" + localStorage["token"] + "/restaurant/mcdo", "POST")
              .success(function (result) {
                  //Ajax call goes here, should get restaurantId in return
                  ajaxFactory.ajaxRequest("user/" + localStorage["token"] + "/restaurant")
                        .success(function (result) {
                            $scope.restaurantId = result;
                            ajaxFactory.ajaxRequest("restaurant/" + result + "/settings")
                                  .success(function (result) {
                                      $scope.global.restaurant = result;
                                  })
                                  .error(function (data, status, headers, config) {
                                      // :(
                                  });
                            window.location = "http://" + window.location.host + "/restaurant/#/";
                        })
                        .error(function (data, status, headers, config) {
                            // HACKATHON, NO ERROR HANDLING, AHAHAHAHAA
                        });
              })
              .error(function (data, status, headers, config) {
                  // HACKATHON, NO ERROR HANDLING, AHAHAHAHAA
              });
    };
}]);

otterpopControllers.controller("CropperCtrl", ["$scope", "$modalInstance", function ($scope, $modalInstance) {
    $scope.uploadingimage = false;
    $scope.chooseimage = function () {
        if (!$scope.doneOnce) {
            $scope.doneOnce = true;
            document.getElementById("createafile").addEventListener("change", $scope.handleFileSelect); //This can't be done when setting up the controller because it's a modal, I believe
        }
        document.getElementById("createafile").click();
    }
    $scope.images = {};
    $scope.images.inimage = '';
    $scope.images.outimage = '';

    $scope.dataURItoBlob = function (dataURI) {
        // convert base64 to raw binary data held in a string
        // doesn't handle URLEncoded DataURIs - see SO answer #6850276 for code that does this
        var byteString = atob(dataURI.split(',')[1]);

        // separate out the mime component
        var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];

        // write the bytes of the string to an ArrayBuffer
        var ab = new ArrayBuffer(byteString.length);
        var ia = new Uint8Array(ab);
        for (var i = 0; i < byteString.length; i++) {
            ia[i] = byteString.charCodeAt(i);
        }

        // write the ArrayBuffer to a blob, and you're done
        return new Blob([ab], { type: mimeString });
    }

    $scope.done = function () {
        var blob = $scope.dataURItoBlob($scope.images.outimage);
        $modalInstance.close(blob);
    };

    $scope.cancel = function () {
        $modalInstance.dismiss("cancel");
    };

    $scope.handleFileSelect = function (e) {
        if (!e.target.files[0] || e.target.files[0] === "")
            return;
        var file = e.currentTarget.files[0];
        var reader = new FileReader();
        reader.onload = function (e) {
            $scope.images.inimage = e.target.result;
            $scope.uploadingimage = false;
            $scope.$apply();
        };
        $scope.uploadingimage = true;
        $scope.$apply();
        reader.readAsDataURL(file);
    };
}]);