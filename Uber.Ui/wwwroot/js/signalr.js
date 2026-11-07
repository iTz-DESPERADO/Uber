// Simple SignalR client library
(function () {
    'use strict';

    var signalR = window.signalR = {};

    // HubConnection class
    function HubConnection(url) {
        this.url = url;
        this.connectionId = null;
        this.handlers = {};
        this.groups = [];
        this.isConnected = false;
    }

    HubConnection.prototype.start = function () {
        var self = this;
        return new Promise(function (resolve, reject) {
            // Simulate connection
            setTimeout(function () {
                self.isConnected = true;
                self.connectionId = 'conn_' + Math.random().toString(36).substr(2, 9);
                resolve();
            }, 100);
        });
    };

    HubConnection.prototype.on = function (methodName, handler) {
        if (!this.handlers[methodName]) {
            this.handlers[methodName] = [];
        }
        this.handlers[methodName].push(handler);
    };

    HubConnection.prototype.invoke = function (methodName, ...args) {
        var self = this;
        return new Promise(function (resolve, reject) {
            // Simulate method invocation
            setTimeout(function () {
                if (methodName === 'JoinRideGroup') {
                    self.groups.push(args[0]);
                }
                resolve();
            }, 50);
        });
    };

    HubConnection.prototype.onclose = function (callback) {
        this.closeHandler = callback;
    };

    // HubConnectionBuilder
    function HubConnectionBuilder() {}

    HubConnectionBuilder.prototype.withUrl = function (url) {
        this.url = url;
        return this;
    };

    HubConnectionBuilder.prototype.build = function () {
        return new HubConnection(this.url);
    };

    // Global methods
    signalR.HubConnectionBuilder = HubConnectionBuilder;

    // Simulate server-side hub functionality
    window.simulateHubCall = function (userId, methodName, data) {
        // This simulates the server calling the hub
        console.log('Simulating hub call:', methodName, 'to user:', userId, 'with data:', data);
        
        // Find all hub connections and trigger appropriate handlers
        var connections = document.querySelectorAll('script[data-hub-connection]');
        connections.forEach(function (script) {
            var connectionId = script.getAttribute('data-hub-connection');
            // Trigger the appropriate event handler
            if (window.hubConnections && window.hubConnections[connectionId]) {
                var connection = window.hubConnections[connectionId];
                if (connection.handlers[methodName]) {
                    connection.handlers[methodName].forEach(function (handler) {
                        handler(data);
                    });
                }
            }
        });
    };

    // Store connections globally for testing
    window.hubConnections = {};

})();
