// Modal controls
function openTicketModal() {
    document.getElementById('createTicketModal').classList.add('show');
    loadCategories();
}

function closeTicketModal() {
    document.getElementById('createTicketModal').classList.remove('show');
}

function selectPriority(element, value) {
    document.querySelectorAll('.priority-card').forEach(card => {
        card.classList.remove('selected-low', 'selected-medium', 'selected-high', 'selected-critical');
    });
    element.classList.add(`selected-${value.toLowerCase()}`);
    element.querySelector('input[type=radio]').checked = true;
}

// Load categories dynamically from API
async function loadCategories() {
    const select = document.getElementById('categorySelect');
    if (select.options.length > 1) return; // already loaded

    try {
        const response = await fetch('/Tickets/GetCategories');
        const categories = await response.json();
        categories.forEach(cat => {
            const option = document.createElement('option');
            option.value = cat.id;
            option.textContent = cat.name;
            select.appendChild(option);
        });
    } catch (e) {
        console.error('Failed to load categories', e);
    }
}

// Handle form submission
document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('createTicketForm');
    if (form) {
        form.addEventListener('submit', async function (e) {
            e.preventDefault();

            const formData = new FormData(form);
            const data = {
                title: formData.get('title'),
                description: formData.get('description'),
                categoryId: formData.get('categoryId'),
                priority: formData.get('priority')
            };

            const response = await fetch('/Tickets/Create', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
                },
                body: JSON.stringify(data)
            });

            if (response.ok) {
                closeTicketModal();
                window.location.reload();
            } else {
                alert('Failed to create ticket. Please try again.');
            }
        });
    }
});

// Close modal when clicking outside
document.addEventListener('click', function (e) {
    const modal = document.getElementById('createTicketModal');
    if (modal && e.target === modal) {
        closeTicketModal();
    }
});
git add