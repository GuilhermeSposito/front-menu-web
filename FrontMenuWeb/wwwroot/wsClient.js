//Função para conectar com o WS
window.socketIO = {
    socket: null,

    connectSocketIO: async (url) => {
        const rawToken = localStorage.getItem("authToken");
        const token = rawToken ? rawToken.replaceAll('"', '') : null;

        if (window.socketIO.socket && window.socketIO.socket.connected) {
            console.log("Socket já conectado.");
            return;
        }

        //https://sophos-erp.com.br
        const socket = io("https://sophos-erp.com.br", {
            path: "/socket.io/",
            transports: ["websocket"],
            withCredentials: true
        });

        window.socketIO.socket = socket;

        socket.on("connect", () => {

        });

        socket.emit("registrar-merchant");

        // Quando o servidor envia algo para o cliente
        socket.on("registrado", (msg) => {
        });

        // Quando o servidor envia algo para o cliente
        socket.on("pedido-recebido", (msg) => {
            // Avisando o Blazor
            if (window.DotNet) {
                DotNet.invokeMethodAsync("FrontMenuWeb", "ReceivePedido", JSON.stringify(msg))
                    .then(() => console.log(""))
                    .catch(err => console.error("Erro ao notificar Blazor:", err));
            }
        });

        // Quando o servidor envia algo para o cliente
        socket.on("pedido-recebido-mesa", (msg) => {
            // Avisando o Blazor
            if (window.DotNet) {
                DotNet.invokeMethodAsync("FrontMenuWeb", "ReceivePedidoMesa", JSON.stringify(msg))
                    .then(() => console.log(""))
                    .catch(err => console.error("Erro ao notificar Blazor:", err));
            }
        });

        socket.on("mesa-fechada", (msg) => {
            // Avisando o Blazor
            if (window.DotNet) {
                DotNet.invokeMethodAsync("FrontMenuWeb", "ReceivePedidoMesaFechada", JSON.stringify(msg))
                    .then(() => { })
                    .catch(err => console.error("Erro ao notificar Blazor:", err));
            }
        });

        //Quando Atualiza a etapa de um pedido
        socket.on("pedido-mudou-etapa", (msg) => {
            // Avisando o Blazor
            if (window.DotNet) {
                DotNet.invokeMethodAsync("FrontMenuWeb", "ReceiveEtapaDoPedido", JSON.stringify(msg))
                    .then(() => { })
                    .catch(err => console.error("Erro ao notificar Blazor:", err));
            }
        });

        socket.on("disconnect", () => {

        });

    },

   /* connectSocketIOMesa: async (url) => {
        const rawToken = localStorage.getItem("authToken");
        const token = rawToken ? rawToken.replaceAll('"', '') : null;

        if (!window.socketIO.socket) {
            await window.socketIO.connectSocketIO();
        }


        const socket = io("https://sophos-erp.com.br", {
            path: "/socket.io/",
            transports: ["websocket"],
            withCredentials: true
        });

        socket.on("connect", () => {

        });

        socket.emit("registrar-merchant");

        // Quando o servidor envia algo para o cliente
        socket.on("registrado", (msg) => {
        });

      

        socket.on("disconnect", () => {

        });

    },*/

   

};

//Função para reproduzir som de notificação
window.playNotificationSound = () => {
    const audio = new Audio('/sounds/notify.mp3');
    audio.play().catch(err => console.warn("Falha ao reproduzir som:", err));

    audio.addEventListener('ended', () => {
        audio.src = ''; // limpa referência
    });
};

//Função para reproduzir som de notificação da fila
window.playNotificationSoundFila = () => {
    const audio = new Audio('/sounds/SomDaFila.mp3');
    audio.play().catch(err => console.warn("Falha ao reproduzir som:", err));

    audio.addEventListener('ended', () => {
        audio.src = ''; // limpa referência
    });
};

//Função para baixar JSON do pedido
window.baixarJSON = (dados, nomeArquivo = "dados.json") => {
    const blob = new Blob([JSON.stringify(dados, null, 2)], { type: "application/json" });
    const url = URL.createObjectURL(blob);

    const a = document.createElement("a");
    a.href = url;
    a.download = nomeArquivo;
    a.click();

    URL.revokeObjectURL(url);
}

window.baixarXMLNF = (dados, nomeArquivo = "proc-nfe.xml") => {
    const blob = new Blob([dados], { type: "application/xml" });
    const url = URL.createObjectURL(blob);

    const a = document.createElement("a");
    a.href = url;
    a.download = nomeArquivo;
    a.click();

    URL.revokeObjectURL(url);
}

//Função para interceptar tecla F3
window.interceptF3 = function (dotnetHelper) {
    document.addEventListener('keydown', function (e) {
        if (e.key === 'F3') {
            e.preventDefault();
        }
    });
};

//função para bloquear todas as teclas de F na página de pagamento
window.interceptFunctionKeys = function (dotnetHelper) {
    document.addEventListener('keydown', function (e) {
        // Lista de teclas a bloquear
        const bloqueadas = ["F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10"];
        if (bloqueadas.includes(e.key)) {
            e.preventDefault(); // bloqueia comportamento padrão
        }
    });
};

//bloquear atalhos do navegador para a tela de pagamento
window.bloquearAtalhosDoNavegador = function () {
    document.addEventListener("keydown", function (e) {
        // Bloquear atalhos específicos do navegador
        if ((e.ctrlKey && ["d", "t", "e"].includes(e.key.toLowerCase()))) {
            e.preventDefault(); // bloqueia o comportamento do navegador
        }
    });
};


window.lockArrowScroll = () => {

    document.addEventListener('keydown', function (e) {
        const keys = [
            'ArrowUp',
            'ArrowDown',
            'ArrowLeft',
            'ArrowRight',
            'PageUp',
            'PageDown',
            'Home',
            'End',
            ' '
        ];

        if (keys.includes(e.key)) {
            e.preventDefault();
        }
    }, { passive: false });
};


window.copyToClipboard = function (text) {
    if (!text) return;

    navigator.clipboard.writeText(text)
        .then()
        .catch(err => console.error("Erro ao copiar", err));
};

window.selectAllInput = (id) => {
    setTimeout(() => {
        const el = document.getElementById(id);
        if (el) {
            el.select();
        }
    },0);
}

window.FocoNoProximoCampoDeEstoque = (name) => {
    setTimeout(() => {
        name = name + 1;
        const el = document.getElementsByClassName(name);
        if (el) {
            el.focus();
        }
    }, 0);
}