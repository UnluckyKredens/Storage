'use strict';

/*
 * Sklep demonstracyjny Magazine Store.
 * Z API rozmawia wyłącznie biblioteka sw.js (window.MagazineStore), serwowana przez samo API.
 * Ten plik odpowiada tylko za interfejs: lista produktów, koszyk, zamówienie i śledzenie.
 */
(() => {
  const DEFAULT_API_ORIGIN = 'https://magazynapi.krdns.pl';
  const STORAGE_KEYS = {
    apiOrigin: 'magazine-store-api-origin',
    trackingToken: 'magazine-store-tracking-token',
    cart: 'magazine-store-cart',
  };
  const ORDER_STEPS = ['Nowe', 'Przyjęte do realizacji', 'Spakowane', 'Wysłane'];
  const moneyFormat = new Intl.NumberFormat('pl-PL', { style: 'currency', currency: 'PLN' });

  const byId = (id) => document.getElementById(id);
  const dom = {
    search: byId('searchInput'),
    onlyAvailable: byId('onlyAvailable'),
    sort: byId('sortSelect'),
    status: byId('statusMessage'),
    grid: byId('productGrid'),
    cartCount: byId('cartCount'),
    cartDrawer: byId('cartDrawer'),
    cartItems: byId('cartItems'),
    cartTotal: byId('cartTotal'),
    checkoutForm: byId('checkoutForm'),
    checkoutError: byId('checkoutError'),
    submitOrder: byId('submitOrder'),
    confirmDialog: byId('confirmDialog'),
    confirmNumber: byId('confirmNumber'),
    confirmToken: byId('confirmToken'),
    trackingDialog: byId('trackingDialog'),
    trackingForm: byId('trackingForm'),
    trackingToken: byId('trackingToken'),
    trackingResult: byId('trackingResult'),
    settingsDialog: byId('settingsDialog'),
    apiForm: byId('apiForm'),
    apiOrigin: byId('apiOrigin'),
    toast: byId('toast'),
  };

  const state = {
    store: null,
    products: [],
    cart: loadCart(),
    toastTimer: 0,
  };

  bindEvents();
  renderCart();
  start();

  // --- Start ------------------------------------------------------------------

  async function start() {
    const apiOrigin = (localStorage.getItem(STORAGE_KEYS.apiOrigin) || DEFAULT_API_ORIGIN).replace(/\/+$/, '');
    dom.apiOrigin.value = apiOrigin;
    dom.trackingToken.value = localStorage.getItem(STORAGE_KEYS.trackingToken) || '';

    try {
      await loadScript(`${apiOrigin}/sw.js`);
      state.store = window.MagazineStore.create({ apiUrl: `${apiOrigin}/api/Storefront` });
    } catch {
      showStatus('Nie udało się połączyć ze sklepem. Sprawdź adres w „Ustawieniach API” na dole strony.');
      return;
    }

    await loadProducts();
  }

  function loadScript(src) {
    return new Promise((resolve, reject) => {
      const script = document.createElement('script');
      script.src = src;
      script.onload = () => (window.MagazineStore ? resolve() : reject(new Error('Brak MagazineStore')));
      script.onerror = reject;
      document.head.append(script);
    });
  }

  async function loadProducts() {
    showStatus('');
    try {
      state.products = await state.store.getProducts();
    } catch (error) {
      state.products = [];
      showStatus(error.message || 'Nie udało się pobrać produktów.');
    }
    syncCartWithStock();
    renderProducts();
    renderCart();
  }

  // --- Zdarzenia -------------------------------------------------------------

  function bindEvents() {
    document.querySelectorAll('[data-close]').forEach((button) => {
      button.addEventListener('click', () => button.closest('dialog').close());
    });
    // Kliknięcie w tło zamyka okno.
    document.querySelectorAll('dialog').forEach((dialog) => {
      dialog.addEventListener('click', (event) => {
        if (event.target === dialog) dialog.close();
      });
    });

    byId('cartOpen').addEventListener('click', () => dom.cartDrawer.showModal());
    byId('trackingOpen').addEventListener('click', () => openTracking());
    byId('settingsOpen').addEventListener('click', () => dom.settingsDialog.showModal());

    dom.search.addEventListener('input', renderProducts);
    dom.onlyAvailable.addEventListener('change', renderProducts);
    dom.sort.addEventListener('change', renderProducts);

    dom.grid.addEventListener('click', (event) => {
      const button = event.target.closest('[data-add]');
      if (button) addToCart(button.dataset.add);
    });

    dom.cartItems.addEventListener('click', (event) => {
      const button = event.target.closest('[data-cart-action]');
      if (!button) return;
      const id = button.dataset.id;
      const change = { increase: 1, decrease: -1, remove: -Infinity }[button.dataset.cartAction];
      setCartQuantity(id, (state.cart.get(id) || 0) + change);
    });

    dom.checkoutForm.addEventListener('submit', submitOrder);
    dom.checkoutForm.addEventListener('input', (event) => event.target.removeAttribute('aria-invalid'));

    dom.trackingForm.addEventListener('submit', (event) => {
      event.preventDefault();
      const token = dom.trackingToken.value.trim();
      if (token) loadTracking(token);
      else renderTrackingError('Wpisz token śledzenia z potwierdzenia zamówienia.');
    });

    dom.trackingResult.addEventListener('click', (event) => {
      const button = event.target.closest('[data-cancel-order]');
      if (button) cancelOrder(button.dataset.cancelOrder, button);
    });

    byId('copyToken').addEventListener('click', copyToken);
    byId('confirmTrack').addEventListener('click', () => {
      dom.confirmDialog.close();
      openTracking(dom.confirmToken.textContent);
    });

    dom.apiForm.addEventListener('submit', (event) => {
      event.preventDefault();
      localStorage.setItem(STORAGE_KEYS.apiOrigin, dom.apiOrigin.value.trim().replace(/\/+$/, ''));
      location.reload(); // inny serwer może mieć inną wersję sw.js
    });
  }

  // --- Lista produktów -------------------------------------------------------

  function renderProducts() {
    const query = dom.search.value.trim().toLowerCase();
    const sorters = {
      name: (a, b) => a.name.localeCompare(b.name, 'pl'),
      priceAsc: (a, b) => a.price - b.price,
      priceDesc: (a, b) => b.price - a.price,
    };
    const list = state.products
      .filter((product) => !dom.onlyAvailable.checked || product.availableQuantity > 0)
      .filter((product) => !query || [product.name, product.sku, product.barcode]
        .some((value) => (value || '').toLowerCase().includes(query)))
      .sort(sorters[dom.sort.value]);

    dom.grid.innerHTML = list.length
      ? list.map(renderCard).join('')
      : state.products.length ? '<p class="empty">Brak produktów spełniających kryteria.</p>' : '';
  }

  function renderCard(product) {
    const available = product.availableQuantity;
    const inCart = state.cart.get(product.id) || 0;
    const stock = available > 0
      ? `<span class="stock">Dostępne: ${formatQuantity(available)} szt.</span>`
      : '<span class="stock none">Brak na stanie</span>';

    return `
      <article class="card">
        ${product.imageUrl ? `<img src="${escapeHtml(product.imageUrl)}" alt="" loading="lazy">` : ''}
        <h2>${escapeHtml(product.name)}</h2>
        <span class="sku">${escapeHtml(product.sku)}</span>
        ${stock}
        ${inCart ? `<span class="in-cart">W koszyku: ${inCart} szt.</span>` : ''}
        <span class="price">${formatMoney(product.price)}</span>
        <button class="btn primary block" type="button" data-add="${escapeHtml(product.id)}"
          ${inCart >= available ? 'disabled' : ''}>Do koszyka</button>
      </article>`;
  }

  // --- Koszyk ----------------------------------------------------------------

  function addToCart(id) {
    const product = findProduct(id);
    if (product && setCartQuantity(id, (state.cart.get(id) || 0) + 1)) {
      showToast(`Dodano do koszyka: ${product.name}`);
    }
  }

  /** Ustawia ilość w koszyku w granicach dostępnego stanu. Zwraca true, gdy ilość się zmieniła. */
  function setCartQuantity(id, quantity) {
    const limit = findProduct(id)?.availableQuantity || 0;
    const current = state.cart.get(id) || 0;
    const next = Math.max(0, Math.min(quantity, limit));
    if (next === current) return false;

    if (next === 0) state.cart.delete(id);
    else state.cart.set(id, next);

    saveCart();
    renderCart();
    renderProducts();
    return true;
  }

  /** Po pobraniu katalogu usuwa z koszyka produkty niedostępne i przycina ilości do stanu. */
  function syncCartWithStock() {
    if (!state.products.length) return;
    for (const [id, quantity] of state.cart) {
      const available = findProduct(id)?.availableQuantity || 0;
      if (available <= 0) state.cart.delete(id);
      else if (quantity > available) state.cart.set(id, available);
    }
    saveCart();
  }

  function renderCart() {
    const lines = [...state.cart]
      .map(([id, quantity]) => ({ product: findProduct(id), quantity }))
      .filter((line) => line.product);
    const total = lines.reduce((sum, line) => sum + line.product.price * line.quantity, 0);

    dom.cartCount.textContent = String(lines.reduce((sum, line) => sum + line.quantity, 0));
    dom.cartTotal.textContent = formatMoney(total);
    dom.submitOrder.disabled = !lines.length || !state.store;
    dom.checkoutForm.hidden = !lines.length;
    dom.cartItems.innerHTML = lines.length
      ? lines.map(renderCartLine).join('')
      : '<p class="cart-empty">Koszyk jest pusty.</p>';
  }

  function renderCartLine({ product, quantity }) {
    const id = escapeHtml(product.id);
    return `
      <div class="cart-item">
        <span class="cart-name">${escapeHtml(product.name)}</span>
        <span class="cart-line-total">${formatMoney(product.price * quantity)}</span>
        <div class="cart-controls">
          <span class="qty">
            <button type="button" data-cart-action="decrease" data-id="${id}" aria-label="Zmniejsz ilość">−</button>
            <span>${quantity}</span>
            <button type="button" data-cart-action="increase" data-id="${id}" aria-label="Zwiększ ilość"
              ${quantity >= product.availableQuantity ? 'disabled' : ''}>+</button>
          </span>
          <button class="link-btn danger" type="button" data-cart-action="remove" data-id="${id}">Usuń</button>
        </div>
      </div>`;
  }

  function loadCart() {
    try {
      const saved = JSON.parse(localStorage.getItem(STORAGE_KEYS.cart) || '[]');
      return new Map(saved.filter(([id, quantity]) => typeof id === 'string' && quantity > 0));
    } catch {
      return new Map();
    }
  }

  function saveCart() {
    localStorage.setItem(STORAGE_KEYS.cart, JSON.stringify([...state.cart]));
  }

  // --- Zamówienie ------------------------------------------------------------

  async function submitOrder(event) {
    event.preventDefault();
    if (!state.store || !state.cart.size || !validateCheckout()) return;

    dom.checkoutError.hidden = true;
    dom.submitOrder.disabled = true;
    dom.submitOrder.textContent = 'Wysyłanie…';

    const form = new FormData(dom.checkoutForm);
    try {
      const order = await state.store.createOrder({
        customerName: form.get('customerName'),
        customerEmail: form.get('customerEmail'),
        customerPhone: form.get('customerPhone'),
        deliveryAddress: form.get('deliveryAddress'),
        notes: form.get('notes'),
        items: [...state.cart].map(([productId, quantity]) => ({ productId, quantity })),
      });

      state.cart.clear();
      saveCart();
      dom.checkoutForm.reset();
      localStorage.setItem(STORAGE_KEYS.trackingToken, order.trackingToken);
      dom.trackingToken.value = order.trackingToken;
      dom.confirmNumber.textContent = order.number;
      dom.confirmToken.textContent = order.trackingToken;
      dom.cartDrawer.close();
      dom.confirmDialog.showModal();
      await loadProducts();
    } catch (error) {
      dom.checkoutError.textContent = error.message || 'Nie udało się złożyć zamówienia.';
      dom.checkoutError.hidden = false;
    } finally {
      dom.submitOrder.textContent = 'Złóż zamówienie';
      renderCart();
    }
  }

  /** Oznacza puste lub błędne pola wymagane i przenosi fokus na pierwsze z nich. */
  function validateCheckout() {
    const invalid = [...dom.checkoutForm.elements].filter((field) => field.willValidate && !field.checkValidity());
    invalid.forEach((field) => field.setAttribute('aria-invalid', 'true'));
    if (!invalid.length) return true;

    dom.checkoutError.textContent = 'Uzupełnij imię i nazwisko oraz poprawny adres e-mail.';
    dom.checkoutError.hidden = false;
    invalid[0].focus();
    return false;
  }

  async function copyToken() {
    try {
      await navigator.clipboard.writeText(dom.confirmToken.textContent);
      showToast('Token skopiowany do schowka');
    } catch {
      showToast('Zaznacz token i skopiuj go ręcznie');
    }
  }

  // --- Śledzenie i anulowanie ------------------------------------------------

  function openTracking(token) {
    if (token) dom.trackingToken.value = token;
    dom.trackingResult.innerHTML = '';
    dom.trackingDialog.showModal();
    if (dom.trackingToken.value.trim()) loadTracking(dom.trackingToken.value.trim());
  }

  async function loadTracking(token) {
    if (!state.store) return;
    localStorage.setItem(STORAGE_KEYS.trackingToken, token);
    dom.trackingResult.innerHTML = '<p class="muted">Sprawdzanie…</p>';
    try {
      renderTracking(await state.store.getTracking(token), token);
    } catch (error) {
      renderTrackingError(error.message || 'Nie udało się sprawdzić zamówienia.');
    }
  }

  function renderTracking(order, token) {
    const cancelled = order.status === 'Anulowane';
    const currentStep = ORDER_STEPS.indexOf(order.status);
    const pillClass = cancelled ? 'cancelled' : order.status === 'Wysłane' ? 'done' : '';
    const steps = cancelled
      ? '<ol class="steps cancelled"><li class="on">Nowe</li><li class="on">Anulowane</li></ol>'
      : `<ol class="steps">${ORDER_STEPS.map((step, index) =>
        `<li class="${index <= currentStep ? 'on' : ''}">${step}</li>`).join('')}</ol>`;
    const carrier = order.carrierName
      ? `<p class="track-meta"><strong>Przewoźnik:</strong> ${escapeHtml(order.carrierName)}${
        order.carrierTrackingNumber ? `, nr przesyłki ${escapeHtml(order.carrierTrackingNumber)}` : ''}</p>`
      : '';
    const items = (order.items || []).map((item) => `
      <div class="track-item">
        <span>${escapeHtml(item.productName)} × ${formatQuantity(item.quantity)}</span>
        <strong>${formatMoney(item.totalPrice)}</strong>
      </div>`).join('');
    const cancelButton = order.status === ORDER_STEPS[0]
      ? `<div class="track-actions">
          <button class="btn danger" type="button" data-cancel-order="${escapeHtml(token)}">Anuluj zamówienie</button>
        </div>`
      : '';

    dom.trackingResult.innerHTML = `
      <div class="track-head">
        <div><span class="muted">Zamówienie</span><br><strong>${escapeHtml(order.number)}</strong></div>
        <span class="pill ${pillClass}">${escapeHtml(order.status)}</span>
      </div>
      ${steps}
      <p class="track-meta"><strong>Gdzie jest:</strong> ${escapeHtml(order.currentLocation)}</p>
      <p class="track-meta muted">${escapeHtml(order.description)}</p>
      ${carrier}
      ${items}
      <div class="track-item"><span>Razem</span><strong>${formatMoney(order.totalValue)}</strong></div>
      ${cancelButton}`;
  }

  async function cancelOrder(token, button) {
    if (!confirm('Na pewno anulować to zamówienie?')) return;
    button.disabled = true;
    try {
      await state.store.cancelOrder(token);
      showToast('Zamówienie zostało anulowane');
      await loadTracking(token);
      await loadProducts();
    } catch (error) {
      button.disabled = false;
      showToast(error.message || 'Nie udało się anulować zamówienia.');
    }
  }

  function renderTrackingError(text) {
    dom.trackingResult.innerHTML = `<p class="form-error">${escapeHtml(text)}</p>`;
  }

  // --- Pomocnicze ------------------------------------------------------------

  function findProduct(id) {
    return state.products.find((product) => product.id === id);
  }

  function showStatus(text) {
    dom.status.hidden = !text;
    dom.status.textContent = text;
  }

  function showToast(text) {
    dom.toast.textContent = text;
    dom.toast.hidden = false;
    clearTimeout(state.toastTimer);
    state.toastTimer = setTimeout(() => {
      dom.toast.hidden = true;
    }, 2400);
  }

  function formatMoney(value) {
    return moneyFormat.format(Number(value) || 0);
  }

  function formatQuantity(value) {
    return Number(value).toLocaleString('pl-PL', { maximumFractionDigits: 3 });
  }

  function escapeHtml(value) {
    const entities = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' };
    return String(value ?? '').replace(/[&<>"']/g, (char) => entities[char]);
  }
})();
