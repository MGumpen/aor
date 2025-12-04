var map;
var drawingMode = null;
var currentDrawing = [];
var drawingLayer = null;
var markers = [];
var polyline = null;
var pointCounter = 0;
var isPanelVisible = false;
var myLocationMarker = null;
var myLocationAccuracyCircle = null;
var myLocationWatchId = null;
var last30DaysLayer = null;
var showingLast30Days = false;

function closeWelcomePopup() {
    document.getElementById('welcome-popup').style.display = 'none';
    localStorage.setItem('aor_welcome_shown', 'true');
}

function showWelcomePopup() {
    const hasSeenPopup = localStorage.getItem('aor_welcome_shown');
    if (!hasSeenPopup) {
        document.getElementById('welcome-popup').style.display = 'flex';
    } else {
        document.getElementById('welcome-popup').style.display = 'none';
    }
}

function togglePointsPanel() {
    const pointsList = document.getElementById('points-list');
    const title = document.getElementById('panel-title');

    if (isPanelVisible) {
        pointsList.style.display = 'none';
        title.setAttribute('title', 'Click to show points');

        isPanelVisible = false;
    } else {
        pointsList.style.display = 'block';
        title.setAttribute('title', 'Click to hide points');

        isPanelVisible = true;
    }
}

document.addEventListener('DOMContentLoaded', function() {
    showWelcomePopup();
    setTimeout(initializeMap, 100);

    const last30DaysBtn = document.getElementById('last30DaysBtn');
    if (last30DaysBtn) {
        last30DaysBtn.style.display = 'inline-block';
    }
});

function initializeMap() {
    map = L.map('obstacle-map', {
        center: [63.4305, 10.3951], // midlertidig senter (Trondheim)
        zoom: 13,
        zoomControl: true,
        touchZoom: true,
        scrollWheelZoom: true,
        doubleClickZoom: true,
        tap: true,
        tapTolerance: 20
    });

    L.tileLayer('https://tile.openstreetmap.org/{z}/{x}/{y}.png', {
        maxZoom: 19,
        attribution: '© OpenStreetMap',
        className: 'map-tiles'
    }).addTo(map);

    map.zoomControl.setPosition('topright');

    setTimeout(function() {
        map.invalidateSize();
    }, 100);

    setupMapEvents();

    startLiveLocationTracking();
}

function setupMapEvents() {
    map.on('click', function(e) {
        if (drawingMode) {
            addPoint(e.latlng);
        }
    });
}

function startLiveLocationTracking() {
    if (!navigator.geolocation) {
        console.warn('Geolocation is not supported by this browser.');
        return;
    }

    const options = {
        enableHighAccuracy: true,
        maximumAge: 5000,
        timeout: 10000
    };

    var firstFix = true;

    myLocationWatchId = navigator.geolocation.watchPosition(
        function (position) {
            const lat = position.coords.latitude;
            const lng = position.coords.longitude;
            const accuracy = position.coords.accuracy || 0;
            const latlng = [lat, lng];

            // Opprett / oppdater blå sirkelmarkør
            if (!myLocationMarker) {
                myLocationMarker = L.circleMarker(latlng, {
                    radius: 8,
                    fillColor: '#2A66FF',
                    color: '#ffffff',
                    weight: 2,
                    opacity: 1,
                    fillOpacity: 0.9
                }).addTo(map);
            } else {
                myLocationMarker.setLatLng(latlng);
            }

            // Nøyaktighetssirkel rundt posisjonen
            if (!myLocationAccuracyCircle) {
                myLocationAccuracyCircle = L.circle(latlng, {
                    radius: accuracy,
                    color: '#2A66FF',
                    weight: 1,
                    opacity: 0.4,
                    fillColor: '#2A66FF',
                    fillOpacity: 0.1
                }).addTo(map);
            } else {
                myLocationAccuracyCircle.setLatLng(latlng);
                myLocationAccuracyCircle.setRadius(accuracy);
            }

            // Første fix: sentrer kartet på brukeren
            if (firstFix) {
                map.setView(latlng, 16);
                firstFix = false;
            }
        },
        function (error) {
            console.warn('Could not get live location:', error);
        },
        options
    );
}

function startDrawing(type) {
    if (drawingLayer) {
        map.removeLayer(drawingLayer);
        drawingLayer = null;
    }

    if (polyline) {
        polyline.remove();
        polyline = null;
    }

    currentDrawing = [];
    markers = [];
    pointCounter = 0;

    drawingMode = type;

    const titles = {
        'line': 'Line',
        'mast': 'Masts',
        'other': 'Obstacles'
    };
    document.getElementById('panel-title').textContent = titles[type];

    document.getElementById('drawing-notification').style.display = 'block';
    document.getElementById('points-panel').style.display = 'block';

    document.getElementById('points-list').style.display = 'block';

    isPanelVisible = true;
    document.getElementById('points-list').innerHTML = '';

    drawingLayer = L.layerGroup().addTo(map);
}

function addPoint(latlng) {
    if (!drawingMode) return;

    if ((drawingMode === 'mast' || drawingMode === 'other') && markers.length >= 1) {
        alert('Only one point can be added for Mast or Other.');
        return;
    }
    pointCounter++;
    const pointId = `point-${pointCounter}`;

    const colors = {
        'line': '#E74C3C',
        'mast': '#0C5AA6',
        'other': '#00A651'
    };

    const marker = L.circleMarker(latlng, {
        radius: 8,
        fillColor: colors[drawingMode],
        color: 'white',
        weight: 2,
        opacity: 1,
        fillOpacity: 1,
        draggable: false,
        pointId: pointId
    });

    const numberIcon = L.divIcon({
        className: 'marker-number-label',
        html: `<div style="
                background: ${colors[drawingMode]};
                border: 2px solid white;
                border-radius: 50%;
                width: 24px;
                height: 24px;
                display: flex;
                align-items: center;
                justify-content: center;
                font-weight: bold;
                font-size: 14px;
                color: white;
                box-shadow: 0 2px 4px rgba(0,0,0,0.3);
            ">${pointCounter}</div>`,
        iconSize: [24, 24],
        iconAnchor: [12, 12]
    });

    const numberMarker = L.marker(latlng, {
        icon: numberIcon,
        interactive: false,
        pointId: pointId + '_label'
    });

    numberMarker.addTo(drawingLayer);
    markers.push(numberMarker);

    let isDragging = false;

    marker.on('mousedown', function(e) {
        startDrag(e);
    });

    marker.on('touchstart', function(e) {
        startDrag(e);
    });

    function startDrag(e) {
        isDragging = true;
        map.dragging.disable();
        L.DomEvent.preventDefault(e.originalEvent);

        const onMove = function(e) {
            if (isDragging) {
                const newLatLng = e.latlng || map.mouseEventToLatLng(e.originalEvent.touches[0]);
                marker.setLatLng(newLatLng);

                const labelMarker = markers.find(m => m.options.pointId === pointId + '_label');
                if (labelMarker) {
                    labelMarker.setLatLng(newLatLng);
                }

                updateCurrentDrawing();
                updatePointInList(pointId, newLatLng);
                if (drawingMode === 'line') {
                    updatePolyline();
                }
            }
        };

        const onEnd = function(e) {
            isDragging = false;
            map.dragging.enable();
            map.off('mousemove', onMove);
            map.off('mouseup', onEnd);
            map.off('touchmove', onMove);
            map.off('touchend', onEnd);
        };

        map.on('mousemove', onMove);
        map.on('mouseup', onEnd);
        map.on('touchmove', onMove);
        map.on('touchend', onEnd);
    }

    marker.addTo(drawingLayer);
    markers.push(marker);
    currentDrawing.push(latlng);

    addPointToList(pointId, latlng, pointCounter);

    if (drawingMode === 'line') {
        updatePolyline();
    }
}

function addPointToList(pointId, latlng, number) {
    const pointsList = document.getElementById('points-list');
    const pointItem = document.createElement('div');
    pointItem.className = 'point-item';
    pointItem.id = pointId;

    const lat = latlng.lat.toFixed(5);
    const lng = latlng.lng.toFixed(5);

    let actionsHtml = '';
    if (drawingMode === 'line') {
        actionsHtml = `<button class="point-btn remove" onclick="removePoint('${pointId}')" title="Remove">✕</button>`;
    }

    pointItem.innerHTML = `
        <div class="point-header">
            <div class="point-number">${number}</div>
            <div class="point-actions">
                ${actionsHtml}
            </div>
        </div>
        <div class="point-coords">${lat}, ${lng}</div>
        `;

    pointsList.appendChild(pointItem);
}

function updatePointInList(pointId, latlng) {
    const pointItem = document.getElementById(pointId);
    if (pointItem) {
        const coordsDiv = pointItem.querySelector('.point-coords');
        const lat = latlng.lat.toFixed(5);
        const lng = latlng.lng.toFixed(5);
        coordsDiv.textContent = `${lat}, ${lng}`;
    }
}

function confirmPoint(pointId) {
    const pointItem = document.getElementById(pointId);
    if (pointItem) {
        pointItem.classList.add('confirmed');
    }
}

function removePoint(pointId) {
    const markerIndex = markers.findIndex(marker => marker.options.pointId === pointId);
    if (markerIndex !== -1) {
        drawingLayer.removeLayer(markers[markerIndex]);
        markers.splice(markerIndex, 1);
    }

    const labelIndex = markers.findIndex(marker => marker.options.pointId === pointId + '_label');
    if (labelIndex !== -1) {
        drawingLayer.removeLayer(markers[labelIndex]);
        markers.splice(labelIndex, 1);
    }

    const pointItem = document.getElementById(pointId);
    if (pointItem) {
        pointItem.remove();
    }

    updateCurrentDrawing();
    if (drawingMode === 'line') {
        updatePolyline();
    }
}

function undoLastPoint() {
    if (markers.length > 0) {
        const lastMarker = markers[markers.length - 1];
        const pointId = lastMarker.options.pointId;
        removePoint(pointId);
    }
}

function updateCurrentDrawing() {
    currentDrawing = markers
        .filter(marker => !marker.options.pointId.includes('_label'))
        .map(marker => marker.getLatLng());
}

function updatePolyline() {
    if (polyline) {
        drawingLayer.removeLayer(polyline);
    }

    if (currentDrawing.length > 1) {
        polyline = L.polyline(currentDrawing, {
            color: '#E74C3C',
            weight: 3,
            opacity: 0.8
        });
        polyline.addTo(drawingLayer);
    }
}

function confirmDrawing() {
    if ((drawingMode === 'line' && currentDrawing.length < 2) ||
        ((drawingMode === 'mast' || drawingMode === 'other') && currentDrawing.length < 1)) {
        alert(
            drawingMode === 'line' ? 'Please add at least two points for a Line.'
                : 'Please add a point.'
        );
        return;
    }

    const obstacleData = {
        type: drawingMode,
        coordinates: currentDrawing.map(latlng => ({
            lat: latlng.lat,
            lng: latlng.lng
        })),
        timestamp: new Date().toISOString()
    };

    console.log('Obstacle data to register:', obstacleData);

    const params = new URLSearchParams({
        type: drawingMode,
        coordinates: JSON.stringify(obstacleData.coordinates),
        count: currentDrawing.length
    });

    window.location.href = `/Obstacle/DataForm?${params.toString()}`;
}

function cancelDrawing() {
    if (drawingLayer) {
        map.removeLayer(drawingLayer);
    }

    drawingMode = null;
    currentDrawing = [];
    markers = [];
    polyline = null;
    drawingLayer = null;
    pointCounter = 0;

    document.getElementById('drawing-notification').style.display = 'none';
    document.getElementById('points-panel').style.display = 'none';

    isPanelVisible = false;
}

function showMyLocation() {
    // My Location-knappen: hopp til live-marker hvis den finnes
    if (myLocationMarker) {
        map.setView(myLocationMarker.getLatLng(), 16);
        return;
    }

    // Fallback: enkel posisjons-henting hvis live-tracking ikke har startet enda
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(
            function(position) {
                const lat = position.coords.latitude;
                const lng = position.coords.longitude;
                const latlng = [lat, lng];

                if (!myLocationMarker) {
                    myLocationMarker = L.circleMarker(latlng, {
                        radius: 8,
                        fillColor: '#2A66FF',
                        color: '#ffffff',
                        weight: 2,
                        opacity: 1,
                        fillOpacity: 0.9
                    }).addTo(map);
                } else {
                    myLocationMarker.setLatLng(latlng);
                }

                map.setView(latlng, 16);
            },
            function(error) {
                alert('Could not get your location. Please ensure location services are enabled.');
            }
        );
    }
}

window.addEventListener('resize', function() {
    if (map) {
        setTimeout(function() {
            map.invalidateSize();
        }, 100);
    }
});

async function toggleLast30Days() {
    const btn = document.getElementById('last30DaysBtn');

    if (showingLast30Days) {
        if (last30DaysLayer) {
            map.removeLayer(last30DaysLayer);
            last30DaysLayer = null;
        }
        showingLast30Days = false;
        btn.classList.remove('active');
        btn.innerHTML = '<i class="fa fa-calendar-days me-2"></i>Last 30 Days';
    } else {
        try {
            btn.disabled = true;
            btn.innerHTML = '<i class="fa fa-spinner fa-spin me-2"></i>Loading...';

            const response = await fetch('/Obstacle/Last30Days');
            if (!response.ok) {
                throw new Error('Failed to fetch obstacles');
            }

            const obstacles = await response.json();
            displayLast30DaysObstacles(obstacles);

            showingLast30Days = true;
            btn.classList.add('active');
            btn.innerHTML = '<i class="fa fa-calendar-check me-2"></i>Hide Last 30 Days';
        } catch (error) {
            console.error('Error loading obstacles:', error);
            alert('Failed to load obstacles from last 30 days');
            btn.innerHTML = '<i class="fa fa-calendar-days me-2"></i>Last 30 Days';
        } finally {
            btn.disabled = false;
        }
    }
}

function displayLast30DaysObstacles(obstacles) {
    if (last30DaysLayer) {
        map.removeLayer(last30DaysLayer);
    }

    last30DaysLayer = L.layerGroup().addTo(map);

    if (obstacles.length === 0) {
        alert('No obstacles registered in the last 30 days');
        return;
    }

    const colors = {
        'line': '#E74C3C',
        'mast': '#0C5AA6',
        'other': '#00A651'
    };

    obstacles.forEach(obstacle => {
        try {
            const coords = JSON.parse(obstacle.coordinates);
            const color = colors[obstacle.obstacleType.toLowerCase()] || '#4f46e5';

            if (coords.length === 1) {
                // Single point - create a marker
                const marker = L.circleMarker([coords[0].lat, coords[0].lng], {
                    radius: 10,
                    fillColor: color,
                    color: '#fff',
                    weight: 3,
                    opacity: 1,
                    fillOpacity: 0.8
                });

                marker.bindPopup(`
                        <strong>${obstacle.obstacleName}</strong><br/>
                        <em>${obstacle.obstacleType}</em><br/>
                        Registered: ${new Date(obstacle.createdAt).toLocaleDateString()}<br/>
                        <a href="/Report/ReportDetails/${obstacle.reportId}">View Details</a>
                    `);

                marker.addTo(last30DaysLayer);
            } else {
                const latlngs = coords.map(c => [c.lat, c.lng]);
                const polyline = L.polyline(latlngs, {
                    color: color,
                    weight: 4,
                    opacity: 0.8
                });

                polyline.bindPopup(`
                        <strong>${obstacle.obstacleName}</strong><br/>
                        <em>${obstacle.obstacleType}</em><br/>
                        Points: ${coords.length}<br/>
                        Registered: ${new Date(obstacle.createdAt).toLocaleDateString()}<br/>
                        <a href="/Report/ReportDetails/${obstacle.reportId}">View Details</a>
                    `);

                polyline.addTo(last30DaysLayer);
            }
        } catch (error) {
            console.error('Error parsing obstacle coordinates:', error, obstacle);
        }
    });
}