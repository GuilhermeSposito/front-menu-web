window.balancaSerial = {
    port: null,
    reader: null,
    isConnected: false,

    // Tenta reconectar automaticamente a uma porta já autorizada
    reconectar: async function (baudRate) {
        try {
            if (!("serial" in navigator)) {
                return { sucesso: false, mensagem: "Web Serial API não suportada neste navegador." };
            }
            const portas = await navigator.serial.getPorts();
            if (portas.length === 0) {
                return { sucesso: false, mensagem: "Nenhuma porta autorizada encontrada. Conecte manualmente." };
            }
            this.port = portas[0];
            await this.port.open({ baudRate: baudRate || 9600 });
            this.isConnected = true;
            return { sucesso: true, mensagem: "Balança reconectada automaticamente." };
        } catch (erro) {
            if (erro.name === "InvalidStateError") {
                // Porta já está aberta
                this.isConnected = true;
                return { sucesso: true, mensagem: "Balança já estava conectada." };
            }
            this.isConnected = false;
            return { sucesso: false, mensagem: "Falha na reconexão automática: " + erro.message };
        }
    },

    conectar: async function (baudRate, forcarSelecao) {
        try {
            if (!("serial" in navigator)) {
                return { sucesso: false, mensagem: "Web Serial API não suportada neste navegador. Use Chrome ou Edge." };
            }

            // Se não forçar seleção, tenta reconectar automaticamente
            if (!forcarSelecao) {
                const recon = await this.reconectar(baudRate);
                if (recon.sucesso) {
                    return recon;
                }
            } else {
                // Desconecta a porta atual antes de selecionar outra
                await this.desconectar();
            }

            // Pede pro usuário selecionar a porta
            this.port = await navigator.serial.requestPort();
            await this.port.open({ baudRate: baudRate || 9600 });
            this.isConnected = true;

            return { sucesso: true, mensagem: "Balança conectada com sucesso." };
        } catch (erro) {
            this.isConnected = false;
            if (erro.name === "NotFoundError") {
                return { sucesso: false, mensagem: "Nenhuma porta foi selecionada." };
            }
            return { sucesso: false, mensagem: "Erro ao conectar: " + erro.message };
        }
    },

    lerPeso: async function () {
        if (!this.port || !this.isConnected) {
            return { sucesso: false, peso: 0, mensagem: "Balança não conectada." };
        }

        try {
            // Limpa o buffer serial antes de solicitar novo peso
            const flushReader = this.port.readable.getReader();
            try {
                while (true) {
                    const { value, done } = await Promise.race([
                        flushReader.read(),
                        new Promise(resolve => setTimeout(() => resolve({ value: null, done: true }), 200))
                    ]);
                    if (done || !value) break;
                }
            } catch (e) { }
            flushReader.releaseLock();

            const writer = this.port.writable.getWriter();
            const comando = new TextEncoder().encode("\x05");
            await writer.write(comando);
            writer.releaseLock();

            const reader = this.port.readable.getReader();
            let resposta = "";
            const timeout = 3000;
            const inicio = Date.now();

            while (Date.now() - inicio < timeout) {
                const { value, done } = await Promise.race([
                    reader.read(),
                    new Promise((_, reject) =>
                        setTimeout(() => reject(new Error("Timeout")), timeout - (Date.now() - inicio))
                    )
                ]);

                if (done) break;

                resposta += new TextDecoder().decode(value);

                // Para no primeiro frame completo (STX...ETX)
                if (resposta.includes("\x03") || resposta.includes("\r") || resposta.includes("\n")) {
                    break;
                }
            }

            // Pega apenas o primeiro frame STX...ETX se houver múltiplos
            const frameMatch = resposta.match(/\x02[^\x03]*\x03/);
            if (frameMatch) {
                resposta = frameMatch[0];
            }

            reader.releaseLock();

            const peso = this._extrairPeso(resposta);
            return { sucesso: true, peso: peso, mensagem: "Leitura realizada.", respostaBruta: resposta };
        } catch (erro) {
            return { sucesso: false, peso: 0, mensagem: "Erro ao ler peso: " + erro.message };
        }
    },

    _extrairPeso: function (resposta) {
        // Primeiro tenta formato com separador decimal (ex: "12.345" ou "12,345")
        const matchDecimal = resposta.match(/(\d+[.,]\d+)/);
        if (matchDecimal) {
            return parseFloat(matchDecimal[1].replace(",", "."));
        }

        // Protocolo Toledo/Prix: peso entre STX (\x02) e ETX (\x03), sem separador decimal
        // Ex: "\x0203020\x03" -> "03020" -> últimos 3 dígitos são decimais -> 03.020 kg
        const matchProtocolo = resposta.match(/\x02(\d+)\x03/);
        if (matchProtocolo) {
            const raw = matchProtocolo[1];
            if (raw.length >= 4) {
                const inteira = raw.slice(0, raw.length - 3);
                const decimal = raw.slice(raw.length - 3);
                return parseFloat((inteira || "0") + "." + decimal);
            }
            return parseFloat(raw);
        }

        // Fallback: tenta extrair qualquer sequência de dígitos
        const matchDigitos = resposta.match(/(\d{2,})/);
        if (matchDigitos) {
            const raw = matchDigitos[1];
            if (raw.length >= 4) {
                const inteira = raw.slice(0, raw.length - 3);
                const decimal = raw.slice(raw.length - 3);
                return parseFloat((inteira || "0") + "." + decimal);
            }
        }

        return 0;
    },

    desconectar: async function () {
        try {
            if (this.port) {
                await this.port.close();
                this.port = null;
                this.isConnected = false;
            }
            return { sucesso: true, mensagem: "Balança desconectada." };
        } catch (erro) {
            return { sucesso: false, mensagem: "Erro ao desconectar: " + erro.message };
        }
    },

    estaConectada: function () {
        return this.isConnected;
    }
};
