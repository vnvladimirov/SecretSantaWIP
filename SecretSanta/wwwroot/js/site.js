document.addEventListener("DOMContentLoaded", () => {

    const root = document.documentElement;

    const themeOptions = document.querySelectorAll(
        ".santa-theme-option"
    );

    const modeToggle = document.getElementById("modeToggle");
    const modeIcon = document.getElementById("modeIcon");

    function setTheme(theme) {

        root.dataset.theme = theme;

        try {
            localStorage.setItem("santa-theme", theme);
        }
        catch {
        }
        
        themeOptions.forEach(option => {

            const isSelected =
                option.dataset.themeValue === theme;

            option.setAttribute("aria-pressed", isSelected);
        });
    }

    function setMode(mode) {

        root.dataset.mode = mode;

        try {
            localStorage.setItem("santa-mode", mode);
        }
        catch {
        }

        const isDark = mode === "dark";

        if (modeToggle && modeIcon) {

            modeIcon.className = isDark
                ? "bi bi-sun-fill"
                : "bi bi-moon-stars-fill";

            modeToggle.setAttribute("aria-pressed", isDark);

            modeToggle.title = isDark
                ? "Switch to light mode"
                : "Switch to dark mode";
        }
    }

    themeOptions.forEach(option => {

        option.addEventListener("click", () => {

            const selectedTheme = option.dataset.themeValue;

            setTheme(selectedTheme);
        });
    });

    if (modeToggle) {

        modeToggle.addEventListener("click", () => {

            const currentMode = root.dataset.mode;

            const newMode = currentMode === "dark"
                ? "light"
                : "dark";

            setMode(newMode);
        });
    }   
    setTheme(root.dataset.theme || "christmas");
    setMode(root.dataset.mode || "light");
});
