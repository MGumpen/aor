// FIX: Fjernet ugyldig Razor i statisk fil og leser koordinater fra skjult input
let initialCoordinatesJson = '[]';
let currentUserId = '';
let heightInputElement;
let conversionDisplayElement;
let hiddenHeightElement;
let containerResizeTimeout;
let formNotification;

document.addEventListener('DOMContentLoaded', function () {
    // Hent koordinater fra hidden felt (generert i DataForm.cshtml)
    const hiddenCoordField = document.getElementById('hiddenCoordinates');
    if (hiddenCoordField && hiddenCoordField.value) {
        initialCoordinatesJson = hiddenCoordField.value.trim();
    }
    // Hent bruker-id hvis tilgjengelig (kan legges inn som <meta name="current-user-id" content="..."> eller hidden input)
    const metaUser = document.querySelector('meta[name="current-user-id"]');
    if (metaUser) {
        currentUserId = metaUser.getAttribute('content') || '';
    } else {
        const hiddenUserField = document.getElementById('currentUserId');
        if (hiddenUserField) {
            currentUserId = hiddenUserField.value.trim();
        }
    }

    heightInputElement = document.getElementById('heightInput');
    conversionDisplayElement = document.getElementById('conversionDisplay');
    hiddenHeightElement = document.getElementById('hiddenHeight');
    formNotification = document.getElementById('formNotification') || document.getElementById('globalToast');

    try {
        if (window.sessionStorage) {
            window.sessionStorage.removeItem('aor_form_notification');
        }
    } catch (error) {
        console.warn('Unable to reset pending notification state', error);
    }

    if (heightInputElement) {
        heightInputElement.addEventListener('input', updateHeightConversion);
        updateHeightConversion();
    }

    // Nå rendres koordinatene korrekt
    renderCoordinatesFromJson(initialCoordinatesJson);
    updateMapSummaryFromData({
        ObstacleType: document.getElementById('hiddenObstacleType')?.value,
        PointCount: document.getElementById('hiddenPointCount')?.value,
        Coordinates: initialCoordinatesJson
    });

    const form = document.querySelector('form');
    if (form) {
        form.addEventListener('submit', function () {
            updateHeightConversion();

            showFormNotification('Report registered!');

            try {
                if (window.sessionStorage) {
                    window.sessionStorage.setItem('aor_form_notification', 'Report registered!');
                }
            } catch (error) {
                console.warn('Unable to persist submission notification', error);
            }

            const urlParams = new URLSearchParams(window.location.search);
            const draftKey = urlParams.get('draft');

            if (draftKey) {
                sessionStorage.setItem('deleteDraft', draftKey);
            }
        });
    }

    loadDraft();
});

function sanitizeKeySegment(value) {
    return (value || '')
        .toString()
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-+|-+$/g, '');
}

function showFormNotification(message) {
    if (!formNotification) {
        return;
    }

    if (typeof message === 'string' && message.trim()) {
        formNotification.textContent = message;
    }

    if (formNotification.hideTimeout) {
        clearTimeout(formNotification.hideTimeout);
    }

    formNotification.classList.add('show');

    formNotification.hideTimeout = setTimeout(() => {
        formNotification.classList.remove('show');
        formNotification.hideTimeout = undefined;
    }, 3000);
}

function showDraftNotification(message) {
    const notification = document.getElementById('draftNotification');
    if (!notification) {
        return;
    }

    if (typeof message === 'string' && message.trim()) {
        notification.textContent = message;
    }

    if (notification.hideTimeout) {
        clearTimeout(notification.hideTimeout);
    }

    notification.classList.add('show');

    notification.hideTimeout = setTimeout(() => {
        notification.classList.remove('show');
        notification.hideTimeout = undefined;
    }, 3000);
}

function updateHeightConversion() {
    if (!heightInputElement || !conversionDisplayElement || !hiddenHeightElement) {
        return;
    }

    const inputValue = parseFloat(heightInputElement.value);
    if (!isNaN(inputValue) && inputValue > 0) {
        const maxFeet = 3280;
        if (inputValue > maxFeet) {
            conversionDisplayElement.textContent = 'Max 1000 m (≈ 3280 ft)';
            hiddenHeightElement.value = '';
            return;
        }

        const meters = inputValue * 0.3048;
        if (meters > 1000) {
            conversionDisplayElement.textContent = 'Max 1000 m (≈ 3280 ft)';
            hiddenHeightElement.value = '';
            return;
        }
        conversionDisplayElement.textContent = `≈ ${meters.toFixed(1)} m`;
        hiddenHeightElement.value = meters;
    } else {
        conversionDisplayElement.textContent = '≈ 0.0 m';
        hiddenHeightElement.value = '';
    }
}

function renderCoordinatesFromJson(json) {
    const display = document.getElementById('coordinates-display');
    if (!display) {
        return;
    }

    if (!json || json === '[]') {
        display.innerHTML = '<div class="coordinate-item">No coordinates registered yet.</div>';
        return;
    }

    let parsed;
    if (typeof json === 'string') {
        try {
            parsed = JSON.parse(json);
        } catch (error) {
            console.warn('Unable to parse coordinates', error);
            display.innerHTML = '<div class="coordinate-item">Error parsing coordinates</div>';
            return;
        }
    } else {
        parsed = json;
    }

    if (!Array.isArray(parsed) || !parsed.length) {
        display.innerHTML = '<div class="coordinate-item">No coordinates registered yet.</div>';
        return;
    }

    display.innerHTML = parsed.map((coord, index) => {
        const lat = typeof coord.lat === 'number' ? coord.lat.toFixed(6) : 'N/A';
        const lng = typeof coord.lng === 'number' ? coord.lng.toFixed(6) : 'N/A';
        return `<div class="coordinate-item">Point ${index + 1}: ${lat}, ${lng}</div>`;
    }).join('');
}

function derivePointCount(data) {
    if (!data) {
        return 0;
    }

    const countFromField = Number(data.PointCount);
    if (!Number.isNaN(countFromField) && countFromField > 0) {
        return countFromField;
    }

    if (data.Coordinates) {
        try {
            const coordinates = typeof data.Coordinates === 'string'
                ? JSON.parse(data.Coordinates)
                : data.Coordinates;

            if (Array.isArray(coordinates)) {
                return coordinates.length;
            }
        } catch (error) {
            console.warn('Unable to parse coordinate data for point count', error);
        }
    }

    return 0;
}

function setFieldValueByName(name, value) {
    if (value === undefined || value === null) {
        return;
    }

    const candidates = Array.from(document.getElementsByName(name));
    const field = candidates.length > 0 ? candidates[0] : document.getElementById(name);

    if (!field) {
        return;
    }

    if (field.type === 'checkbox') {
        const normalized = typeof value === 'string' ? value.trim().toLowerCase() : value;
        field.checked = normalized === true || normalized === 'true' || normalized === '1';
        return;
    }

    if (field.type === 'radio') {
        const match = candidates.find(radio => radio.value === value.toString());
        if (match) {
            match.checked = true;
        }
        return;
    }

    const normalizedValue = typeof value === 'string' ? value : value.toString();
    field.value = normalizedValue;

    if (field.type !== 'hidden') {
        field.dispatchEvent(new Event('input', { bubbles: true }));
        field.dispatchEvent(new Event('change', { bubbles: true }));
    }
}

function applyDraftToForm(data) {
    setFieldValueByName('ObstacleName', data.ObstacleName);
    setFieldValueByName('ObstacleDescription', data.ObstacleDescription);
    setFieldValueByName('heightInput', data.heightInput);
    setFieldValueByName('ObstacleHeight', data.ObstacleHeight);
    setFieldValueByName('WireCount', data.WireCount);
    setFieldValueByName('MastType', data.MastType);
    if (data.HasLighting !== undefined) {
        setFieldValueByName('HasLighting', data.HasLighting);
    }
    setFieldValueByName('Category', data.Category);

    if (heightInputElement && typeof data.heightInput !== 'undefined') {
        heightInputElement.value = data.heightInput;
    }

    if (hiddenHeightElement && typeof data.ObstacleHeight !== 'undefined') {
        hiddenHeightElement.value = data.ObstacleHeight;
    }

    const hiddenCoordinatesField = document.getElementById('hiddenCoordinates');
    if (hiddenCoordinatesField && typeof data.Coordinates !== 'undefined') {
        hiddenCoordinatesField.value = typeof data.Coordinates === 'string'
            ? data.Coordinates
            : JSON.stringify(data.Coordinates ?? []);
    }

    const hiddenPointCountField = document.getElementById('hiddenPointCount');
    if (hiddenPointCountField && typeof data.PointCount !== 'undefined') {
        hiddenPointCountField.value = data.PointCount;
    }

    const hiddenTypeField = document.getElementById('hiddenObstacleType');
    if (hiddenTypeField && typeof data.ObstacleType !== 'undefined') {
        hiddenTypeField.value = data.ObstacleType.toString();
    }

    if (data.Coordinates) {
        renderCoordinatesFromJson(data.Coordinates);
    }

    updateHeightConversion();
    updateMapSummaryFromData(data);
}

function canonicalizeDraftData(source = {}) {
    const data = { ...source };

    const getFirstValue = (...keys) => {
        for (const key of keys) {
            if (Object.prototype.hasOwnProperty.call(data, key)) {
                const value = data[key];
                if (value !== undefined && value !== null) {
                    if (typeof value === 'string') {
                        const trimmed = value.trim();
                        if (trimmed !== '') {
                            return trimmed;
                        }
                    } else if (!(typeof value === 'number' && Number.isNaN(value))) {
                        return value;
                    }
                }
            }
        }

        return undefined;
    };

    const setCanonicalField = (canonicalKey, ...aliases) => {
        const value = getFirstValue(canonicalKey, ...aliases);
        if (value !== undefined) {
            data[canonicalKey] = value;
        }
    };

    setCanonicalField('ObstacleName', 'obstacleName', 'name', 'ObstacleId', 'obstacleId', 'draftTitle', 'title', 'displayName', 'ObstacleData.ObstacleName', 'obstacle_data.obstacle_name');
    setCanonicalField('ObstacleType', 'obstacleType', 'type', 'ObstacleData.ObstacleType', 'obstacle_data.obstacle_type', 'hiddenObstacleType');
    setCanonicalField('ObstacleDescription', 'obstacleDescription', 'description', 'ObstacleData.ObstacleDescription', 'obstacle_data.obstacle_description');
    setCanonicalField('ObstacleHeight', 'obstacleHeight', 'heightMeters', 'height', 'Height', 'ObstacleData.ObstacleHeight', 'obstacle_data.obstacle_height');
    setCanonicalField('heightInput', 'heightFeet', 'HeightFeet', 'height_ft', 'feetHeight', 'ObstacleData.heightInput', 'obstacle_data.height_input');
    setCanonicalField('WireCount', 'wireCount', 'wires', 'wire_count', 'numberOfWires', 'wireTotal', 'ObstacleData.WireCount', 'obstacle_data.wire_count');
    setCanonicalField('MastType', 'mastType', 'towerType', 'mast_type', 'tower_type', 'ObstacleData.MastType', 'obstacle_data.mast_type');
    setCanonicalField('HasLighting', 'hasLighting', 'lighting', 'Lighting', 'has_lighting', 'ObstacleData.HasLighting', 'obstacle_data.has_lighting');
    setCanonicalField('Category', 'category', 'obstacleCategory', 'obstacle_category', 'ObstacleData.Category', 'obstacle_data.category');
    setCanonicalField('Coordinates', 'coordinates', 'pointsJson', 'ObstacleData.Coordinates', 'obstacle_data.coordinates', 'coordinatesJson');
    setCanonicalField('PointCount', 'pointCount', 'points', 'ObstacleData.PointCount', 'obstacle_data.point_count');
    setCanonicalField('savedAt', 'SavedAt', 'saved_at', 'ObstacleData.SavedAt', 'obstacle_data.saved_at', 'timestamp');
    setCanonicalField('ownerId', 'OwnerId');

    if (data.ObstacleType) {
        data.ObstacleType = data.ObstacleType.toString().trim();
    }

    if (typeof data.HasLighting === 'boolean') {
        data.HasLighting = data.HasLighting ? 'true' : 'false';
    } else if (typeof data.HasLighting === 'string') {
        const trimmed = data.HasLighting.trim().toLowerCase();
        if (trimmed === 'yes') {
            data.HasLighting = 'true';
        } else if (trimmed === 'no') {
            data.HasLighting = 'false';
        } else {
            data.HasLighting = data.HasLighting.trim();
        }
    }

    if (data.Coordinates && typeof data.Coordinates !== 'string') {
        try {
            data.Coordinates = JSON.stringify(data.Coordinates);
        } catch (error) {
            console.warn('Unable to stringify coordinates during canonicalization', error);
            data.Coordinates = '[]';
        }
    }

    const parsedHeight = Number(data.ObstacleHeight);
    if (!Number.isNaN(parsedHeight) && parsedHeight > 0) {
        data.ObstacleHeight = parsedHeight;
    }

    if ((!data.heightInput || data.heightInput === '') && parsedHeight > 0) {
        const feet = parsedHeight / 0.3048;
        data.heightInput = (Math.round(feet * 10) / 10).toString();
    }

    data.PointCount = derivePointCount(data);

    ['ObstacleName', 'ObstacleDescription', 'MastType', 'Category', 'WireCount', 'heightInput', 'draftTitle', 'displayName'].forEach(key => {
        if (typeof data[key] === 'string') {
            data[key] = data[key].trim();
        }
    });

    if (typeof data.ownerId === 'string') {
        data.ownerId = data.ownerId.trim();
    }

    if (!data.draftTitle && data.ObstacleName) {
        data.draftTitle = data.ObstacleName;
    }

    if (!data.displayName && data.ObstacleName) {
        data.displayName = data.ObstacleName;
    }

    return data;
}

function buildDraftData(options = {}) {
    const form = document.querySelector('form');
    if (!form) {
        return null;
    }

    updateHeightConversion();

    const formData = new FormData(form);
    const draftData = {};

    formData.forEach((value, key) => {
        if (key === '__RequestVerificationToken') {
            return;
        }
        draftData[key] = value;
    });

    const canonicalDraft = canonicalizeDraftData(draftData);

    const nameField = document.getElementById('ObstacleName');
    if (!canonicalDraft.ObstacleName && nameField && nameField.value.trim() !== '') {
        canonicalDraft.ObstacleName = nameField.value.trim();
    }

    if (canonicalDraft.ObstacleName) {
        const trimmedName = canonicalDraft.ObstacleName.toString().trim();
        canonicalDraft.ObstacleName = trimmedName;
        if (!canonicalDraft.draftTitle) {
            canonicalDraft.draftTitle = trimmedName;
        }
        if (!canonicalDraft.displayName) {
            canonicalDraft.displayName = trimmedName;
        }
    }

    if (!canonicalDraft.ObstacleDescription) {
        const descriptionField = document.getElementById('ObstacleDescription');
        if (descriptionField && descriptionField.value.trim() !== '') {
            canonicalDraft.ObstacleDescription = descriptionField.value.trim();
        }
    }

    if (!canonicalDraft.ObstacleType) {
        const typeField = document.getElementById('hiddenObstacleType');
        if (typeField && typeField.value) {
            canonicalDraft.ObstacleType = typeField.value;
        }
    }

    if (canonicalDraft.ObstacleType) {
        canonicalDraft.ObstacleType = canonicalDraft.ObstacleType.toString().trim();
    }

    const coordinatesField = document.getElementById('hiddenCoordinates');
    if (coordinatesField && coordinatesField.value) {
        canonicalDraft.Coordinates = coordinatesField.value;
    }

    if (hiddenHeightElement && hiddenHeightElement.value) {
        const meters = Number(hiddenHeightElement.value);
        if (!Number.isNaN(meters) && meters > 0) {
            canonicalDraft.ObstacleHeight = meters;
        }
    }

    if (heightInputElement) {
        canonicalDraft.heightInput = heightInputElement.value || canonicalDraft.heightInput || '';
    }

    const mastTypeField = document.querySelector('[name="MastType"]');
    if (!canonicalDraft.MastType && mastTypeField && mastTypeField.value) {
        canonicalDraft.MastType = mastTypeField.value;
    }

    const lightingField = document.querySelector('[name="HasLighting"]');
    if (!canonicalDraft.HasLighting && lightingField && lightingField.value !== '') {
        canonicalDraft.HasLighting = lightingField.value;
    }

    const wireCountField = document.querySelector('[name="WireCount"]');
    if (!canonicalDraft.WireCount && wireCountField && wireCountField.value) {
        canonicalDraft.WireCount = wireCountField.value;
    }

    const categoryField = document.querySelector('[name="Category"]');
    if (!canonicalDraft.Category && categoryField && categoryField.value) {
        canonicalDraft.Category = categoryField.value;
    }

    canonicalDraft.PointCount = derivePointCount(canonicalDraft);
    if (!Number.isFinite(canonicalDraft.PointCount)) {
        canonicalDraft.PointCount = 0;
    }

    canonicalDraft.savedAt = new Date().toISOString();
    canonicalDraft.isDraft = true;
    canonicalDraft.autoSaved = Boolean(options.autoSave);
    canonicalDraft.ownerId = currentUserId;

    const existingKey = document.getElementById('currentDraftKey')?.value;
    const typeSegment = (canonicalDraft.ObstacleType || 'obstacle')
        .toString()
        .toLowerCase()
        .replace(/[^a-z0-9]+/g, '-')
        .replace(/^-+|-+$/g, '') || 'obstacle';
    const ownerSegment = sanitizeKeySegment(currentUserId);
    const keyPrefix = ownerSegment ? `draft_${ownerSegment}_${typeSegment}` : `draft_${typeSegment}`;
    const draftKey = existingKey || `${keyPrefix}_${Date.now()}`;
    canonicalDraft.draftKey = draftKey;

    return canonicalDraft;
}

function saveDraft() {
    const draftData = buildDraftData();
    if (!draftData) {
        return;
    }

    localStorage.setItem(draftData.draftKey, JSON.stringify(draftData));

    const currentDraftKeyField = document.getElementById('currentDraftKey');
    if (currentDraftKeyField) {
        currentDraftKeyField.value = draftData.draftKey;
    }

    showDraftNotification('Draft saved successfully!');

    try {
        if (window.sessionStorage) {
            window.sessionStorage.setItem('aor_form_notification', 'Draft saved successfully!');
        }
    } catch (error) {
        console.warn('Unable to persist draft notification', error);
    }

    setTimeout(() => {
        window.location.href = '/Crew';
    }, 1000);
}

// Load draft data if available
function loadDraft() {
    const urlParams = new URLSearchParams(window.location.search);
    const draftKey = urlParams.get('draft');

    if (draftKey) {
        const draftData = localStorage.getItem(draftKey);
        if (draftData) {
            try {
                const data = canonicalizeDraftData(JSON.parse(draftData));
                const ownerId = typeof data.ownerId === 'string' ? data.ownerId.trim() : '';
                if (ownerId && currentUserId && ownerId !== currentUserId) {
                    alert('You do not have access to this draft.');
                    return;
                }
                if (data.ObstacleName) {
                    data.ObstacleName = data.ObstacleName.toString().trim();
                }

                if (!data.ObstacleName && data.obstacleName) {
                    data.ObstacleName = data.obstacleName.toString().trim();
                }
                data.draftKey = draftKey;

                const pageType = document.getElementById('hiddenObstacleType')?.value || '';
                const draftType = typeof data.ObstacleType === 'string' ? data.ObstacleType : '';
                const normalizedPageType = pageType.toLowerCase();
                const normalizedDraftType = draftType.toLowerCase();

                if (normalizedDraftType && normalizedPageType && normalizedDraftType !== normalizedPageType) {
                    const params = new URLSearchParams(window.location.search);
                    params.set('draft', draftKey);
                    params.set('type', normalizedDraftType);

                    if (data.Coordinates) {
                        params.set('coordinates', data.Coordinates);
                    }

                    const pointCount = derivePointCount(data);
                    if (!Number.isNaN(pointCount) && pointCount >= 0) {
                        params.set('count', pointCount.toString());
                    }

                    const newUrl = `${window.location.pathname}?${params.toString()}`;
                    if (newUrl !== window.location.href) {
                        window.location.replace(newUrl);
                        return;
                    }
                }

                applyDraftToForm(data);

                const currentDraftKeyField = document.getElementById('currentDraftKey');
                if (currentDraftKeyField) {
                    currentDraftKeyField.value = draftKey;
                }

                showDraftNotification('Draft loaded successfully!');

            } catch (e) {
                console.error('Error loading draft:', e);
            }
        }
    }
}

function updateMapSummaryFromData(data = {}) {
    const typeRaw = (data.ObstacleType || '').toString();
    const typeLower = typeRaw ? typeRaw.toLowerCase() : 'unknown';
    const formattedType = formatObstacleTypeLabel(typeRaw);
    const typeUpper = formattedType.toUpperCase();

    const heading = document.getElementById('summaryTypeHeading');
    if (heading) {
        heading.textContent = typeUpper;
    }

    const typeText = document.getElementById('summaryTypeText');
    if (typeText) {
        typeText.textContent = typeUpper;
    }

    const typeBadge = document.getElementById('summaryTypeBadge');
    if (typeBadge) {
        const badgeType = ['mast', 'line', 'other'].includes(typeLower) ? typeLower : 'other';
        typeBadge.textContent = formattedType;
        typeBadge.className = `obstacle-type-badge badge-${badgeType}`;
    }

    const coordinatesValue = typeof data.Coordinates === 'string'
        ? data.Coordinates
        : JSON.stringify(data.Coordinates ?? []);

    const hiddenCoordinatesField = document.getElementById('hiddenCoordinates');
    if (hiddenCoordinatesField) {
        hiddenCoordinatesField.value = coordinatesValue || '[]';
    }

    renderCoordinatesFromJson(coordinatesValue);

    const pointCount = derivePointCount({
        PointCount: data.PointCount,
        Coordinates: coordinatesValue
    });

    const pointCountSpan = document.getElementById('summaryPointCount');
    if (pointCountSpan) {
        pointCountSpan.textContent = pointCount;
    }

    const hiddenPointCountField = document.getElementById('hiddenPointCount');
    if (hiddenPointCountField) {
        hiddenPointCountField.value = pointCount;
    }

    const hiddenTypeField = document.getElementById('hiddenObstacleType');
    if (hiddenTypeField) {
        hiddenTypeField.value = typeRaw;
    }
}

function formatObstacleTypeLabel(value) {
    if (!value) {
        return 'Draft';
    }

    const lower = value.toLowerCase();
    switch (lower) {
        case 'mast':
            return 'Mast';
        case 'line':
            return 'Line';
        case 'other':
            return 'Other';
        default:
            return value.charAt(0).toUpperCase() + value.slice(1);
    }
}

setInterval(() => {
    const form = document.querySelector('form');
    if (!form) {
        return;
    }

    const formData = new FormData(form);
    let hasContent = false;

    for (let value of formData.values()) {
        if (value && value.toString().trim() !== '') {
            hasContent = true;
            break;
        }
    }

    if (hasContent) {
        saveDraftSilently();
    }
}, 30000);

function saveDraftSilently() {
    const draftData = buildDraftData({ autoSave: true });
    if (!draftData) {
        return;
    }

    const typeSegment = sanitizeKeySegment(draftData.ObstacleType) || 'obstacle';
    const ownerSegment = sanitizeKeySegment(currentUserId);
    const autoPrefix = ownerSegment ? `autosave_${ownerSegment}_${typeSegment}` : `autosave_${typeSegment}`;
    const draftKey = `${autoPrefix}_${Date.now()}`;
    draftData.draftKey = draftKey;
    draftData.ownerId = currentUserId;
    localStorage.setItem(draftKey, JSON.stringify(draftData));

    const autoSaves = Object.keys(localStorage).filter(key => key.startsWith('autosave_'));
    if (autoSaves.length > 3) {
        autoSaves.sort();
        localStorage.removeItem(autoSaves[0]);
    }
}

window.addEventListener('resize', () => {
    const container = document.querySelector('.pilot-form-container');
    if (!container) {
        return;
    }

    clearTimeout(containerResizeTimeout);
    containerResizeTimeout = setTimeout(() => {
        const previousDisplay = container.style.display;
        container.style.display = 'none';
        container.offsetHeight;
        container.style.display = previousDisplay || '';
    }, 100);
});

