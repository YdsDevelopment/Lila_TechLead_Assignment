// Place this file at Assets/Plugins/WebGL/TicTacToeSocketBridge.jslib.
// The page hosting the Unity WebGL build must load Socket.IO before Unity:
// <script src="http://localhost:3000/socket.io/socket.io.js"></script>

mergeInto(LibraryManager.library, {
  TTT_SocketConnect: function (serverUrlPointer, gameObjectPointer) {
    var serverUrl = UTF8ToString(serverUrlPointer);
    var gameObjectName = UTF8ToString(gameObjectPointer);
    var bridge = window.TicTacToeWebGLSocketBridge || {};

    if (bridge.socket) {
      bridge.socket.disconnect();
    }

    bridge.gameObjectName = gameObjectName;
    bridge.send = function (eventName, payload) {
      var envelope = JSON.stringify({ eventName: eventName, payload: payload || {} });
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
      bridge.send("socket-connected", { socketId: bridge.socket.id });
    });

    bridge.socket.on("disconnect", function (reason) {
      bridge.send("socket-disconnected", { reason: reason });
    });

    bridge.socket.on("connect_error", function (error) {
      bridge.send("socket-error", { message: error && error.message ? error.message : "Socket connection failed" });
    });

    [
      "room-created",
      "room-joined",
      "player-joined",
      "game-started",
      "move-result",
      "player-disconnected",
      "player-reconnected",
      "room-state",
      "error"
    ].forEach(function (eventName) {
      bridge.socket.on(eventName, function (payload) {
        bridge.send(eventName, payload);
      });
    });

    window.TicTacToeWebGLSocketBridge = bridge;
  },

  TTT_SocketEmit: function (eventNamePointer, payloadPointer) {
    var bridge = window.TicTacToeWebGLSocketBridge;
    var eventName = UTF8ToString(eventNamePointer);
    var payloadJson = UTF8ToString(payloadPointer);

    if (!bridge || !bridge.socket || !bridge.socket.connected) {
      if (bridge && bridge.send) {
        bridge.send("socket-error", { message: "Socket.IO is not connected" });
      }
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
