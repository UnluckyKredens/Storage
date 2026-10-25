(() => {
  const API_KEY = 'magazine-store-api-url';
  const TRACKING_KEY = 'magazine-store-tracking-id';
  const STEPS = ['Nowe', 'Przyjęte do realizacji', 'Zrealizowane'];
  const GLYPHS = {
    CPU: '<rect x="6" y="6" width="12" height="12" rx="2"/><path d="M9 2v4M15 2v4M9 18v4M15 18v4M2 9h4M2 15h4M18 9h4M18 15h4"/>',
    GPU: '<rect x="2" y="7" width="20" height="10" rx="2"/><circle cx="9" cy="12" r="2.5"/><circle cx="16" cy="12" r="2.5"/>',
    RAM: '<rect x="2" y="8" width="20" height="8" rx="1"/><path d="M6 16v3M10 16v3M14 16v3M18 16v3M6 11h2M11 11h2M16 11h2"/>',
    SSD: '<rect x="4" y="5" width="16" height="14" rx="2"/><circle cx="8" cy="15" r="1"/><path d="M8 9h8"/>',
    HDD: '<rect x="4" y="4" width="16" height="16" rx="2"/><circle cx="12" cy="12" r="4"/><circle cx="12" cy="12" r="1"/>',
    DEFAULT: '<path d="M21 8 12 3 3 8v8l9 5 9-5z"/><path d="m3 8 9 5 9-5M12 13v8"/>'
  };
  const TONES = [['#1f5fbf', '#0f2d5c'], ['#0f766e', '#134e4a'], ['#7c3aed', '#3b1f77'], ['#b45309', '#6b2f06'], ['#be123c', '#5c0a1f'], ['#334155', '#0f172a']];

  const $ = (id) => document.getElementById(id);
  const el = {
    apiForm: $('apiForm'), apiUrl: $('apiUrl'), grid: $('productGrid'), status: $('statusMessage'),
    count: $('resultCount'), search: $('searchInput'), sort: $('sortSelect'), only: $('onlyAvailable'),
    cartCount: $('cartCount'), cartItems: $('cartItems'), cartTotal: $('cartTotal'),
    checkout: $('checkoutForm'), checkoutError: $('checkoutError'), submit: $('submitOrder'),
    trackingInput: $('trackingIdentifier'), trackingResult: $('trackingResult'), toast: $('toast')
  };

  let store = null;
  let products = [];
  const cart = new Map();
  let toastTimer = 0;

  el.apiUrl.value = localStorage.getItem(API_KEY) || el.apiUrl.value;
  el.trackingInput.value = localStorage.getItem(TRACKING_KEY) || '';

  document.querySelectorAll('[data-close]').forEach((b) => b.addEventListener('click', () => b.closest('dialog').close()));
  document.querySelectorAll('dialog').forEach((d) => d.addEventListener('click', (e) => { if (e.target === d) d.close(); }));
  $('cartOpen').addEventListener('click', () => $('cartDrawer').showModal());
  $('settingsOpen').addEventListener('click', () => $('settingsDialog').showModal());
  $('trackingOpen').addEventListener('click', () => openTracking());
  $('refreshBtn').addEventListener('click', loadProducts);
  [el.search, el.sort, el.only].forEach((c) => c.addEventListener('input', renderGrid));

  el.apiForm.addEventListener('submit', (e) => {
    e.preventDefault();
    localStorage.setItem(API_KEY, el.apiUrl.value.trim());
    $('settingsDialog').close();
    connectStore();
    loadProducts();
  });

  el.grid.addEventListener('click', (e) => {
    const btn = e.target.closest('[data-action]');
    if (!btn) return;
    const id = btn.dataset.id;
    if (btn.dataset.action === 'add') addToCart(id);
    if (btn.dataset.action === 'buy') { addToCart(id, true); $('cartDrawer').showModal(); }
  });

  el.cartItems.addEventListener('click', (e) => {
    const btn = e.target.closest('[data-cart]');
    if (!btn) return;
    const id = btn.dataset.id;
    const product = products.find((p) => p.id === id);
    const current = cart.get(id) || 0;
    if (btn.dataset.cart === 'inc') cart.set(id, Math.min(current + 1, product ? product.availableQuantity : current));
    if (btn.dataset.cart === 'dec') current <= 1 ? cart.delete(id) : cart.set(id, current - 1);
    if (btn.dataset.cart === 'remove') cart.delete(id);
    renderCart();
  });

  el.checkout.addEventListener('submit', submitOrder);

  $('trackingForm').addEventListener('submit', (e) => {
    e.preventDefault();
    const token = el.trackingInput.value.trim();
    if (!token) return renderTrackingError('Wpisz token śledzenia zamówienia.');
    localStorage.setItem(TRACKING_KEY, token);
    loadTracking(token);
  });

  $('copyToken').addEventListener('click', async () => {
    try {
      await navigator.clipboard.writeText($('confirmToken').textContent);
      showToast('Token skopiowany');
    } catch {
      showToast('Zaznacz token i skopiuj ręcznie');
    }
  });

  $('confirmTrack').addEventListener('click', () => {
    $('confirmDialog').close();
    openTracking($('confirmToken').textContent);
  });

  connectStore();
  loadProducts();
  renderCart();

  function connectStore() {
    if (!window.MagazineStore) {
      showStatus('Nie udało się załadować sw.js. Sprawdź adres API w ustawieniach (zębatka).', true);
      return;
    }
    store = window.MagazineStore.create({ apiUrl: el.apiUrl.value.trim(), currency: 'PLN' });
  }

  async function loadProducts() {
    if (!store) return;
    showStatus('');
    el.grid.innerHTML = '<div class="skeleton"></div>'.repeat(4);
    try {
      products = await store.getProducts();
      [...cart.keys()].forEach((id) => { if (!products.some((p) => p.id === id)) cart.delete(id); });
    } catch (error) {
      products = [];
      showStatus(error.message || 'Nie udało się pobrać produktów.', true);
    }
    renderGrid();
    renderCart();
  }

  function renderGrid() {
    const query = el.search.value.trim().toLowerCase();
    let list = products.filter((p) => (!el.only.checked || p.availableQuantity > 0)
      && (!query || [p.name, p.sku, p.barcode, p.description].some((v) => (v || '').toLowerCase().includes(query))));

    const sorters = {
      name: (a, b) => a.name.localeCompare(b.name, 'pl'),
      priceAsc: (a, b) => a.price - b.price,
      priceDesc: (a, b) => b.price - a.price,
      stock: (a, b) => b.availableQuantity - a.availableQuantity
    };
    list = list.sort(sorters[el.sort.value]);

    el.count.textContent = products.length ? `Produkty: ${list.length} z ${products.length}` : '';
    if (!list.length) {
      el.grid.innerHTML = products.length ? '<p class="empty">Brak produktów spełniających kryteria.</p>' : '';
      return;
    }
    el.grid.innerHTML = list.map(renderProduct).join('');
  }

  function renderProduct(p) {
    const qty = p.availableQuantity;
    const [a, b] = TONES[hash(p.sku || p.name) % TONES.length];
    const type = skuType(p.sku);
    const media = p.imageUrl
      ? `<img src="${escapeAttribute(p.imageUrl)}" alt="${escapeAttribute(p.name)}" loading="lazy">`
      : `<svg class="glyph" viewBox="0 0 24 24" aria-hidden="true">${GLYPHS[type] || GLYPHS.DEFAULT}</svg>`;
    const stockClass = qty <= 0 ? 'none' : qty <= 5 ? 'low' : '';
    const stockText = qty <= 0 ? 'Brak na stanie' : qty <= 5 ? `Ostatnie sztuki: ${qty}` : `Dostępne: ${qty}`;

    return `
      <article class="card ${qty <= 0 ? 'out' : ''}">
        <div class="thumb" style="--tone-a:${a};--tone-b:${b}">
          ${media}
          ${type !== 'DEFAULT' ? `<span class="tag">${type}</span>` : ''}
        </div>
        <div class="card-body">
          <h3>${escapeHtml(p.name)}</h3>
          <span class="sku">${escapeHtml(p.sku)}</span>
          ${p.description ? `<p class="desc">${escapeHtml(p.description)}</p>` : ''}
          <span class="stock ${stockClass}">${stockText}</span>
          <div class="card-foot">
            <span class="price">${money(p.price)}</span>
            <div class="card-actions">
              <button class="btn ghost" data-action="add" data-id="${p.id}" ${qty <= 0 ? 'disabled' : ''}>Do koszyka</button>
              <button class="btn primary" data-action="buy" data-id="${p.id}" ${qty <= 0 ? 'disabled' : ''}>Kup</button>
            </div>
          </div>
        </div>
      </article>`;
  }

  function addToCart(id, silent = false) {
    const product = products.find((p) => p.id === id);
    if (!product) return;
    const current = cart.get(id) || 0;
    if (current >= product.availableQuantity) {
      showToast('Osiągnięto dostępny stan magazynowy');
      return;
    }
    cart.set(id, current + 1);
    renderCart();
    if (!silent) showToast(`Dodano: ${product.name}`);
  }

  function renderCart() {
    const lines = [...cart.entries()]
      .map(([id, quantity]) => ({ product: products.find((p) => p.id === id), quantity }))
      .filter((l) => l.product);
    const total = lines.reduce((sum, l) => sum + l.product.price * l.quantity, 0);
    const count = lines.reduce((sum, l) => sum + l.quantity, 0);

    el.cartCount.textContent = count;
    el.cartTotal.textContent = money(total);
    el.submit.disabled = !lines.length;
    el.cartItems.innerHTML = lines.length
      ? lines.map(({ product: p, quantity }) => `
        <div class="cart-item">
          <div><strong>${escapeHtml(p.name)}</strong><div class="sub">${escapeHtml(p.sku)} · ${money(p.price)}</div></div>
          <div class="cart-line-right"><strong>${money(p.price * quantity)}</strong>
            <button class="link-btn" data-cart="remove" data-id="${p.id}">Usuń</button></div>
          <div class="qty">
            <button data-cart="dec" data-id="${p.id}" aria-label="Mniej">−</button>
            <span>${quantity}</span>
            <button data-cart="inc" data-id="${p.id}" aria-label="Więcej">+</button>
          </div>
        </div>`).join('')
      : '<p class="empty">Koszyk jest pusty.</p>';
  }

  async function submitOrder(event) {
    event.preventDefault();
    if (!store || !cart.size) return;
    el.checkoutError.hidden = true;
    el.submit.disabled = true;
    el.submit.textContent = 'Wysyłanie...';

    const form = new FormData(el.checkout);
    try {
      const order = await store.createOrder({
        customerName: form.get('customerName'),
        customerEmail: form.get('customerEmail'),
        customerPhone: form.get('customerPhone'),
        deliveryAddress: form.get('deliveryAddress'),
        notes: form.get('notes'),
        items: [...cart.entries()].map(([productId, quantity]) => ({ productId, quantity }))
      });

      cart.clear();
      el.checkout.reset();
      localStorage.setItem(TRACKING_KEY, order.trackingToken);
      el.trackingInput.value = order.trackingToken;
      $('confirmNumber').textContent = order.number;
      $('confirmToken').textContent = order.trackingToken;
      $('cartDrawer').close();
      $('confirmDialog').showModal();
      await loadProducts();
    } catch (error) {
      el.checkoutError.textContent = error.message || 'Nie udało się złożyć zamówienia.';
      el.checkoutError.hidden = false;
    } finally {
      el.submit.textContent = 'Złóż zamówienie';
      renderCart();
    }
  }

  function openTracking(token) {
    if (token) el.trackingInput.value = token;
    $('trackingDialog').showModal();
    if (el.trackingInput.value.trim()) loadTracking(el.trackingInput.value.trim());
  }

  async function loadTracking(token) {
    if (!store) return;
    el.trackingResult.innerHTML = '<p class="muted">Sprawdzanie...</p>';
    try {
      renderTracking(await store.getTracking(token), token);
    } catch (error) {
      renderTrackingError(error.message || 'Błąd śledzenia zamówienia.');
    }
  }

  function renderTracking(order, token) {
    const cancelled = order.status === 'Anulowane';
    const stepIndex = STEPS.indexOf(order.status);
    const pillClass = cancelled ? 'cancelled' : order.status === 'Zrealizowane' ? 'done' : '';
    const steps = cancelled
      ? `<ol class="steps cancelled"><li class="on">Nowe</li><li class="on">Anulowane</li></ol>`
      : `<ol class="steps">${STEPS.map((s, i) => `<li class="${i <= stepIndex ? 'on' : ''}">${s}</li>`).join('')}</ol>`;

    el.trackingResult.innerHTML = `
      <div class="track-head">
        <div><span class="muted">Zamówienie</span><br><strong>${escapeHtml(order.number)}</strong></div>
        <span class="pill ${pillClass}">${escapeHtml(order.status)}</span>
      </div>
      ${steps}
      <p class="track-meta"><strong>Lokalizacja:</strong> ${escapeHtml(order.currentLocation)}</p>
      <p class="muted">${escapeHtml(order.description)}</p>
      <div>${(order.items || []).map((item) => `
        <div class="track-item"><span>${escapeHtml(item.productName)} × ${item.quantity}</span><strong>${money(item.totalPrice)}</strong></div>`).join('')}
        <div class="track-item"><span>Razem</span><strong>${money(order.totalValue)}</strong></div>
      </div>
      ${order.status === 'Nowe' ? '<div class="track-actions"><button class="btn danger" id="cancelOrder" type="button">Anuluj zamówienie</button></div>' : ''}`;

    const cancelBtn = $('cancelOrder');
    if (cancelBtn) cancelBtn.addEventListener('click', () => cancelOrder(token, cancelBtn));
  }

  async function cancelOrder(token, button) {
    if (!confirm('Na pewno anulować to zamówienie?')) return;
    button.disabled = true;
    try {
      const response = await fetch(`${el.apiUrl.value.trim().replace(/\/$/, '')}/tracking/${encodeURIComponent(token)}/cancel`, { method: 'POST' });
      if (!response.ok) throw new Error('Nie udało się anulować zamówienia.');
      showToast('Zamówienie anulowane');
      await loadTracking(token);
      await loadProducts();
    } catch (error) {
      button.disabled = false;
      showToast(error.message);
    }
  }

  function renderTrackingError(text) {
    el.trackingResult.innerHTML = `<p class="form-error">${escapeHtml(text)}</p>`;
  }

  function showStatus(text, isError = false) {
    el.status.hidden = !text;
    el.status.textContent = text;
    el.status.classList.toggle('error', isError);
  }

  function showToast(text) {
    el.toast.textContent = text;
    el.toast.hidden = false;
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => { el.toast.hidden = true; }, 2200);
  }

  function skuType(sku) {
    const part = String(sku || '').split('-')[1] || '';
    return GLYPHS[part] ? part : 'DEFAULT';
  }

  function hash(text) {
    return [...String(text)].reduce((h, c) => (h * 31 + c.charCodeAt(0)) >>> 0, 7);
  }

  function money(value) {
    return store ? store.formatMoney(value) : window.MagazineStore.formatMoney(value, 'PLN');
  }

  function escapeHtml(value) {
    return String(value ?? '').replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[c]));
  }

  function escapeAttribute(value) {
    return escapeHtml(value);
  }
})();
