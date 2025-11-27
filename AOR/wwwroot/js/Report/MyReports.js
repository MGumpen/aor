const currentUserId = Html.Raw(System.Text.Json.JsonSerializer.Serialize(currentUserId));
(function () {
    if (typeof window === 'undefined') {
        return;
    }

    let draftKeyToDelete = null;

if (TempData["DeleteDraft"] != null)
    {
        <text>draftKeyToDelete = @Html.Raw(System.Text.Json.JsonSerializer.Serialize(TempData["DeleteDraft"]));</text>
    }

    if (!draftKeyToDelete && window.sessionStorage) {
        draftKeyToDelete = window.sessionStorage.getItem('deleteDraft');
    }

    if (!draftKeyToDelete) {
        return;
    }

    try {
        if (window.sessionStorage) {
            window.sessionStorage.removeItem('deleteDraft');
        }

        if (window.localStorage && window.localStorage.getItem(draftKeyToDelete)) {
            const stored = window.localStorage.getItem(draftKeyToDelete);
            if (stored) {
                try {
                    const parsed = JSON.parse(stored);
                    const ownerId = typeof parsed.ownerId === 'string' ? parsed.ownerId.trim() : '';
                    if (ownerId && currentUserId && ownerId !== currentUserId) {
                        return;
                    }
                } catch (parseError) {
                    console.warn('Unable to inspect stored draft before removal', draftKeyToDelete, parseError);
                }
            }
            window.localStorage.removeItem(draftKeyToDelete);
        }
    } catch (error) {
        console.warn('Unable to clear stored draft state', draftKeyToDelete, error);
    }
})();

(function () {
    const draftsContainer = document.getElementById('draftsContainer');
    if (!draftsContainer) {
        return;
    }

    const draftKeys = Object.keys(localStorage)
        .filter(key => key.startsWith('draft_'))
        .map(key => {
            try {
                const raw = localStorage.getItem(key);
                if (!raw) {
                    return null;
                }

                const data = JSON.parse(raw);
                const canonical = canonicalizeDraftData(data);
                const ownerId = typeof canonical.ownerId === 'string' ? canonical.ownerId.trim() : '';
                if (!ownerId || (currentUserId && ownerId !== currentUserId)) {
                    return null;
                }
                try {
                    localStorage.setItem(key, JSON.stringify(canonical));
                } catch (saveError) {
                    console.warn('Unable to rewrite canonical draft entry', key, saveError);
                }

                return { key, data: canonical };
            } catch (error) {
                console.warn('Unable to parse draft entry', key, error);
                return null;
            }
        })
        .filter(entry => entry && entry.data);

    if (!draftKeys.length) {
        return;
    }

    draftKeys.sort((a, b) => {
        const aDate = new Date(a.data.savedAt || 0).getTime();
        const bDate = new Date(b.data.savedAt || 0).getTime();
        return bDate - aDate;
    });

    draftsContainer.style.display = 'grid';
    draftsContainer.innerHTML = '';

    const emptyState = document.querySelector('.empty-state');
    if (emptyState) {
        emptyState.style.display = 'none';
    }

    const draftsStats = document.getElementById('draftsStats');
    const draftsNumber = document.getElementById('draftsNumber');
    if (draftsStats && draftsNumber) {
        draftsStats.style.display = 'block';
        draftsNumber.textContent = draftKeys.length.toString();
    }

    draftKeys.forEach(entry => {
        const { key, data } = entry;
        const card = document.createElement('div');
        card.className = 'obstacle-card';
        card.dataset.draftKey = key;
        card.style.background = '#fffbe8';
        card.style.border = '1px solid #fde68a';

        const header = document.createElement('div');
        header.className = 'obstacle-header';

        const title = document.createElement('h3');
        title.className = 'obstacle-name';
        const candidateName = getFirstNonEmpty(
            data.ObstacleName,
            data.obstacleName,
            data.displayName,
            data.draftTitle,
            data.title,
            data.name,
            data.ObstacleId,
            data.obstacleId
        );

        title.textContent = candidateName ? candidateName.toString() : 'Draft Unknown';

        // Get obstacle type for badge display
        const obstacleTypeRawValue = getFirstNonEmpty(
            data.ObstacleType,
            data.obstacleType,
            data.type,
            data.hiddenObstacleType,
            data['ObstacleData.ObstacleType'],
            data['obstacle_data.obstacle_type']
        );
        const obstacleTypeRaw = (obstacleTypeRawValue || '').toString().trim();
        const obstacleType = obstacleTypeRaw.toLowerCase();
        const formattedType = formatObstacleType(obstacleTypeRaw);

        // Create type badge (like registered obstacles)
        const typeBadge = document.createElement('span');
        typeBadge.className = 'obstacle-type-badge draft-type-badge';
        typeBadge.textContent = formattedType;

        header.appendChild(title);
        header.appendChild(typeBadge);
        card.appendChild(header);

        const details = document.createElement('div');
        details.className = 'obstacle-details';

        const formattedHeight = formatDraftHeight(data.ObstacleHeight, data.heightInput);
        const pointCount = deriveDraftPointCount(data);

        // Updated detail items WITHOUT the "Type:" row
        const detailItems = [
            { label: 'Height:', value: formattedHeight },
            { label: 'Points:', value: pointCount.toString() },
            { label: 'Registered:', value: formatSavedDate(data.savedAt) }
        ];

        detailItems.forEach(item => {
            const wrapper = document.createElement('div');
            wrapper.className = 'detail-item';

            const label = document.createElement('div');
            label.className = 'detail-label';
            label.textContent = item.label;

            const value = document.createElement('div');
            value.className = 'detail-value';

            if (item.badge) {
                const statusBadge = document.createElement('span');
                statusBadge.className = 'status-badge status-badge-pending';
                statusBadge.style.background = '#fef3c7';
                statusBadge.style.color = '#b45309';
                statusBadge.textContent = item.badge;
                value.appendChild(statusBadge);
            } else {
                value.textContent = item.value ?? '—';
            }

            wrapper.appendChild(label);
            wrapper.appendChild(value);
            details.appendChild(wrapper);
        });

        card.appendChild(details);

        if (data.ObstacleDescription && data.ObstacleDescription.trim() !== '') {
            const description = document.createElement('div');
            description.className = 'obstacle-description';

            const text = data.ObstacleDescription.length > 80
                ? `${data.ObstacleDescription.substring(0, 80)}...`
                : data.ObstacleDescription;

            description.textContent = text;
            card.appendChild(description);
        }

        const actions = document.createElement('div');
        actions.className = 'report-actions';

        const continueLink = document.createElement('a');
        continueLink.className = 'btn-small btn-details';
        const continueParams = new URLSearchParams();
        continueParams.set('draft', key);

        if (obstacleType) {
            continueParams.set('type', obstacleType);
        }

        if (data.Coordinates) {
            continueParams.set('coordinates', data.Coordinates);
        }

        if (!Number.isNaN(pointCount) && pointCount >= 0) {
            continueParams.set('count', pointCount.toString());
        }

        continueLink.href = `/Obstacle/DataForm?${continueParams.toString()}`;
        continueLink.textContent = 'Continue Editing';

        const deleteLink = document.createElement('a');
        deleteLink.className = 'btn-small btn-details';
        deleteLink.style.background = '#fee2e2';
        deleteLink.style.color = '#991b1b';
        deleteLink.style.border = '1px solid #fee2e2';
        deleteLink.href = '#';
        deleteLink.textContent = 'Delete Draft';
        deleteLink.addEventListener('click', function (event) {
            event.preventDefault();

            if (confirm('Delete this draft?')) {
                localStorage.removeItem(key);
                card.remove();

                const remaining = draftsContainer.querySelectorAll('.obstacle-card').length;
                if (draftsStats && draftsNumber) {
                    if (remaining === 0) {
                        draftsStats.style.display = 'none';
                        draftsContainer.style.display = 'none';
                        const emptyStateAfterDelete = document.querySelector('.empty-state');
                        if (emptyStateAfterDelete) {
                            emptyStateAfterDelete.style.display = '';
                        }
                    }
                    draftsNumber.textContent = remaining.toString();
                }
            }
        });

        actions.appendChild(continueLink);
        actions.appendChild(deleteLink);
        card.appendChild(actions);

        draftsContainer.appendChild(card);
    });

    // All helper functions remain the same
    function formatSavedDate(value) {
        if (!value) {
            return 'Just now';
        }

        const date = new Date(value);
        if (Number.isNaN(date.getTime())) {
            return 'Just now';
        }

        const diffMs = Date.now() - date.getTime();
        if (diffMs < 0) {
            return 'Just now';
        }

        const seconds = Math.floor(diffMs / 1000);
        const minutes = Math.floor(seconds / 60);
        const hours = Math.floor(minutes / 60);
        const days = Math.floor(hours / 24);
        if (seconds < 5) return 'Just now';
        if (seconds < 60) return `${seconds} seconds ago`;
        if (minutes === 1) return '1 minute ago';
        if (minutes < 60) return `${minutes} minutes ago`;
        if (hours === 1) return '1 hour ago';
        if (hours < 24) return `${hours} hours ago`;
        if (days === 1) return '1 day ago';
        if (days === 2) return '2 days ago';
        if (days >= 3) return date.toLocaleDateString('no-NO');

        return date.toLocaleDateString('no-NO');
    }

    function deriveDraftPointCount(data) {
        if (!data) {
            return 0;
        }

        const count = Number(data.PointCount);
        if (!Number.isNaN(count) && count >= 0) {
            return count;
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
                console.warn('Unable to parse coordinates for draft card', error);
            }
        }

        return 0;
    }

    function formatDraftHeight(valueMeters, valueFeet) {
        let meters = Number(valueMeters);

        if (Number.isNaN(meters) || meters <= 0) {
            const feet = Number(valueFeet);
            if (!Number.isNaN(feet) && feet > 0) {
                meters = feet * 0.3048;
            }
        }

        if (Number.isNaN(meters) || meters <= 0) {
            return '—';
        }

        return `${meters.toFixed(1)} m`;
    }

    function formatObstacleType(value) {
        if (!value) {
            return 'Unknown';
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

    function formatYesNo(value) {
        if (typeof value === 'boolean') {
            return value ? 'Yes' : 'No';
        }

        if (typeof value === 'string') {
            const trimmed = value.trim().toLowerCase();
            if (trimmed === 'true') {
                return 'Yes';
            }
            if (trimmed === 'false') {
                return 'No';
            }
        }

        return value || '—';
    }

    function canonicalizeDraftData(source = {}) {
        const data = { ...source };

        const getFirstValue = (...keys) => getFirstNonEmptyFrom(data, ...keys);

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

        data.PointCount = deriveDraftPointCount(data);

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

    function getFirstNonEmpty(...values) {
        for (const value of values) {
            if (value === undefined || value === null) {
                continue;
            }

            const candidate = typeof value === 'string' ? value.trim() : value;
            if (candidate !== '' && !(typeof candidate === 'number' && Number.isNaN(candidate))) {
                return candidate;
            }
        }

        return undefined;
    }

    function getFirstNonEmptyFrom(data, ...keys) {
        for (const key of keys) {
            if (Object.prototype.hasOwnProperty.call(data, key)) {
                const candidate = data[key];
                if (candidate !== undefined && candidate !== null) {
                    if (typeof candidate === 'string') {
                        const trimmed = candidate.trim();
                        if (trimmed !== '') {
                            return trimmed;
                        }
                    } else if (!(typeof candidate === 'number' && Number.isNaN(candidate))) {
                        return candidate;
                    }
                }
            }
        }

        return undefined;
    }
})();