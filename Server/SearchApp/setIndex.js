(function(){ 
	console.log("Komodo search application loaded");
 
	var app = angular.module("komodo", [ ]);

	var hostname = "localhost";
	var port = 9090;
  var ssl = false;
  var apiKey = null;
  var headers = { };
  var indices = [];
  var indexName = null;

  app.controller("searchController", function($scope) {
  	console.log("Komodo controller loaded");

    var commonLib = require('./common.bundle.js');
    if (!commonLib.localStorageSupported()) {
      alert("Local storage must be enabled to use this application.");
      throw "Local storage must be enabled to use this application.";
    }

    var persistMetadata = function() {
            var md = {
                "hostname": hostname,
                "port": port,
                "ssl": ssl,
                "apiKey": apiKey,
                "headers": headers,
                "indices": indices,
                "indexName": indexName
            };

            commonLib.localStorageSet("komodoMetadata", md);
        };

        var retrieveMetadata = function() {
            var md = commonLib.localStorageGet("komodoMetadata");
            if (!md || md === null || md === undefined) {
                return false;
            }
            else {
                hostname = md.hostname;
                port = md.port;
                ssl = md.ssl;
                apiKey = md.apiKey;
                headers = md.headers;
                indices = md.indices;
                indexName = md.indexName;
                return true;
            }
        }

        if (!retrieveMetadata()) {
          console.log("Unable to retrieve metadata from local storage, redirecting to index");
          window.location = "index.html";
          return;
        }

        $scope.indices = indices;
        $scope.selectedIndex = null; // not necessary

        $scope.setIndex = function() {
          if (!$scope.selectedIndex) {
              alert("Please choose an index.");
              return;
          }
          console.log("Set index clicked: " + $scope.selectedIndex);
          this.indexName = $scope.selectedIndex;
          window.location = "search.html";
        };

    }); 

})(); // self-running function