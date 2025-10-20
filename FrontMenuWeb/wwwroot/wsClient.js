// wsClient.js
window.socketIO = {
    socket: null,

    connectSocketIO: function (url) {
        console.log("🧠 Tentando conectar ao Socket.IO:", url);

        var token = localStorage.getItem("authToken");

        this.socket = io(url, {
            transports: ['websocket'],
            query: {
                token: token
            }
        });

        /*this.socket = io(url, {
            transports: ['websocket'],
            extraHeaders:
            {
                Authorization: `Bearer ${token}`
            }
        });
        */

        this.socket.on("connect", () => {
            console.log("✅ Conectado ao servidor Socket.IO");
            DotNet.invokeMethodAsync("FrontMenuWeb", "ReceiveMessage", "Conectado ao servidor");
        });

        this.socket.emit("registrar-merchant");

        // Quando o servidor envia algo para o cliente
        this.socket.on("registrado", (msg) => {
            console.log("📩 Mensagem recebida do servidor Registrado:", msg);
            DotNet.invokeMethodAsync("FrontMenuWeb", "ReceiveMessage", msg);
        });

        // Quando o servidor envia algo para o cliente
        this.socket.on("pedido-recebido", (msg) => {
            console.log("📩 Mensagem recebida do servidor pedido-recebido:", msg);
            DotNet.invokeMethodAsync("FrontMenuWeb", "ReceiveMessage", msg);
        });

        this.socket.on("disconnect", () => {
            console.log("❌ Desconectado do servidor Socket.IO");
        });

    },

    sendSocketMessage: function (event, message) {
        if (this.socket) {
            console.log("🚀 Enviando mensagem:", message);
            this.socket.emit(event, message);
        } else {
            console.warn("⚠️ Socket não está conectado.");
        }
    }
};
