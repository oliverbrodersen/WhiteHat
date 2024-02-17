function detectColorScheme() {
    // Default the theme to "dark"
    var theme = "dark";

    // Check if a theme has been saved in localStorage
    if (localStorage.getItem("theme")) {
        theme = localStorage.getItem("theme");
        // Assuming the stored string is quoted, remove the quotes
        theme = theme.substring(1, theme.length - 1);
    }
    // Use matchMedia to check if the user has a preference for "light" scheme
    else if (window.matchMedia && window.matchMedia("(prefers-color-scheme: light)").matches) {
        theme = "light";
    }

    // Apply the theme
    document.documentElement.setAttribute("data-theme", theme);
}

// Invoke the function to set the theme
detectColorScheme();