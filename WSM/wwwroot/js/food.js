document.addEventListener("DOMContentLoaded", function () {
    const searchBox = document.querySelector(".search-box");
    const form = document.querySelector(".filter-form");
    const imageInput = document.getElementById("ImageFile");
    const preview = document.getElementById("previewImage");
    const resizedImageField = document.getElementById("ResizedImage");

    /* =========================================================
       SEARCH BOX HIGHLIGHT
    ========================================================= */
    if (searchBox) {
        searchBox.addEventListener("input", function () {
            this.style.borderColor = this.value.trim() !== "" ? "#0d6efd" : "";
        });
    }

    /* =========================================================
       SMOOTH SCROLL AFTER FORM SUBMIT
    ========================================================= */
    if (form) {
        form.addEventListener("submit", function () {
            setTimeout(() => {
                const offsetTop = form.offsetTop - 20;
                window.scrollTo({
                    top: offsetTop,
                    behavior: "smooth"
                });
            }, 200);
        });
    }

    /* =========================================================
       IMAGE PREVIEW + CLIENT-SIDE RESIZE
    ========================================================= */
    if (imageInput && preview && resizedImageField) {
        imageInput.addEventListener("change", function (event) {
            const file = event.target.files[0];
            if (!file) return;

            const reader = new FileReader();
            reader.onload = function (e) {
                const img = new Image();
                img.src = e.target.result;

                img.onload = function () {
                    const canvas = document.createElement("canvas");
                    const ctx = canvas.getContext("2d");

                    const MAX_WIDTH = 300;
                    const MAX_HEIGHT = 300;
                    let width = img.width;
                    let height = img.height;

                    // Maintain aspect ratio
                    if (width > height) {
                        if (width > MAX_WIDTH) {
                            height *= MAX_WIDTH / width;
                            width = MAX_WIDTH;
                        }
                    } else {
                        if (height > MAX_HEIGHT) {
                            width *= MAX_HEIGHT / height;
                            height = MAX_HEIGHT;
                        }
                    }

                    // Resize the image
                    canvas.width = width;
                    canvas.height = height;
                    ctx.drawImage(img, 0, 0, width, height);

                    // Show preview
                    preview.src = canvas.toDataURL("image/jpeg", 0.9);
                    preview.style.display = "block";

                    // Save resized image to hidden field
                    resizedImageField.value = canvas.toDataURL("image/jpeg", 0.9);
                };
            };

            reader.readAsDataURL(file);
        });
    }

    /* =========================================================
       CATEGORY DROPDOWN STYLING
    ========================================================= */
    const categoryDropdown = document.querySelector("select[name='categoryId']");
    if (categoryDropdown) {
        categoryDropdown.addEventListener("change", function () {
            if (this.value) {
                this.style.borderColor = "#198754";
            } else {
                this.style.borderColor = "";
            }
        });
    }
});
