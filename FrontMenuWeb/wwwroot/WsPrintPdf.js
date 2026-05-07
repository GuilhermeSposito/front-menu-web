
window.generatePdfFromHtml = async (elementId, filename) => {
    const element = document.getElementById(elementId);
    if (!element) {
        console.error("Elemento não encontrado: " + elementId);
        return;
    }

    // Coleta todos os blocos de QR Code individuais
    const qrPages = element.querySelectorAll('[id="qrcode"]');

    // Se não há blocos individuais, usa o comportamento original (fallback)
    if (qrPages.length === 0) {
        const clone = element.cloneNode(true);
        clone.style.display = "block";
        const tempContainer = document.createElement("div");
        tempContainer.style.position = "fixed";
        tempContainer.style.top = "0";
        tempContainer.style.left = "0";
        tempContainer.style.background = "#fff";
        tempContainer.style.zIndex = "-1";
        tempContainer.style.width = "210mm";
        tempContainer.appendChild(clone);
        document.body.appendChild(tempContainer);
        const opt = {
            margin: 2, filename: filename || 'documento.pdf',
            image: { type: 'jpeg', quality: 0.98 },
            html2canvas: { scale: 2, scrollY: 0, useCORS: true },
            jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' },
            pagebreak: { mode: ['css', 'legacy'] }
        };
        html2pdf().set(opt).from(clone).save().then(() => tempContainer.remove());
        return;
    }

    // Processa cada QR Code individualmente para evitar estouro do canvas do browser
    const { jsPDF } = window.jspdf;
    const pdf = new jsPDF({ unit: 'mm', format: 'a4', orientation: 'portrait' });
    const pdfWidth = pdf.internal.pageSize.getWidth();
    const pdfHeight = pdf.internal.pageSize.getHeight();

    let isFirst = true;
    for (const page of qrPages) {
        const clone = page.cloneNode(true);
        clone.style.display = "block";
        clone.style.visibility = "visible";

        const tempContainer = document.createElement("div");
        tempContainer.style.position = "fixed";
        tempContainer.style.top = "-99999px";
        tempContainer.style.left = "0";
        tempContainer.style.background = "#fff";
        tempContainer.style.width = "210mm";
        tempContainer.appendChild(clone);
        document.body.appendChild(tempContainer);

        const canvas = await html2canvas(clone, { scale: 2, useCORS: true, backgroundColor: '#ffffff' });
        document.body.removeChild(tempContainer);

        const imgData = canvas.toDataURL('image/jpeg', 0.95);
        const imgAspect = canvas.width / canvas.height;
        let imgW = pdfWidth - 20;
        let imgH = imgW / imgAspect;
        if (imgH > pdfHeight - 20) {
            imgH = pdfHeight - 20;
            imgW = imgH * imgAspect;
        }
        const x = (pdfWidth - imgW) / 2;
        const y = (pdfHeight - imgH) / 2;

        if (!isFirst) pdf.addPage();
        pdf.addImage(imgData, 'JPEG', x, y, imgW, imgH);
        isFirst = false;
    }

    pdf.save(filename || 'documento.pdf');
};


window.generatePdfFromLancamentos = async (htmlContent, filename) => {
    const element = document.createElement("div");
    element.innerHTML = htmlContent;

    const opt = {
        margin: 10,
        filename: filename || 'documento.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: { scale: 2 },
        jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' },
        pagebreak: { mode: ['css', 'legacy'] } // 👈 Importante p/ respeitar page-breaks no CSS
    };

    //html2pdf().set(opt).from(element).save(); //codigo para salvar automaticamente
    html2pdf().set(opt).from(element).output('dataurlnewwindow'); //codigo para abrir em nova aba
};

window.gerarQrCode = (elementId, text) => {
    const el = document.getElementById(elementId);
    el.innerHTML = "";
    QRCode.toCanvas(el, text, { width: 200 });
};