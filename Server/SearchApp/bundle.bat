@echo off
browserify common.js -o common.bundle.js
browserify index.js -o index.bundle.js
browserify setIndex.js -o setIndex.bundle.js
browserify search.js -o search.bundle.js
