var headerHeight = -1;
function updateVersionText() {
    var versionElement = document.getElementById('version');
    if (!versionElement) return; // Element not found

    var versionValue = localStorage.getItem('version');
    if (versionValue) {
        // Remove any potential double quotes from the string
        versionValue = versionValue.replace(/"/g, '');
        versionElement.textContent = 'v' + versionValue;
    } else {
        versionElement.textContent = 'Welcome to White Hat';
    }

    // Create a new span element
    var spanElement = document.createElement('span');
    spanElement.innerHTML = '&#8226;';

    // Insert the new span as a sibling of the version element
    versionElement.parentNode.insertBefore(spanElement, versionElement.nextSibling);
}
document.addEventListener('DOMContentLoaded', updateVersionText);

function iframeLoaded() {
    console.log(this);
    var iframe = document.getElementById("itemPreview");
    var frameLoader = document.getElementById("frameLoader");

    iframe.style.opacity = 1;
    frameLoader.style.opacity = 0;
}

function scrollToItem(id) {
    var item = document.getElementById(id);
    if (!item) return; // Exit if the element is not found

    var itemPosition = item.getBoundingClientRect().top + window.pageYOffset;

    // Scroll to the element's position minus the header height
    if (window.innerHeight > window.innerWidth) {
        document.getElementById(id).scrollIntoView({ behavior: "smooth", block: "start", inline: "nearest" });
    }
    else {
        window.scrollTo({
            top: itemPosition - getHeaderHeight(),
            behavior: 'smooth'
        });
    }

}

function getHeaderHeight() {
    if (headerHeight == -1) {
        // Extract values from the CSS variable --header-h
        var headerHeightCss = getComputedStyle(document.documentElement).getPropertyValue('--header-h') || 'calc(2.6rem + 48px)';
        headerHeightCss = headerHeightCss.substring(5, headerHeightCss.length - 1);
        var headerHeightValues = headerHeightCss.split('+').map(s => s.trim());

        // Calculate header height in pixels
        var headerHeightInPixels = headerHeightValues.reduce((total, currentValue) => {
            if (currentValue.endsWith('rem')) {
                return total + parseFloat(currentValue) * parseFloat(getComputedStyle(document.documentElement).fontSize);
            } else if (currentValue.endsWith('px')) {
                return total + parseFloat(currentValue);
            }
            return total;
        }, 0);

        headerHeight = headerHeightInPixels
    }

    return headerHeight;
} 

window.setupKeyboardShortcuts = (dotNetReference) => {
    document.addEventListener("keydown", (event) => {

        // Check if the current active element is within a "disable-shortcuts" container
        if (document.activeElement.closest('.disable-shortcuts')) {
            return; // If it is, do nothing and let the event propagate normally
        }

        switch (event.key.toLowerCase()) {
            case "w":
                dotNetReference.invokeMethodAsync("HandleUp", false);
                break;
            case "s":
                dotNetReference.invokeMethodAsync("HandleDown", false);
                break;
            case "a":
                dotNetReference.invokeMethodAsync("HandleCommentToggle");
                break;
            case "d":
                dotNetReference.invokeMethodAsync("HandlePreviewToggle");
                break;
            case "e":
                dotNetReference.invokeMethodAsync("HandleExpand");
                break;
            case "o": 
                dotNetReference.invokeMethodAsync("HandleOpen");
                break;
            case "q": 
                dotNetReference.invokeMethodAsync("HandlePaneCommentModeToggle");
                break;
        }
    });
};