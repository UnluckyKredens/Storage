(function (global) {
  const script = document.currentScript;
  const defaultApiUrl = script?.src
    ? new URL('/api/Storefront', script.src).toString().replace(/\/$/, '')
    : '/api/Storefront';

  class MagazineStoreError extends Error {
    constructor(message, status, details) {
      super(message);
      this.name = 'MagazineStoreError';
      this.status = status;
      this.details = details || null;
    }
  }

  class MagazineStoreClient {
    constructor(options) {
      const settings = options || {};
      this.apiUrl = normalizeApiUrl(settings.apiUrl || script?.dataset.apiUrl || defaultApiUrl);
      this.currency = settings.currency || script?.dataset.currency || 'PLN';
    }

    setApiUrl(apiUrl) {
      this.apiUrl = normalizeApiUrl(apiUrl);
      return this;
    }

    getProducts() {
      return request(this.apiUrl, '/products');
    }

    createOrder(order) {
      return request(this.apiUrl, '/orders', {
        method: 'POST',
        body: normalizeOrder(order)
      });
    }

    buy(order) {
      return this.createOrder(order);
    }

    getTracking(identifier) {
      if (!identifier || !String(identifier).trim()) {
        throw new MagazineStoreError('Podaj numer albo ID zamówienia.', 0);
      }

      return request(this.apiUrl, `/orders/${encodeURIComponent(String(identifier).trim())}/tracking`);
    }

    formatMoney(value, currency) {
      return formatMoney(value, currency || this.currency);
    }
  }

  function create(options) {
    return new MagazineStoreClient(options);
  }

  async function request(apiUrl, path, options) {
    const settings = options || {};
    const init = {
      method: settings.method || 'GET',
      headers: {
        accept: 'application/json',
        ...(settings.body ? { 'content-type': 'application/json' } : {}),
        ...(settings.headers || {})
      }
    };

    if (settings.body) {
      init.body = JSON.stringify(settings.body);
    }

    let response;
    try {
      response = await fetch(`${apiUrl}${path}`, init);
    } catch (error) {
      throw new MagazineStoreError(
        'Nie udało się połączyć z API magazynu.',
        0,
        { originalError: error }
      );
    }

    const payload = await readJson(response);
    if (!response.ok) {
      throw new MagazineStoreError(
        payload?.message || 'API magazynu zwróciło błąd.',
        response.status,
        payload
      );
    }

    return payload;
  }

  async function readJson(response) {
    const text = await response.text();
    if (!text) return null;

    try {
      return JSON.parse(text);
    } catch {
      return { message: text };
    }
  }

  function normalizeApiUrl(apiUrl) {
    const value = String(apiUrl || defaultApiUrl).trim();
    return value.replace(/\/$/, '');
  }

  function normalizeOrder(order) {
    if (!order) {
      throw new MagazineStoreError('Brak danych zamówienia.', 0);
    }

    const items = normalizeItems(order.items);
    if (!items.length) {
      throw new MagazineStoreError('Koszyk jest pusty.', 0);
    }

    if (!order.customerName || !String(order.customerName).trim()) {
      throw new MagazineStoreError('Podaj imię i nazwisko.', 0);
    }

    if (!order.customerEmail || !String(order.customerEmail).includes('@')) {
      throw new MagazineStoreError('Podaj poprawny adres e-mail.', 0);
    }

    return {
      customerName: String(order.customerName).trim(),
      customerEmail: String(order.customerEmail).trim(),
      customerPhone: cleanOptional(order.customerPhone),
      deliveryAddress: cleanOptional(order.deliveryAddress),
      notes: cleanOptional(order.notes),
      items
    };
  }

  function normalizeItems(items) {
    if (!Array.isArray(items)) return [];

    const grouped = new Map();
    items.forEach((item) => {
      const productId = item?.productId || item?.id;
      const quantity = Number(item?.quantity || 0);
      if (!productId || quantity <= 0) return;

      const key = String(productId);
      grouped.set(key, (grouped.get(key) || 0) + quantity);
    });

    return [...grouped.entries()].map(([productId, quantity]) => ({
      productId,
      quantity
    }));
  }

  function cleanOptional(value) {
    const text = value == null ? '' : String(value).trim();
    return text || null;
  }

  function formatMoney(value, currency) {
    return new Intl.NumberFormat('pl-PL', {
      style: 'currency',
      currency: currency || 'PLN'
    }).format(Number(value || 0));
  }

  const api = {
    create,
    getProducts: (options) => create(options).getProducts(),
    createOrder: (order, options) => create(options).createOrder(order),
    buy: (order, options) => create(options).buy(order),
    getTracking: (identifier, options) => create(options).getTracking(identifier),
    formatMoney,
    Error: MagazineStoreError
  };

  global.MagazineStore = api;
})(window);
