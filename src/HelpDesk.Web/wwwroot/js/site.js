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
    if (!select) return;
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

// Handle form submission - single listener
document.addEventListener('DOMContentLoaded', function () {
    const form = document.getElementById('createTicketForm');
    if (form) {
        form.addEventListener('submit', async function (e) {
            e.preventDefault();

            // Prevent duplicate submissions
            if (form.dataset.submitting === 'true') return;
            form.dataset.submitting = 'true';

            const submitBtn = form.querySelector('button[type=submit]');
            if (submitBtn) {
                submitBtn.disabled = true;
            }

            const formData = new FormData(form);
            const data = {
                title: formData.get('title'),
                description: formData.get('description'),
                categoryId: formData.get('categoryId'),
                priority: formData.get('priority')
            };

            try {
                const tokenInput = document.querySelector('input[name="__RequestVerificationToken"]');
                const response = await fetch('/Tickets/Create', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                        'RequestVerificationToken': tokenInput ? tokenInput.value : '',
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    body: JSON.stringify(data)
                });

                if (response.ok) {
                    closeTicketModal();
                    window.location.reload();
                } else {
                    alert('Failed to create ticket. Please try again.');
                }
            } catch (err) {
                console.error('Error creating ticket', err);
                alert('Failed to create ticket. Please try again.');
            }
            finally {
                form.dataset.submitting = 'false';
                if (submitBtn) submitBtn.disabled = false;
            }
        }, { once: false });
    }

    // Close modal when clicking outside
    document.addEventListener('click', function (e) {
        const modal = document.getElementById('createTicketModal');
        if (modal && e.target === modal) {
            closeTicketModal();
        }
    });
});

let activeFilters = { status: '', priority: '', category: '', submitter: '', date: '' };

function applyFilters() {
    const searchQuery = (document.getElementById('ticketSearch').value || '').trim().toLowerCase();
    const today = new Date(); today.setHours(0, 0, 0, 0);

    const rows = Array.from(document.querySelectorAll('tbody tr[data-title]'));
    rows.forEach(row => {
        const title = (row.getAttribute('data-title') || '').toLowerCase();
        const status = (row.getAttribute('data-status') || '').toLowerCase();
        const priority = (row.getAttribute('data-priority') || '').toLowerCase();
        const category = (row.getAttribute('data-category') || '').toLowerCase();
        const submitter = (row.getAttribute('data-submitter') || '').toLowerCase();
        const createdStr = row.getAttribute('data-created');

        let matchesDate = true;
        if (activeFilters.date && createdStr) {
            const created = new Date(createdStr); created.setHours(0, 0, 0, 0);
            if (activeFilters.date === 'today') matchesDate = created.getTime() === today.getTime();
            else matchesDate = (today - created) / 86400000 <= parseInt(activeFilters.date);
        }

        const matchesSearch = !searchQuery || title.includes(searchQuery);
        const matchesStatus = !activeFilters.status || status === activeFilters.status;
        const matchesPriority = !activeFilters.priority || priority === activeFilters.priority;
        const matchesCategory = !activeFilters.category || category === activeFilters.category;
        const matchesSubmitter = !activeFilters.submitter || submitter === activeFilters.submitter;

        row.dataset.visible = (matchesSearch && matchesStatus && matchesPriority && matchesCategory && matchesSubmitter && matchesDate) ? '1' : '0';
    });

    applyPagination();
}
