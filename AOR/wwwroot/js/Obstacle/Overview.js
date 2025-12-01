document.addEventListener('DOMContentLoaded', function() {
    const coordinatesJson = '@Html.Raw(Model.Coordinates ?? "[]")';
    const coordinatesDisplay = document.getElementById('coordinates-display');

    if (coordinatesJson && coordinatesJson !== '[]' && coordinatesDisplay) {
        try {
            const coordinates = JSON.parse(coordinatesJson);
            let html = '';

            coordinates.forEach((coord, index) => {
                html += `<div class="coordinate-item">
                                   Point ${index + 1}: ${coord.lat.toFixed(6)}, ${coord.lng.toFixed(6)}
                                 </div>`;
            });

            coordinatesDisplay.innerHTML = html;
        } catch (e) {
            coordinatesDisplay.innerHTML = '<div class="coordinate-item">Error parsing coordinates</div>';
        }
    }
});