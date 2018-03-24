(function(){ 
	console.log("Komodo search application loaded");

	var app = angular.module("komodo", [ ]);

	var hostname = "localhost";
	var port = 9090;
    var ssl = false;

	var isEmpty = function(val){
    	return (val === undefined || val == null || val.length <= 0);
	};

  var restRequest = function(verb, path, host, port, headers, contentType, data, debug) {
  	return new Promise(function (resolve, reject) {
        var self = this;
        var http = require("http");
        var https = require("https");

        if (!headers) headers = {};
        if (data) headers["content-length"] = data.length;
        if (contentType) headers["content-type"] = contentType;

        var options = {
            host: host,
            port: port,
            path: path,
            headers: headers,
            method: verb
        };

        console.log("restRequest " + verb + " " + host + ":" + port + " " + path);
        console.log(data);

        process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";

        var callbackInternal = function(response) {
            var responseBody = '';
            var statusCode = response.statusCode;

            response.on('error', function (error) {
                console.log("restRequest error encountered: " + error);
                reject(error);
            });

            response.on('data', function (chunk) {
                console.log("restRequest retrieved data: " + chunk.length + " bytes");
                responseBody += chunk;
            });

            response.on('end', function () {
                console.log("restRequest status " + statusCode + ": " + responseBody.length);
                if (debug) {
                	console.log(responseBody);
                }

                if (statusCode > 299) {
                    // error
                    reject(responseBody);
                }
                else {
                    // data
                    resolve(responseBody);
                }
            });
        };

        var req;
        if (ssl) req = https.request(options, callbackInternal);
        else req = http.request(options, callbackInternal);

        if (data) req.write(data);
        req.end();
      });
  };
 
  var searchResult = function(result) {
  	console.log("Search results retrieved:");
  	console.log(result);
  	return;
  };
 
	app.controller("searchController", function($scope) {
		console.log("Komodo controller loaded");

		$scope.searchSubmit = function() {
			console.log("Seach clicked");

			var required = document.getElementById("required").value;
			var optional = document.getElementById("optional").value;
			var excluded = document.getElementById("excluded").value

			if (!required) {
				alert("Please provide required terms.");
				return;
			}

			var requiredTerms, optionalTerms, excludedTerms;
			if (required) requiredTerms = required.split(" ");
 			if (optional) optionalTerms = optional.split(" ");
 			if (excluded) excludedTerms = excluded.split(" ");
 
			console.log("Search data retrieved");

			var headers = {
				"x-api-key": "default"
			};

			var req = {
				"MaxResults": 10,
				"StartIndex": null,
				"Required": {
					"Terms": requiredTerms,
					"Filter": []
				},
				"Optional": {
					"Terms": optionalTerms,
					"Filter": []
				},
				"Exclude": {
					"Terms": excludedTerms,
					"Filter": []
				},
				"IncludeContent": false,
				"IncludeParsedDoc": true
			};

			restRequest("PUT", "/First", hostname, port, headers, "application/json", 
				JSON.stringify(req), false).then(searchResult);
			return;
    };
 
	}); 
})(); // self-running function