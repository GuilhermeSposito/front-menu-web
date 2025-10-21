
window.generatePdfFromHtml = (elementId, filename) => {
    const element = document.getElementById(elementId);
    if (!element) {
        console.error("Elemento não encontrado: " + elementId);
        return;
    }

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


