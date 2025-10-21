
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
           // DotNet.invokeMethodAsync("FrontMenuWeb", "ReceiveMessage", "Conectado ao servidor");
        });

       socket.emit("registrar-merchant");

        // Quando o servidor envia algo para o cliente
       socket.on("registrado", (msg) => {
            console.log("📩 Mensagem recebida do servidor Registrado:", msg);
        });

        // Quando o servidor envia algo para o cliente
      socket.on("pedido-recebido", (msg) => {
          // Avisando o Blazor
          if (window.DotNet) {
              DotNet.invokeMethodAsync("FrontMenuWeb", "ReceivePedido", JSON.stringify(msg))
                  .then(() => console.log("Blazor foi notificado!"))
                  .catch(err => console.error("Erro ao notificar Blazor:", err));
          }
        });

       socket.on("disconnect", () => {
            console.log("❌ Desconectado do servidor Socket.IO");
        });

    },
   
};


window.playNotificationSound = () => {
    const audio = new Audio('/sounds/notify.mp3');
    audio.play().catch(err => console.warn("Falha ao reproduzir som:", err));

    audio.addEventListener('ended', () => {
        audio.src = ''; // limpa referência
    });
};