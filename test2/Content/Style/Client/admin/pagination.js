
document.addEventListener("DOMContentLoaded", function () {
    // Determine the current page from the URL
    const currentPage = parseInt(new URLSearchParams(window.location.search).get("page")) || 1;
    const paginationItems = document.querySelectorAll(".pagination .page-item");

    paginationItems.forEach((item, index) => {
        const pageNumber = index + 1;
        const link = item.querySelector(".page-link");

        if (pageNumber === currentPage) {
            item.classList.add("active"); // Set the active class on the current page item
            link.style.backgroundColor = "#007bff"; // Primary color for active page
            link.style.color = "#ffffff"; // White text for active page
        }

        // Hover effect for pagination links
        link.addEventListener("mouseover", function () {
            if (!item.classList.contains("active")) {
                link.style.backgroundColor = "#f0f0f0"; // Light gray on hover
            }
        });
        link.addEventListener("mouseout", function () {
            link.style.backgroundColor = item.classList.contains("active") ? "#007bff" : "";
        });
    });

    // Tooltips for 'Previous' and 'Next' buttons
    const prevButton = document.querySelector(".pagination .page-item:first-child .page-link");
    const nextButton = document.querySelector(".pagination .page-item:last-child .page-link");

    if (prevButton) prevButton.setAttribute("title", "Go to previous page");
    if (nextButton) nextButton.setAttribute("title", "Go to next page");
});