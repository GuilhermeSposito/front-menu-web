window.SophosCharts = (function () {
    const _instances = {};

    function _destroy(id) {
        if (_instances[id]) {
            _instances[id].destroy();
            delete _instances[id];
        }
    }

    function _isDark() {
        return document.body.classList.contains('dark') ||
            document.documentElement.getAttribute('data-theme') === 'dark' ||
            window.matchMedia('(prefers-color-scheme: dark)').matches;
    }

    function _gridColor() { return _isDark() ? 'rgba(255,255,255,0.08)' : 'rgba(0,0,0,0.08)'; }
    function _textColor() { return _isDark() ? '#c9cdd4' : '#555'; }

    function renderBar(canvasId, labels, data, label, colorHex) {
        _destroy(canvasId);
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const color = colorHex || '#F88113';
        _instances[canvasId] = new Chart(canvas.getContext('2d'), {
            type: 'bar',
            data: {
                labels: labels,
                datasets: [{
                    label: label,
                    data: data,
                    backgroundColor: color + 'b3',
                    borderColor: color,
                    borderWidth: 1,
                    borderRadius: 4,
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: { mode: 'index', intersect: false }
                },
                scales: {
                    x: {
                        ticks: { color: _textColor(), font: { size: 11 } },
                        grid: { color: _gridColor() }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: { color: _textColor(), font: { size: 11 } },
                        grid: { color: _gridColor() }
                    }
                }
            }
        });
    }

    function renderLine(canvasId, labels, data, label, colorHex) {
        _destroy(canvasId);
        const canvas = document.getElementById(canvasId);
        if (!canvas) return;

        const color = colorHex || '#3dcb6c';
        _instances[canvasId] = new Chart(canvas.getContext('2d'), {
            type: 'line',
            data: {
                labels: labels,
                datasets: [{
                    label: label,
                    data: data,
                    backgroundColor: color + '33',
                    borderColor: color,
                    borderWidth: 2,
                    pointBackgroundColor: color,
                    pointRadius: 3,
                    fill: true,
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { display: false },
                    tooltip: { mode: 'index', intersect: false }
                },
                scales: {
                    x: {
                        ticks: { color: _textColor(), font: { size: 11 } },
                        grid: { color: _gridColor() }
                    },
                    y: {
                        beginAtZero: true,
                        ticks: { color: _textColor(), font: { size: 11 } },
                        grid: { color: _gridColor() }
                    }
                }
            }
        });
    }

    function destroy(canvasId) {
        _destroy(canvasId);
    }

    return { renderBar, renderLine, destroy };
})();
