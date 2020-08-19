// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function checkAll(isChecked) {
    if (isChecked) {
        $('input[id="team"]').each(function () {
            this.checked = true;
        });
    } else {
        $('input[id="team"]').each(function () {
            this.checked = false;
        });
    }
}

