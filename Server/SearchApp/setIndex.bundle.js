(function e(t,n,r){function s(o,u){if(!n[o]){if(!t[o]){var a=typeof require=="function"&&require;if(!u&&a)return a(o,!0);if(i)return i(o,!0);var f=new Error("Cannot find module '"+o+"'");throw f.code="MODULE_NOT_FOUND",f}var l=n[o]={exports:{}};t[o][0].call(l.exports,function(e){var n=t[o][1][e];return s(n?n:e)},l,l.exports,e,t,n,r)}return n[o].exports}var i=typeof require=="function"&&require;for(var o=0;o<r.length;o++)s(r[o]);return s})({1:[function(require,module,exports){
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

    var commonLib = new common();
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
},{}]},{},[1]);
