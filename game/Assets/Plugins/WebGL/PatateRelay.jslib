// Pont entre Unity et une WebSocket du navigateur, pour le relais en ligne
// (server/relay). Les messages reçus attendent dans une file que le jeu lit à
// chaque image (OnlineRelay.cs) : le navigateur n'appelle jamais Unity.
var PatateRelay = {
  $patateRelay: {
    socket: null, inbox: [], state: 2, reason: '',
    // Chaîne rendue à Unity, qui libère la mémoire.
    string: function (text) {
      var size = lengthBytesUTF8(text) + 1;
      var buffer = _malloc(size);
      stringToUTF8(text, buffer, size);
      return buffer;
    },
  },

  PatateRelayOpen: function (url) {
    var relay = patateRelay;
    if (relay.socket) { try { relay.socket.close(1000); } catch (e) {} }
    relay.socket = null;
    relay.inbox = [];
    relay.state = 0;
    relay.reason = '';
    var socket;
    try { socket = new WebSocket(UTF8ToString(url)); }
    catch (e) { relay.state = 2; relay.reason = 'adresse du relais invalide'; return; }
    relay.socket = socket;
    socket.onopen = function () { if (relay.socket === socket) relay.state = 1; };
    socket.onmessage = function (event) {
      if (relay.socket === socket && typeof event.data === 'string') relay.inbox.push(event.data);
    };
    socket.onclose = function (event) {
      if (relay.socket !== socket) return;
      relay.state = 2;
      relay.reason = event.reason || (event.code === 1006 ? 'relais injoignable' : 'connexion fermée (' + event.code + ')');
    };
  },

  PatateRelayState: function () { return patateRelay.state; },

  PatateRelaySend: function (text) {
    var socket = patateRelay.socket;
    if (socket && socket.readyState === 1) socket.send(UTF8ToString(text));
  },

  // Message suivant, ou null s'il n'y en a pas.
  PatateRelayReceive: function () {
    var relay = patateRelay;
    return relay.inbox.length === 0 ? 0 : relay.string(relay.inbox.shift());
  },

  PatateRelayReason: function () { return patateRelay.string(patateRelay.reason); },

  PatateRelayClose: function () {
    var relay = patateRelay;
    if (relay.socket) { try { relay.socket.close(1000); } catch (e) {} }
    relay.socket = null;
    relay.state = 2;
    if (!relay.reason) relay.reason = 'connexion fermée';
  },
};

autoAddDeps(PatateRelay, '$patateRelay');
mergeInto(LibraryManager.library, PatateRelay);
