document.addEventListener("DOMContentLoaded", function () {

    const searchInput = document.getElementById("shortcutSearch");
    const resultsContainer = document.getElementById("shortcutResults");

    if (!searchInput || !resultsContainer) {
        console.error("Shortcut search elements not found.");
        return;
    }

    let shortcuts = [];
    let selectedIndex = 0;

    fetch("/Shortcut/GetShortcuts")
        .then(response => {
            if (!response.ok) {
                throw new Error("Unable to load shortcuts.");
            }

            return response.json();
        })
        .then(data => {
            shortcuts = data || [];
            console.log("Shortcuts loaded:", shortcuts);
        })
        .catch(error => {
            console.error("Shortcut loading error:", error);
        });


    function searchShortcuts(value) {

        const searchValue = value.toLowerCase().trim();

        if (!searchValue) {
            hideResults();
            return;
        }

        const results = shortcuts.filter(item => {

            const name = (item.name || "").toLowerCase();
            const description = (item.description || "").toLowerCase();

            const keywords = Array.isArray(item.keywords)
                ? item.keywords
                : [];

            return (
                name.includes(searchValue) ||
                description.includes(searchValue) ||
                keywords.some(keyword =>
                    keyword.toLowerCase().includes(searchValue)
                )
            );
        });

        renderResults(results);
    }


    function renderResults(results) {

        resultsContainer.innerHTML = "";

        if (results.length === 0) {

            resultsContainer.innerHTML = `
                <div class="shortcut-no-result">
                    <div class="shortcut-no-result-icon">
                        <i class="las la-search"></i>
                    </div>

                    <div>
                        <div class="shortcut-no-result-title">
                            No results found
                        </div>

                        <div class="shortcut-no-result-text">
                            Try another keyword
                        </div>
                    </div>
                </div>
            `;

            resultsContainer.style.display = "block";

            selectedIndex = -1;

            return;
        }

        selectedIndex = 0;
        const header = document.createElement("div");

        header.className = "shortcut-results-header";
        header.innerHTML = `
            <span>Quick Navigation</span>
            <span>${results.length} result${results.length > 1 ? "s" : ""}</span>
        `;
        resultsContainer.appendChild(header);

        const visibleResults = results.slice(0, 8);

        visibleResults.forEach((item, index) => {
           const resultItem = document.createElement("a");
           resultItem.href = item.url || "#";
           resultItem.className = "shortcut-result-item";
           resultItem.dataset.index = index;

           resultItem.innerHTML = `
                <div class="shortcut-result-icon">
                    <i class="${item.icon || "las la-link"}"></i>
                </div>

                <div class="shortcut-result-info">

                    <div class="shortcut-result-name">
                        ${escapeHtml(item.name || "")}
                    </div>

                    <div class="shortcut-result-description">
                        ${escapeHtml(item.description || "")}
                    </div>

                </div>

                <div class="shortcut-result-action">
                    <i class="las la-arrow-right"></i>
                </div>
            `;
            resultItem.addEventListener("mouseenter", function () {
                selectedIndex = index;
                updateSelection();
            });
            resultItem.addEventListener("click", function () {
                hideResults();
            });
            resultsContainer.appendChild(resultItem);
        });
        resultsContainer.style.display = "block";
        updateSelection();
    }


    function updateSelection() {
        const items = resultsContainer.querySelectorAll(
            ".shortcut-result-item"
        );
        items.forEach((item, index) => {
            if (index === selectedIndex) {
                item.classList.add(
                    "shortcut-result-selected"
                );
            } else {
                item.classList.remove(
                    "shortcut-result-selected"
                );
            }
        });
        const selectedItem = items[selectedIndex];
        if (selectedItem) {
            selectedItem.scrollIntoView({
                block: "nearest"
            });
        }
    }

    searchInput.addEventListener("input", function () {
        searchShortcuts(this.value);
    });

    searchInput.addEventListener("keydown", function (event) {
        const items = resultsContainer.querySelectorAll(
            ".shortcut-result-item"
        );

        if (!items.length) {
            return;
        }
        if (event.key === "ArrowDown") {
            event.preventDefault();
            selectedIndex++;
            if (selectedIndex >= items.length) {
                selectedIndex = 0;
            }
            updateSelection();
            return;
        }

        if (event.key === "ArrowUp") {
            event.preventDefault();
            selectedIndex--;
            if (selectedIndex < 0) {
                selectedIndex = items.length - 1;
            }
            updateSelection();
            return;
        }

        if (event.key === "Enter") {
            event.preventDefault();
            const selectedItem = items[selectedIndex];
            if (selectedItem) {
                window.location.href =
                    selectedItem.getAttribute("href");
            }
            return;
        }

        if (event.key === "Escape") {
            event.preventDefault();
            hideResults();
            searchInput.value = "";
            return;
        }
    });

    document.addEventListener("click", function (event) {
        const wrapper = document.querySelector(".shortcut-search-wrapper");
        if (wrapper && !wrapper.contains(event.target) ) {
            hideResults();
        }
    });

    function hideResults() {
       resultsContainer.style.display = "none";
        resultsContainer.innerHTML = "";
        selectedIndex = 0;
    }


    function escapeHtml(value) {
        const div = document.createElement("div");
        div.textContent = value;
        return div.innerHTML;
    }
});
