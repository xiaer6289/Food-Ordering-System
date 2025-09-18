// Initiate GET request (AJAX-supported)
$(document).on('click', '[data-get]', e => {
    e.preventDefault();
    const url = e.target.dataset.get;
    location = url || location;
});

// Initiate POST request (AJAX-supported)
$(document).on('click', '[data-post]', e => {
    e.preventDefault();
    const url = e.target.dataset.post;
    const f = $('<form>').appendTo(document.body)[0];
    f.method = 'post';
    f.action = url || location;
    f.submit();
});

// Trim input
$('[data-trim]').on('change', e => {
    e.target.value = e.target.value.trim();
});

// Auto uppercase
$('[data-upper]').on('input', e => {
    const a = e.target.selectionStart;
    const b = e.target.selectionEnd;
    e.target.value = e.target.value.toUpperCase();
    e.target.setSelectionRange(a, b);
});

// RESET form
$('[type=reset]').on('click', e => {
    e.preventDefault();
    location = location;
});

// Check all checkboxes
$('[data-check]').on('click', e => {
    e.preventDefault();
    const name = e.target.dataset.check;
    $(`[name=${name}]`).prop('checked', true);
});

// Uncheck all checkboxes
$('[data-uncheck]').on('click', e => {
    e.preventDefault();
    const name = e.target.dataset.uncheck;
    $(`[name=${name}]`).prop('checked', false);
});

// Row checkable (AJAX-supported)
$(document).on('click', '[data-checkable]', e => {
    if ($(e.target).is(':input,a')) return;

    $(e.currentTarget)
        .find(':checkbox')
        .prop('checked', (i, v) => !v);
});

window.onclick = function (event) {
    if (!event.target.matches('.profile img')) {
        let dropdown = document.getElementById("dropdownMenu");
        if (dropdown.classList.contains('show')) {
            dropdown.classList.remove('show');
        }
    }
}

function generatePDF() {
    const pdf = document.getElementById("PDF");

    //dynamically set the filename with date
    const fileName = "Order_" + new Date().toISOString().slice(0, 10) + ".pdf";

    html2pdf().from(pdf).save(fileName);
}

//Staff 
$(document).ready(function () {
    let timer = null;

    function loadStaff(searchText = '', page = 1) {
        $.ajax({
            url: '@Url.Action("ReadStaff", "Staff")',
            type: 'GET',
            data: { id: searchText, page: page },
            success: function (result) {
                $('#staff-table-container').html(result);
            },
        });
    }

    // Header search input
    $('#search').on('input', function () {
        clearTimeout(timer);
        const searchText = $(this).val();
        timer = setTimeout(() => loadStaff(searchText), 500);
    });

    // Pagination links
    $(document).on('click', '#staff-table-container .pagination-wrapper a', function (e) {
        e.preventDefault();
        const url = new URL($(this).attr('href'), window.location.origin);
        const page = url.searchParams.get('page') || 1;
        const searchText = $('#search').val();
        loadStaff(searchText, page);
    });
});

$(function () {
    // Search on input
    $('#staff-search').on('input', function () {
        let query = $(this).val();
        $.get('@Url.Action("ReadStaff", "Staff")', { id: query }, function (data) {
            $('#staff-table-container').html(data);
        });
    });

    // Clear search
    $('#clear-search').on('click', function () {
        $('#staff-search').val('');
        $.get('@Url.Action("ReadStaff", "Staff")', {}, function (data) {
            $('#staff-table-container').html(data);
        });
    });
});

document.addEventListener('DOMContentLoaded', function () {
    const photoInput = document.querySelector('input[name="Photo"]');
    const preview = document.getElementById('previewImage');

    if (photoInput) {
        photoInput.addEventListener('change', function (event) {
            const file = event.target.files[0];
            if (file) {
                const reader = new FileReader();
                reader.onload = function (e) {
                    preview.src = e.target.result;
                    preview.style.display = 'block';
                };
                reader.readAsDataURL(file);
            } else {
                preview.style.display = 'none';
            }
        });
    }
});

/* main Menu dropdown */
function toggleDropdown() {
        document.getElementById("dropdownMenu").classList.toggle("show");
}
    // Close dropdown when clicking outside
    window.onclick = function(event) {
    if (!event.target.closest('.portrait')) {
        document.getElementById("dropdownMenu").classList.remove("show");
    }
}

//Main menu redirect to no permission page
const role = document.body.getAttribute("data-role");

document.querySelectorAll(".restricted").forEach(btn => {
    btn.addEventListener("click", e => {
        if (role === "Staff") {
            e.preventDefault();
            window.location.href = "/Home/NoPermission";
        }
    });
});


