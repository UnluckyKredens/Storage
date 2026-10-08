(function () {
  const apiForm = document.querySelector('#apiForm');
  const apiInput = document.querySelector('#apiUrl');
  const trackingForm = document.querySelector('#trackingForm');
  const trackingInput = document.querySelector('#trackingIdentifier');
  const trackingResult = document.querySelector('#trackingResult');
  const shop = document.querySelector('#shop');

  const savedApiUrl = localStorage.getItem('magazine-store-api-url');
  const savedTrackingIdentifier = localStorage.getItem('magazine-store-tracking-id');

  let store = null;
  let products = [];
  let cart = new Map();
  let loading = false;
  let message = '';

  if (savedApiUrl) apiInput.value = savedApiUrl;
  if (savedTrackingIdentifier) trackingInput.value = savedTrackingIdentifier;

  window.addEventListener('load', async () => {
    connectStore();
    await loadProducts();
    if (trackingInput.value.trim()) await loadTracking(trackingInput.value.trim());
  });

  apiForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    localStorage.setItem('magazine-store-api-url', apiInput.value.trim());
    connectStore();
    await loadProducts();
  });

  trackingForm.addEventListener('submit', async (event) => {
    event.preventDefault();
    const identifier = trackingInput.value.trim();
    if (!identifier) {
      renderTrackingError('Wpisz numer lub ID zamówienia.');
      return;
    }

    localStorage.setItem('magazine-store-tracking-id', identifier);
    await loadTracking(identifier);
  });

  function connectStore() {
    if (!window.MagazineStore) {
      message = 'Nie udało się załadować sw.js. Sprawdź, czy API działa pod http://localhost:5292.';
      renderShop();
      return;
    }

    store = window.MagazineStore.create({
      apiUrl: apiInput.value.trim(),
      currency: 'PLN'
    });
  }

  async function loadProducts() {
    if (!store) return;

    loading = true;
    message = 'Ładowanie produktów...';
    renderShop();

    try {
      products = await store.getProducts();
      message = '';
    } catch (error) {
      message = error.message || 'Nie udało się pobrać produktów.';
    } finally {
      loading = false;
      renderShop();
    }
  }

  function renderShop() {
    const cartItems = [...cart.values()];
    const total = cartItems.reduce((sum, item) => sum + item.product.price * item.quantity, 0);

    shop.innerHTML = `
      <header class="shop-header">
        <div>
          <h2>Produkty dostępne do wysyłki</h2>
          <p class="muted">${cartItems.length} pozycji w koszyku</p>
        </div>
        <button type="button" data-reload-products ${loading ? 'disabled' : ''}>Odśwież</button>
      </header>

      <p class="${message ? 'shop-message' : 'shop-message is-empty'}">${escapeHtml(message)}</p>

      <div class="shop-layout">
        <section class="product-grid">
          ${products.map(renderProduct).join('')}
        </section>

        <aside class="cart-box">
          <h2>Koszyk</h2>
          <section class="cart-items">
            ${cartItems.length ? cartItems.map(renderCartItem).join('') : '<p class="muted">Brak produktów.</p>'}
          </section>
          <strong class="cart-total">${money(total)}</strong>
          <form class="checkout-form" id="checkoutForm">
            <input name="customerName" placeholder="Imię i nazwisko" required />
            <input name="customerEmail" type="email" placeholder="E-mail" required />
            <input name="customerPhone" placeholder="Telefon" />
            <textarea name="deliveryAddress" rows="2" placeholder="Adres dostawy"></textarea>
            <textarea name="notes" rows="2" placeholder="Uwagi"></textarea>
            <button type="submit" ${cartItems.length ? '' : 'disabled'}>Kup</button>
          </form>
        </aside>
      </div>
    `;

    shop.querySelector('[data-reload-products]')?.addEventListener('click', loadProducts);
    shop.querySelectorAll('[data-add-product]').forEach((button) => {
      button.addEventListener('click', () => addToCart(button.dataset.addProduct));
    });
    shop.querySelectorAll('[data-buy-product]').forEach((button) => {
      button.addEventListener('click', () => buyNow(button.dataset.buyProduct));
    });
    shop.querySelectorAll('[data-remove-product]').forEach((button) => {
      button.addEventListener('click', () => removeFromCart(button.dataset.removeProduct));
    });
    shop.querySelector('#checkoutForm')?.addEventListener('submit', submitOrder);
  }

  function renderProduct(product) {
    const unavailable = product.availableQuantity <= 0;
    const image = product.imageUrl
      ? `<img src="${escapeAttribute(product.imageUrl)}" alt="${escapeAttribute(product.name)}" loading="lazy" />`
      : '<div class="product-placeholder">Brak zdjęcia</div>';

    return `
      <article class="product-card">
        ${image}
        <div class="product-body">
          <header>
            <h3>${escapeHtml(product.name)}</h3>
            <span>${escapeHtml(product.sku)}</span>
          </header>
          <p>${escapeHtml(product.description || 'Opis produktu nie został uzupełniony.')}</p>
          <span class="availability">Dostępne: ${product.availableQuantity}</span>
          <footer>
            <strong>${money(product.price)}</strong>
            <span class="product-actions">
              <button type="button" data-add-product="${escapeAttribute(product.id)}" ${unavailable ? 'disabled' : ''}>
                ${unavailable ? 'Niedostępny' : 'Dodaj'}
              </button>
              <button type="button" data-buy-product="${escapeAttribute(product.id)}" ${unavailable ? 'disabled' : ''}>Kup teraz</button>
            </span>
          </footer>
        </div>
      </article>
    `;
  }

  function renderCartItem(item) {
    return `
      <div class="cart-row">
        <span>${escapeHtml(item.product.name)} x ${item.quantity}</span>
        <strong>${money(item.product.price * item.quantity)}</strong>
        <span class="cart-controls">
          <button type="button" aria-label="Zmniejsz" data-remove-product="${escapeAttribute(item.product.id)}">-</button>
          <button type="button" aria-label="Zwiększ" data-add-product="${escapeAttribute(item.product.id)}">+</button>
        </span>
      </div>
    `;
  }

  function addToCart(productId) {
    const product = products.find((item) => item.id === productId);
    if (!product) return;

    const current = cart.get(productId);
    const quantity = current ? current.quantity + 1 : 1;
    if (quantity > product.availableQuantity) {
      message = `Dostępne jest tylko ${product.availableQuantity} szt. produktu ${product.name}.`;
      renderShop();
      return;
    }

    cart.set(productId, { product, quantity });
    message = '';
    renderShop();
  }

  function buyNow(productId) {
    cart = new Map();
    addToCart(productId);
    shop.querySelector('[name="customerName"]')?.focus();
  }

  function removeFromCart(productId) {
    const current = cart.get(productId);
    if (!current) return;

    if (current.quantity <= 1) cart.delete(productId);
    else cart.set(productId, { ...current, quantity: current.quantity - 1 });

    renderShop();
  }

  async function submitOrder(event) {
    event.preventDefault();
    if (!store) return;

    const form = event.currentTarget;
    const payload = {
      customerName: form.customerName.value,
      customerEmail: form.customerEmail.value,
      customerPhone: form.customerPhone.value,
      deliveryAddress: form.deliveryAddress.value,
      notes: form.notes.value,
      items: [...cart.values()].map((item) => ({
        productId: item.product.id,
        quantity: item.quantity
      }))
    };

    message = 'Wysyłanie zamówienia...';
    renderShop();

    try {
      const order = await store.createOrder(payload);

      cart.clear();
      message = `Zamówienie ${order.number} zostało przyjęte.`;
      trackingInput.value = order.number || order.id;
      localStorage.setItem('magazine-store-tracking-id', trackingInput.value);
      renderShop();
      await loadTracking(trackingInput.value);
    } catch (error) {
      message = error.message || 'Nie udało się złożyć zamówienia.';
      renderShop();
    }
  }

  async function loadTracking(identifier) {
    if (!store) return;

    trackingResult.innerHTML = '<p class="muted">Sprawdzam status...</p>';
    try {
      const order = await store.getTracking(identifier);
      renderTracking(order);
    } catch (error) {
      renderTrackingError(error.message || 'Błąd śledzenia zamówienia.');
    }
  }

  function renderTracking(order) {
    trackingResult.innerHTML = `
      <article class="tracking-card">
        <dl>
          <dt>Numer</dt>
          <dd>${escapeHtml(order.number)}</dd>
          <dt>Status</dt>
          <dd>${escapeHtml(order.status)}</dd>
          <dt>Lokalizacja</dt>
          <dd>${escapeHtml(order.currentLocation)}</dd>
          <dt>Wartość</dt>
          <dd>${money(order.totalValue)}</dd>
        </dl>
        <p class="muted">${escapeHtml(order.description)}</p>
        <section class="tracking-items">
          ${(order.items || []).map((item) => `
            <div class="tracking-item">
              <span>${escapeHtml(item.productName)} x ${item.quantity}</span>
              <strong>${money(item.totalPrice)}</strong>
            </div>
          `).join('')}
        </section>
      </article>
    `;
  }

  function renderTrackingError(text) {
    trackingResult.innerHTML = `<p class="error">${escapeHtml(text)}</p>`;
  }

  function money(value) {
    return store
      ? store.formatMoney(value)
      : new Intl.NumberFormat('pl-PL', { style: 'currency', currency: 'PLN' }).format(value || 0);
  }

  function escapeHtml(value) {
    return String(value ?? '').replace(/[&<>"']/g, (character) => ({
      '&': '&amp;',
      '<': '&lt;',
      '>': '&gt;',
      '"': '&quot;',
      "'": '&#39;'
    })[character]);
  }

  function escapeAttribute(value) {
    return escapeHtml(value).replace(/`/g, '&#96;');
  }
})();
