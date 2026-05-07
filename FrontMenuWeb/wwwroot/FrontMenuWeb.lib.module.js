// Blazor JS Initializer - runs after all other JS initializer modules (including MudBlazor.Extensions).
// Patches Blazor._internal.domWrapper.focus to guard against null elements,
// which MudBlazor.Extensions can pass during navigation, causing:
// "Cannot read properties of null (reading 'setAttribute')"

export function afterStarted(blazor) {
    const originalFocus = blazor._internal.domWrapper.focus;
    blazor._internal.domWrapper.focus = function (element, preventScroll) {
        if (!element) return;
        try {
            originalFocus.call(this, element, preventScroll);
        } catch (e) {
            // Silently ignore focus errors on unmounted elements
        }
    };
}
