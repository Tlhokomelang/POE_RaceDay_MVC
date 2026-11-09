// RaceDay: small progressive enhancements. Every page also works without JS.
(function () {
  "use strict";

  // Confirm destructive actions: <form data-confirm="Are you sure?">
  document.addEventListener("submit", function (e) {
    var message = e.target.getAttribute("data-confirm");
    if (message && !window.confirm(message)) {
      e.preventDefault();
    }
  });

  // Live preview + size check for image inputs: <input type="file" data-preview="#id">
  document.querySelectorAll("input[type=file][data-preview]").forEach(function (input) {
    input.addEventListener("change", function () {
      var target = document.querySelector(input.getAttribute("data-preview"));
      var warning = document.querySelector(input.getAttribute("data-warning"));
      var file = input.files && input.files[0];
      if (warning) { warning.textContent = ""; }
      if (!file) { return; }
      if (file.size > 5 * 1024 * 1024) {
        if (warning) { warning.textContent = "That image is larger than 5 MB. Please choose a smaller one."; }
        input.value = "";
        return;
      }
      if (target && file.type.indexOf("image/") === 0) {
        var reader = new FileReader();
        reader.onload = function (ev) { target.src = ev.target.result; target.hidden = false; };
        reader.readAsDataURL(file);
      }
    });
  });

  // Category name suggestions: <button data-fill-name="Under 18" data-fill-target="#CategoryName">
  document.querySelectorAll("[data-fill-name]").forEach(function (button) {
    button.addEventListener("click", function () {
      var target = document.querySelector(button.getAttribute("data-fill-target"));
      if (target) { target.value = button.getAttribute("data-fill-name"); target.focus(); }
    });
  });

  // Prevent double-submits
  document.querySelectorAll("form[data-once]").forEach(function (form) {
    form.addEventListener("submit", function () {
      var btn = form.querySelector("button[type=submit]");
      if (btn && form.checkValidity()) { setTimeout(function () { btn.disabled = true; }, 0); }
    });
  });
})();
