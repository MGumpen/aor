document.addEventListener('DOMContentLoaded', function () {
    const checkboxes = document.querySelectorAll('.role-checkbox');
    const hiddenContainer = document.getElementById('roleHiddenInputs');
    const textSpan = document.getElementById('roleDropdownText');

    function updateHiddenInputs() {
        hiddenContainer.innerHTML = '';
        const selectedNames = [];

        checkboxes.forEach(cb => {
            if (cb.checked) {
                const label = cb.closest('.form-check').querySelector('.form-check-label');
                if (label) {
                    selectedNames.push(label.textContent.trim());
                }

                const hidden = document.createElement('input');
                hidden.type = 'hidden';
                hidden.name = 'RoleIds';
                hidden.value = cb.value;
                hiddenContainer.appendChild(hidden);
            }
        });

        if (selectedNames.length === 0) {
            textSpan.textContent = 'Choose role(s)';
        } else {
            textSpan.textContent = selectedNames.join(', ');
        }
    }

    checkboxes.forEach(cb => cb.addEventListener('change', updateHiddenInputs));

    // Initial sync when page loads (for validation errors etc.)
    updateHiddenInputs();
});