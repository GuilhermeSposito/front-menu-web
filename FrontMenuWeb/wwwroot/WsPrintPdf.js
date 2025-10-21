
window.generatePdfFromHtml = (elementId, filename) => {
    const element = document.getElementById(elementId);
    if (!element) {
        console.error("Elemento não encontrado: " + elementId);
        return;
    }

    // 🔹 Clona o conteúdo e remove o posicionamento da página
    const clone = element.cloneNode(true);
    clone.style.display = "block";
    clone.style.margin = "0";
    clone.style.padding = "0";
    clone.style.position = "static";
    clone.style.transform = "none";
    clone.style.top = "0";
    clone.style.left = "0";

    // 🔹 Cria um container temporário fora da viewport
    const tempContainer = document.createElement("div");
    tempContainer.style.position = "fixed";
    tempContainer.style.top = "0";
    tempContainer.style.left = "0";
    tempContainer.style.background = "#fff";
    tempContainer.style.zIndex = "-1";
    tempContainer.style.width = "210mm"; // A4 width
    tempContainer.appendChild(clone);
    document.body.appendChild(tempContainer);

    const opt = {
        margin: 2,
        filename: filename || 'documento.pdf',
        image: { type: 'jpeg', quality: 0.98 },
        html2canvas: {
            scale: 2,
            scrollY: 0,
            useCORS: true
        },
        jsPDF: { unit: 'mm', format: 'a4', orientation: 'portrait' },
        pagebreak: { mode: ['css', 'legacy'] }
    };

    html2pdf().set(opt).from(clone).output('dataurlnewwindow').then(() => {
        tempContainer.remove(); // 🔹 Remove o container após gerar o PDF
    });


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

