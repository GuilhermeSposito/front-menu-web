
window.socketIO = {
    socket: null,

    connectSocketIO: async (url) => {
        console.log("🧠 Tentando conectar ao Socket.IO:", url);

        var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJNZXJjaGFudCI6eyJpZCI6IjYwOWIxMmU1LTczZTAtNGE1OC04ZmVkLWVlY2JkMWNiNGFkZiIsImVtYWlsIjoic29waG9zQGRldi5jb20uYnIiLCJyYXphb1NvY2lhbCI6IlNvcGhvcyBBcGxpY2F0aXZvcyBlIFRlY25vbG9naWEgTFREQSIsIkltYWdlbUxvZ28iOiJodHRwczovL2V4ZW1wbG8uY29tL2xvZ28ucG5nIiwiTm9tZUZhbnRhc2lhIjoiU29waG9zIEFwcHMiLCJlbmRlcmVjb3NfbWVyY2hhbnQiOltdLCJkb2N1bWVudG9zIjpbXSwidGVsZWZvbmVzIjpbXSwibWFyY2FEZXBhcnRhbWVudG8iOm51bGwsImxlZ2VuZGFEb1ZvbHVtYSI6bnVsbCwiYXRpdm8iOnRydWV9LCJpc0FkbWluIjpmYWxzZSwiaWF0IjoxNzYxMDQ5NzI1LCJleHAiOjE3NjEwODIxMjV9.gXqPwbzjvd5g8rgXFq1yHPxBdWgtUOVnb4wEtsOzSQk"

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
