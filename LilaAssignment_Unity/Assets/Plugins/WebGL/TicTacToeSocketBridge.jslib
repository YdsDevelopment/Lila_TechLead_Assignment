mergeInto(LibraryManager.library, {
  TTT_SocketConnect: function (serverUrlPointer, gameObjectPointer) {
    var serverUrl = UTF8ToString(serverUrlPointer);
    console.log("ReceiveAdaptorMessage TTT_SocketConnect : " + serverUrl);
    var gameObjectName = UTF8ToString(gameObjectPointer);
    var bridge = window.TicTacToeWebGLSocketBridge || {};
    console.log("ReceiveAdaptorMessage TTT_SocketConnect : gameObj  " + gameObjectName);
    if (bridge.socket) {
      bridge.socket.disconnect();
    }

    bridge.gameObjectName = gameObjectName;
    bridge.send = function (eventName, payload) {
      console.log("connect function : " + eventName  + " Payload : " + payload);
      var envelope = JSON.stringify({ eventName: eventName, payload: payload || {} });
      console.log("connect function : " + eventName  + " Payload : " + envelope);
      SendMessage(bridge.gameObjectName, "OnSocketBridgeMessage", envelope);
    };

    if (typeof window.io !== "function") {
      bridge.send("socket-error", {
        message: "Socket.IO browser client is not loaded. Add /socket.io/socket.io.js to the WebGL template."
      });
      window.TicTacToeWebGLSocketBridge = bridge;
      return;
    }

    bridge.socket = window.io(serverUrl, {
      transports: ["websocket", "polling"],
      autoConnect: true
    });

    bridge.socket.on("connect", function () {
      console.log("connect TTT_SocketConnect : " + bridge.socket.id);
      bridge.send("socket-connected", { socketId: bridge.socket.id });
    });

    bridge.socket.on("disconnect", function (reason) {
      console.log("disconnect TTT_SocketConnect : " + reason);
      bridge.send("socket-disconnected", { reason: reason });
    });

    bridge.socket.on("connect_error", function (error) {
      console.log("connect_error TTT_SocketConnect : " + error);
      bridge.send("socket-error", { message: error && error.message ? error.message : "Socket connection failed" });
    });

    bridge.socket.on("reconnect_attempt", function (attempt) {
      console.log("reconnect_attempt TTT_SocketConnect : " + attempt);
      bridge.send("socket-reconnect-attempt", { attempt: attempt });
    });

    [
      "room-created",
      "room-joined",
      "player-joined",
      "game-started",
      "move-result",
      "turn-timer",
      "player-disconnected",
      "player-reconnected",
      "player-left",
      "room-state",
      "rooms-list",
      "room-details",
      "health-status",
      "error"
    ].forEach(function (eventName) {
      bridge.socket.on(eventName, function (payload) {
        console.log("eventName TTT_SocketConnect : " + payload);
        bridge.send(eventName, payload);
      });
    });
    console.log("connect function :  TicTacToeWebGLSocketBridge assigned");
    window.TicTacToeWebGLSocketBridge = bridge;
  },

  TTT_SocketEmit: function (eventNamePointer, payloadPointer) {
    var bridge = window.TicTacToeWebGLSocketBridge;
    var eventName = UTF8ToString(eventNamePointer);
    var payloadJson = UTF8ToString(payloadPointer);

    if (!bridge || !bridge.socket || !bridge.socket.connected) {
      return;
    }

    try {
      bridge.socket.emit(eventName, JSON.parse(payloadJson));
    } catch (error) {
      bridge.send("socket-error", {
        message: error && error.message ? error.message : "Unable to emit Socket.IO event"
      });
    }
  },

  TTT_SocketDisconnect: function () {
    var bridge = window.TicTacToeWebGLSocketBridge;
    if (bridge && bridge.socket) {
      bridge.socket.disconnect();
    }
  },

  TTT_SocketIsConnected: function () {
    var bridge = window.TicTacToeWebGLSocketBridge;
    return bridge && bridge.socket && bridge.socket.connected ? 1 : 0;
  }
});
