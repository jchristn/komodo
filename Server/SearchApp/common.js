/*

Common methods

*/

'use strict';

class common {
  constructor() {

  };

  localStorageSupported() {
		if (typeof(Storage) !== "undefined") {
			return true;
		} 
		else {
		  return false;
		}
  };

  localStorageGet(key) {
  	if (!key) throw "No key supplied for localStorageGet";
  	return localStorage.getItem(key);
  };

  localStorageSet(key, val) {
  	if (!key) throw "No key supplied for localStorageSet";
  	if (!val) throw "No val supplied for localStorageSet";
  	return localStorage.setItem(key, val);
  };

  isEmpty(val) {
		return (val === undefined || val === null || val.length <= 0);
  };

  restRequest(verb, path, host, port, headers, contentType, data) {
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

      if (data && data.length > 0) console.log(verb + " " + host + ":" + port + path + " [" + data.length + " bytes]");
      else console.log(verb + " " + host + ":" + port + path);

      process.env.NODE_TLS_REJECT_UNAUTHORIZED = "0";

      var callbackInternal = function(response) {
          var responseBody = '';
          var statusCode = response.statusCode;

          response.on('error', function (error) {
              console.log("REST error encountered: " + error);
              reject(error);
          });

          response.on('data', function (chunk) {
              responseBody += chunk;
          });

          response.on('end', function () {
              console.log(verb + " " + host + ":" + port + path + " " + statusCode + " [" + responseBody.length + " bytes]");
              if (statusCode > 299) {
                  reject(JSON.parse(responseBody));
              }
              else {
                  resolve(JSON.parse(responseBody));
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

};

module.exports = common;
