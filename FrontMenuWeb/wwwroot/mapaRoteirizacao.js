window.mapaRoteirizacao = {
    mapa: null,
    markers: [],
    polyline: null,

    init: function (containerId, lat, lng) {
        const container = document.getElementById(containerId);
        if (!container) return;
        if (typeof google === 'undefined' || !google.maps) {
            console.warn('Google Maps SDK não carregado.');
            return;
        }

        this.mapa = new google.maps.Map(container, {
            center: { lat: lat, lng: lng },
            zoom: 13,
            mapTypeControl: false,
            streetViewControl: false,
            fullscreenControl: true
        });
        this.markers = [];
        this.polyline = null;
    },

    atualizarPontos: function (origem, pontos, voltarAoOrigem) {
        if (!this.mapa) return;

        this.markers.forEach(m => m.setMap(null));
        this.markers = [];

        if (this.polyline) {
            this.polyline.setMap(null);
            this.polyline = null;
        }

        this.markers.push(new google.maps.Marker({
            position: origem,
            map: this.mapa,
            label: { text: 'M', color: '#ffffff', fontWeight: 'bold' },
            title: 'Estabelecimento',
            icon: {
                path: google.maps.SymbolPath.CIRCLE,
                scale: 12,
                fillColor: '#d32f2f',
                fillOpacity: 1,
                strokeColor: '#ffffff',
                strokeWeight: 2
            }
        }));

        pontos.forEach((p, i) => {
            this.markers.push(new google.maps.Marker({
                position: { lat: p.lat, lng: p.lng },
                map: this.mapa,
                label: { text: String(i + 1), color: '#ffffff', fontWeight: 'bold' },
                title: p.cliente || ('Parada ' + (i + 1))
            }));
        });

        if (pontos.length > 0) {
            let path = [origem, ...pontos.map(p => ({ lat: p.lat, lng: p.lng }))];
            
            if (voltarAoOrigem === true || voltarAoOrigem === undefined) {
                path.push(origem);
            }

            this.polyline = new google.maps.Polyline({
                path: path,
                map: this.mapa,
                strokeColor: '#1976D2',
                strokeWeight: 3,
                strokeOpacity: 0.85,
                geodesic: true
            });

            const bounds = new google.maps.LatLngBounds();
            path.forEach(p => bounds.extend(p));
            this.mapa.fitBounds(bounds, 60);
        } else {
            this.mapa.setCenter(origem);
            this.mapa.setZoom(13);
        }
    }
};
