(() => {
  const tabs = document.querySelectorAll('[data-profile-tab]');
  const panels = document.querySelectorAll('[data-profile-panel]');

  function activateTab(tabName) {
    tabs.forEach(tab => {
      const active = tab.dataset.profileTab === tabName;
      tab.classList.toggle('active', active);
      tab.setAttribute('aria-selected', String(active));
    });

    panels.forEach(panel => {
      const active = panel.id === tabName;
      panel.classList.toggle('active', active);
      panel.hidden = !active;
    });
  }

  tabs.forEach(tab => tab.addEventListener('click', () => activateTab(tab.dataset.profileTab)));

  const requestedTab = window.location.hash.slice(1);
  if ([...panels].some(panel => panel.id === requestedTab)) activateTab(requestedTab);

  const phoneInput = document.getElementById('phone');
  phoneInput?.addEventListener('input', () => {
    phoneInput.value = phoneInput.value.replace(/\D/g, '');
  });

  const form = document.getElementById('userProfileForm');
  const status = document.getElementById('profileFormStatus');
  form?.addEventListener('submit', async event => {
    event.preventDefault();
    const submitButton = form.querySelector('button[type="submit"]');
    submitButton.disabled = true;
    status.hidden = true;

    const response = await fetch('/api/users/me', {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({
        name: form.elements.Name.value,
        email: form.elements.Email.value,
        phoneNumber: form.elements.PhoneNumber.value,
        dateOfBirth: form.elements.DateOfBirth.value
      })
    });

    if (response.ok) {
      window.location.reload();
      return;
    }

    const problem = await response.json();
    status.textContent = problem.detail || problem.title || 'Unable to update your profile.';
    status.hidden = false;
    submitButton.disabled = false;
  });
})();
