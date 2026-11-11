const API = 'https://magazynapi.krdns.pl/api/Storefront';

let products = [];
let cart = JSON.parse(localStorage.getItem('cart') || '{}');

const $ = (id) => document.getElementById(id);
const money = (value) => Number(value).toLocaleString('pl-PL', { style: 'currency', currency: 'PLN' });
const esc = (text) => String(text ?? '').replace(/[&<>"']/g, (c) => `&#${c.charCodeAt(0)};`);

async function api(path, options = {}) {
  const response = await fetch(API + path, {
    ...options,
    headers: { 'Content-Type': 'application/json' },
  });
  const data = await response.json().catch(() => null);
  if (!response.ok) throw new Error(data?.message || 'Wystąpił błąd. Spróbuj ponownie.');
  return data;
}

async function loadProducts() {
  try {
    products = await api('/products');
    renderProducts();
  } catch {
    $('products').innerHTML = '<p class="info">Nie udało się wczytać produktów.</p>';
  }
}

function renderProducts() {
  const query = $('search').value.trim().toLowerCase();
  const list = products
    .filter((p) => p.name.toLowerCase().includes(query))
    .sort((a, b) => (b.availableQuantity > 0) - (a.availableQuantity > 0));

  $('products').innerHTML = list.map((p) => `
    <article class="card">
      <h3>${esc(p.name)}</h3>
      <p class="desc">${esc(p.description)}</p>
      <p class="code">Kod: ${esc(p.sku)}</p>
      <p class="${p.availableQuantity > 0 ? 'stock' : 'stock out'}">
        ${p.availableQuantity > 0 ? 'Dostępny' : 'Niedostępny'}
      </p>
      <p class="price">${money(p.price)}</p>
      <button class="buy" data-id="${p.id}" ${p.availableQuantity > 0 ? '' : 'disabled'}>Do koszyka</button>
    </article>`).join('') || '<p class="info">Brak produktów.</p>';
}

function saveCart() {
  localStorage.setItem('cart', JSON.stringify(cart));
  renderCart();
}

function renderCart() {
  const lines = Object.entries(cart)
    .map(([id, quantity]) => ({ product: products.find((p) => p.id === id), quantity }))
    .filter((line) => line.product);
  const total = lines.reduce((sum, line) => sum + line.product.price * line.quantity, 0);

  $('cartCount').textContent = lines.reduce((sum, line) => sum + line.quantity, 0);
  $('cartTotal').textContent = money(total);
  $('checkout').hidden = lines.length === 0;
  $('cartLines').innerHTML = lines.map(({ product, quantity }) => `
    <div class="line">
      <span>${esc(product.name)}<br><small>${money(product.price)}</small></span>
      <input type="number" min="0" max="${product.availableQuantity}" value="${quantity}" data-qty="${product.id}">
    </div>`).join('') || '<p class="info">Koszyk jest pusty.</p>';
}

$('products').addEventListener('click', (event) => {
  const id = event.target.dataset.id;
  if (!id) return;
  cart[id] = (cart[id] || 0) + 1;
  saveCart();
  event.target.textContent = 'Dodano ✓';
  setTimeout(() => (event.target.textContent = 'Do koszyka'), 1000);
});

$('cartLines').addEventListener('change', (event) => {
  const id = event.target.dataset.qty;
  const quantity = Math.max(0, Math.floor(Number(event.target.value)));
  if (quantity > 0) cart[id] = quantity;
  else delete cart[id];
  saveCart();
});

$('checkout').addEventListener('submit', async (event) => {
  event.preventDefault();
  const form = Object.fromEntries(new FormData(event.target));
  const items = Object.entries(cart).map(([productId, quantity]) => ({ productId, quantity }));

  try {
    const order = await api('/orders', { method: 'POST', body: JSON.stringify({ ...form, items }) });
    cart = {};
    saveCart();
    event.target.reset();
    $('cartLines').innerHTML = `
      <p class="success">Dziękujemy! Zamówienie <b>${esc(order.number)}</b> zostało przyjęte.</p>
      <p>Token do śledzenia zamówienia:</p>
      <p class="token">${esc(order.trackingToken)}</p>`;
    $('token').value = order.trackingToken;
    loadProducts();
  } catch (error) {
    alert(error.message);
  }
});

async function track(token) {
  try {
    const order = await api(`/orders/${encodeURIComponent(token)}/tracking`);
    $('trackResult').innerHTML = `
      <p>Zamówienie <b>${esc(order.number)}</b>: <b>${esc(order.status)}</b></p>
      <p>${esc(order.description)}</p>
      ${order.carrierName ? `<p>Przewoźnik: ${esc(order.carrierName)} ${esc(order.carrierTrackingNumber)}</p>` : ''}
      <ul>${order.items.map((i) => `<li>${esc(i.productName)} × ${i.quantity} – ${money(i.totalPrice)}</li>`).join('')}</ul>
      <p>Razem: <b>${money(order.totalValue)}</b></p>
      ${order.status === 'Nowe' ? '<button class="link" id="cancel">Anuluj zamówienie</button>' : ''}`;
  } catch (error) {
    $('trackResult').innerHTML = `<p class="info">${esc(error.message)}</p>`;
  }
}

$('trackForm').addEventListener('submit', (event) => {
  event.preventDefault();
  track($('token').value.trim());
});

$('trackResult').addEventListener('click', async (event) => {
  if (event.target.id !== 'cancel' || !confirm('Anulować zamówienie?')) return;
  const token = $('token').value.trim();
  try {
    await api(`/tracking/${encodeURIComponent(token)}/cancel`, { method: 'POST' });
    track(token);
  } catch (error) {
    alert(error.message);
  }
});

$('openCart').addEventListener('click', () => $('cart').showModal());
$('openTrack').addEventListener('click', () => $('tracking').showModal());
document.querySelectorAll('.close').forEach((button) =>
  button.addEventListener('click', () => button.closest('dialog').close()));

$('search').addEventListener('input', renderProducts);

loadProducts().then(renderCart);
