//Função para conectar com o WS
window.socketIO = {
    socket: null,

    disconnectSocketIO: () => {
        if (window.socketIO.socket) {
            window.socketIO.socket.off();
            window.socketIO.socket.disconnect();
            window.socketIO.socket = null;
        }
    },

    connectSocketIO: async (url) => {
        try {
            if (window.socketIO.socket) {
                return;
            }

            if (url == "" || url == null || !url)
                url = "https://sophos-erp.com.br";

            const socket = io(url, {
                path: "/socket.io/",
                transports: ["websocket"],
                withCredentials: true
            });

            window.socketIO.socket = socket;

            socket.on("connect", () => {
                socket.emit("registrar-merchant");
            });


            socket.on("registrado", (msg) => {
            });

            socket.off("pedido-recebido");
            socket.on("pedido-recebido", (msg) => {
                if (window.DotNet) {
                    DotNet.invokeMethodAsync("FrontMenuWeb", "ReceivePedido", JSON.stringify(msg))
                        .catch(err => console.error("Erro ao notificar Blazor:", err));
                }
            });


            socket.off("pedido-recebido-mesa");
            socket.on("pedido-recebido-mesa", (msg) => {
                // Avisando o Blazor
                if (window.DotNet) {
                    DotNet.invokeMethodAsync("FrontMenuWeb", "ReceivePedidoMesa", JSON.stringify(msg))
                        .catch(err => console.error("Erro ao notificar Blazor:", err));
                }
            });

            socket.off("mesa-fechada");
            socket.on("mesa-fechada", (msg) => {
                // Avisando o Blazor
                if (window.DotNet) {
                    DotNet.invokeMethodAsync("FrontMenuWeb", "ReceivePedidoMesaFechada", JSON.stringify(msg))
                        .catch(err => console.error("Erro ao notificar Blazor:", err));
                }
            });

            socket.off("pedido-mudou-etapa")
            socket.on("pedido-mudou-etapa", (msg) => {
                // Avisando o Blazor
                if (window.DotNet) {
                    DotNet.invokeMethodAsync("FrontMenuWeb", "ReceiveEtapaDoPedido", JSON.stringify(msg))
                        .catch(err => console.error("Erro ao notificar Blazor:", err));
                }
            });

            socket.off("pedido-mudou-info")
            socket.on("pedido-mudou-info", (msg) => {
                // Avisando o Blazor
                if (window.DotNet) {
                    DotNet.invokeMethodAsync("FrontMenuWeb", "ReceiveInfoAdicionalDoPedido", JSON.stringify(msg))
                        .catch(err => console.error("Erro ao notificar Blazor:", err));
                }
            });

           
        } catch (e) {
            console.error("Erro ao conectar Socket.IO:", e);
        }
    },

  

};

//Função para reproduzir som de notificação
window.playNotificationSound = () => {
    const audio = new Audio('/sounds/notify.mp3');
    audio.play().catch(err => console.warn("Falha ao reproduzir som:", err));

    audio.addEventListener('ended', () => {
        audio.src = ''; // limpa referência
    });
};

window.playNotificationSoundPedidoIntegracao = () => {
    const audio = new Audio('/sounds/notifyPedidoIntegracao.mp3');
    audio.play().catch(err => console.warn("Falha ao reproduzir som:", err));

    audio.addEventListener('ended', () => {
        audio.src = ''; // limpa referência
    });
};

window.playNotificationSoundPedidoIfoodstatusNovo = () => {
    const audio = new Audio('/sounds/toqueifood.mp3');
    audio.play().catch(err => console.warn("Falha ao reproduzir som:", err));

    audio.addEventListener('ended', () => {
        audio.src = ''; // limpa referência
    });
};

//Função para reproduzir som de notificação da fila
window.playNotificationSoundFila = () => {
    const audio = new Audio('/sounds/dingdong.mp3');
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
    }, 0);
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


window.startIfoodWidget = async function (merchantIds) {
    try {

        if (!merchantIds) {
            return { success: false, message: "MerchantId inválido" };
        }

        if (typeof iFoodWidget === "undefined") {
            return { success: false, message: "iFoodWidget não carregado" };
        }

      
        iFoodWidget.init({
            widgetId: 'a1ff7bc4-b660-44fb-af64-afc98b4ce06e',
            merchantIds: merchantIds,
        });

        return { success: true, message: "Widget iniciado com sucesso" };
    }
    catch (e) {
        console.error(e);
        return { success: false, message: e.message };
    }
};