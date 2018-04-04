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

        // var commonLib = new common();
        var commonLib = require('./common.js');
        if (!commonLib.localStorageSupported()) {
            alert("Local storage must be enabled to use this application.");
            throw "Local storage must be enabled to use this application.";
        }

        var persistMetadata = function() {
            var komodoMetadata = {
                "hostname": hostname,
                "port": port,
                "ssl": ssl,
                "apiKey": apiKey,
                "headers": headers,
                "indices": indices,
                "indexName": indexName
            };

            commonLib.localStorageSet("komodoMetadata", komodoMetadata);
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

        $scope.setApiKey = function() {
            console.log("Set API key clicked");
            var key = document.getElementById("apiKey").value;

            if (commonLib.isEmpty(key)) {
                alert("Please enter your API key.");
                return;
            }

            console.log("API key set: " + key);

            var headers = {
                "x-api-key": key
            };
 
            commonLib.restRequest("GET", "/indices", hostname, port, headers, "application/json", null, false)
                .then(
                    function(data) {
                        // login success
                        console.log("API key validated");
                        commonLib.localStorageSet("komodoApiKey", key);

                        if (!data || data.length < 1) {
                            alert("No indices found.  Please configure an index first.");
                            return;
                        } 

                        indices = data; 
                        console.log("Indices detected (" + indices.length + "): " + indices);

                        // save metadata to local storage
                        persistMetadata();

                        // move to select index view
                        window.location = "setIndex.html";
                        return;                        
                    },
                    function(err) {
                        // login failed
                        alert("Invalid API key.");
                        console.log("REST error: " + err);
                        return;                        
                    }
                );
            return;
        };
    }); 

})(); // self-running function