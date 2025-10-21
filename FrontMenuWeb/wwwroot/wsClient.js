
window.socketIO = {
    socket: null,

    connectSocketIO: async (url) => {
        console.log("🧠 Tentando conectar ao Socket.IO:", `https://syslogicadev.com/socket.io/`);

        const rawToken = localStorage.getItem("authToken");
        const token = rawToken ? rawToken.replaceAll('"', '') : null;

        const socket = io("https://syslogicadev.com", {
            path: "/socket.io/",
            transports: ["websocket"],
            query:
            {
                token: `${token}`
            }
        });
 
        socket.on("connect", () => {
            console.log("✅ Conectado ao servidor Socket.IO");
            DotNet.invokeMethodAsync("FrontMenuWeb", "ReceiveMessage", "Conectado ao servidor");
        });

       socket.emit("registrar-merchant");

        // Quando o servidor envia algo para o cliente
       socket.on("registrado", (msg) => {
            console.log("📩 Mensagem recebida do servidor Registrado:", msg);
            DotNet.invokeMethodAsync("FrontMenuWeb", "ReceiveMessage", msg);
        });

        // Quando o servidor envia algo para o cliente
      socket.on("pedido-recebido", (msg) => {
            console.log("📩 Mensagem recebida do servidor pedido-recebido:", msg);
            DotNet.invokeMethodAsync("FrontMenuWeb", "ReceiveMessage", msg);
        });

       socket.on("disconnect", () => {
            console.log("❌ Desconectado do servidor Socket.IO");
        });

    },

   
   
};
