document.addEventListener('DOMContentLoaded', function () {
    const coordField = document.getElementById('reportCoordinates');
    const typeField = document.getElementById('reportObstacleType');
    const coordinatesDisplay = document.getElementById('coordinates-display');
    const mapWrapper = document.getElementById('map-wrapper');

    let coordinatesJson = coordField?.value?.trim() || '[]';
    let obstacleType = (typeField?.value || '').toLowerCase();

    let coordinates = [];
    if (coordinatesJson && coordinatesJson !== '[]') {
        try {
            coordinates = JSON.parse(coordinatesJson);
        } catch (e) {
            console.warn('Feil ved parsing av koordinater', e);
            if (coordinatesDisplay) {
                coordinatesDisplay.innerHTML = '<div class="coordinate-item">Error parsing coordinates</div>';
            }
            coordinates = [];
        }
    }

    if (coordinatesDisplay) {
        if (!coordinates.length) {
            coordinatesDisplay.innerHTML = '<div class="coordinate-item">No coordinates</div>';
        } else {
            coordinatesDisplay.innerHTML = coordinates.map((c, i) => {
                const lat = (c.lat ?? c.latitude ?? c.Latitude);
                const lng = (c.lng ?? c.longitude ?? c.Longitude);
                const latStr = typeof lat === 'number' ? lat.toFixed(6) : 'N/A';
                const lngStr = typeof lng === 'number' ? lng.toFixed(6) : 'N/A';
                return `<div class="coordinate-item">Point ${i + 1}: ${latStr}, ${lngStr}</div>`;
            }).join('');
        }
    }

    if (!mapWrapper || coordinates.length === 0) {
        return; // Ingenting å vise på kartet
    }

    // Init Leaflet-kart
    const map = L.map('report-map');
    window.reportMap = map;

    L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '&copy; OpenStreetMap contributors',
        className: 'map-tiles'
    }).addTo(map);

    const latLngs = coordinates.map(c => {
        const lat = (c.lat ?? c.latitude ?? c.Latitude);
        const lng = (c.lng ?? c.longitude ?? c.Longitude);
        return L.latLng(lat, lng);
    }).filter(p => p.lat && p.lng);

    if (latLngs.length === 0) {
        return;
    }

    const isLineType = obstacleType === 'line' || latLngs.length > 1;

    if (isLineType) {
        L.polyline(latLngs, { weight: 4 }).addTo(map);
        latLngs.forEach(p => L.marker(p).addTo(map));
        map.fitBounds(L.latLngBounds(latLngs), { padding: [20, 20] });
    } else {
        const point = latLngs[0];
        L.marker(point).addTo(map);
        map.setView(point, 15);
    }
});