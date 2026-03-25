document.addEventListener("change", function (event) {
  const target = event.target;
  if (target instanceof HTMLSelectElement && target.dataset.autosubmit === "true") {
    const form = target.form;
    if (form) {
      form.submit();
    }
  }
});
