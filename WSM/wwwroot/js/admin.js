document.addEventListener("DOMContentLoaded", function () {
    /* 1. Smooth Toggle Password Section */
    const togglePasswordButton = document.querySelector(".btn-warning[onclick='togglePasswordFields()']");
    const passwordSection = document.getElementById("passwordSection");

    if (togglePasswordButton && passwordSection) {
        togglePasswordButton.addEventListener("click", function () {
            passwordSection.style.transition = "max-height 0.4s ease, opacity 0.4s ease";
            if (passwordSection.style.display === "none" || passwordSection.style.display === "") {
                passwordSection.style.display = "block";
                passwordSection.style.maxHeight = passwordSection.scrollHeight + "px";
                passwordSection.style.opacity = "1";
            } else {
                passwordSection.style.maxHeight = "0";
                passwordSection.style.opacity = "0";
                setTimeout(() => passwordSection.style.display = "none", 400);
            }
        });
    }

    /* 2. Delete Confirmation */
    const deleteButtons = document.querySelectorAll("a.delete-admin");
    if (deleteButtons.length > 0) {
        deleteButtons.forEach(btn => {
            btn.addEventListener("click", function (e) {
                e.preventDefault();
                const url = this.getAttribute("href");
                const adminName = this.getAttribute("data-admin-name");

                Swal.fire({
                    title: `Are you sure you want to delete ${adminName}?`,
                    text: "This action cannot be undone!",
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#d33',
                    cancelButtonColor: '#6c757d',
                    confirmButtonText: 'Yes, delete it!',
                    cancelButtonText: 'Cancel'
                }).then((result) => {
                    if (result.isConfirmed) {
                        window.location.href = url;
                    }
                });
            });
        });
    }

    /* 3. Auto-hide Bootstrap Alerts */
    const alerts = document.querySelectorAll(".alert-danger, .alert-success");
    if (alerts.length > 0) {
        setTimeout(() => {
            alerts.forEach(alert => {
                alert.style.transition = "opacity 0.5s ease";
                alert.style.opacity = "0";
                setTimeout(() => alert.remove(), 500);
            });
        }, 5000);
    }

    /* 4. SweetAlert2 Popup for Success Messages */
    const successAlert = document.querySelector(".alert-success");
    if (successAlert) {
        Swal.fire({
            icon: 'success',
            title: 'Success!',
            text: successAlert.textContent.trim(),
            showConfirmButton: false,
            timer: 2000
        });
    }
});
