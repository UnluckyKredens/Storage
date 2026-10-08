const $ = (selector) => document.querySelector(selector);
const $$ = (selector) => Array.from(document.querySelectorAll(selector));

async function post(url, body) {
  await fetch(url, {
    method: 'POST',
    headers: body ? { 'content-type': 'application/json' } : undefined,
    body: body ? JSON.stringify(body) : undefined
  });
  await refresh();
}

function readAccounts() {
  return $$('.account').map((form) => ({
    name: form.elements.name.value,
    role: form.dataset.role,
    login: form.elements.login.value,
    password: form.elements.password.value
  }));
}

function renderActors(actors) {
  const target = $('#actors');
  target.innerHTML = actors.length
    ? actors.map((actor) => `
        <div class="actor ${actor.isOnline ? 'online' : ''}">
          <div>
            <strong>${escapeHtml(actor.name)}</strong>
            <span>${escapeHtml(actor.login)} | ${escapeHtml(actor.role)} | ${escapeHtml(actor.warehouse || 'brak magazynu')}</span>
          </div>
          <div class="actor-meta">
            <span>${actor.actionsCount} akcji</span>
            <span>${escapeHtml(actor.lastAction || 'czeka')}</span>
            <span>${actor.nextAction ? `nast.: ${escapeHtml(actor.nextAction)} ${formatTime(actor.nextActionUtc)}` : ''}</span>
          </div>
          ${actor.lastError ? `<p class="error">${escapeHtml(actor.lastError)}</p>` : ''}
        </div>
      `).join('')
    : '<p class="empty">Wpisz konta i zapisz konfiguracje.</p>';
}

function renderStats(stats) {
  const entries = Object.entries(stats || {});
  $('#stats').innerHTML = entries.length
    ? entries.map(([name, count]) => `
        <div class="stat">
          <span>${escapeHtml(name)}</span>
          <strong>${count}</strong>
        </div>
      `).join('')
    : '<p class="empty">Brak statystyk.</p>';
}

function renderEvents(events) {
  $('#events').innerHTML = (events || []).slice(0, 120).map((event) => `
    <div class="event ${event.success ? '' : 'fail'}">
      <time>${new Date(event.at).toLocaleTimeString()}</time>
      <strong>${escapeHtml(event.actor)}</strong>
      <span>${escapeHtml(event.action)}</span>
      <p>${escapeHtml(event.message)}</p>
    </div>
  `).join('');
}

async function refresh() {
  const response = await fetch('/api/bot/snapshot');
  const snapshot = await response.json();
  $('#runState').textContent = snapshot.isRunning ? 'dziala' : 'zatrzymany';
  $('#runState').classList.toggle('running', snapshot.isRunning);
  renderActors(snapshot.actors);
  renderStats(snapshot.stats);
  renderEvents(snapshot.events);
}

function escapeHtml(value) {
  return String(value)
    .replaceAll('&', '&amp;')
    .replaceAll('<', '&lt;')
    .replaceAll('>', '&gt;')
    .replaceAll('"', '&quot;')
    .replaceAll("'", '&#039;');
}

function formatTime(value) {
  return value ? new Date(value).toLocaleTimeString() : '';
}

$('#saveAccounts').addEventListener('click', () => post('/api/bot/accounts', { accounts: readAccounts() }));
$('#startBot').addEventListener('click', () => post('/api/bot/start'));
$('#stopBot').addEventListener('click', () => post('/api/bot/stop'));

refresh();
setInterval(refresh, 1500);
