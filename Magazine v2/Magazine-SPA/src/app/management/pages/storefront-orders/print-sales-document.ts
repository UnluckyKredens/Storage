import { StorefrontOrderHistory } from '../../models/management.models';

export function printSalesDocument(order: StorefrontOrderHistory): void {
  const printWindow = window.open('', '_blank', 'width=900,height=700');
  if (!printWindow) return;
  const escape = (value: string | null | undefined): string =>
    (value ?? '').replace(/[&<>"']/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[c] as string);
  const date = (value: string | null): string => (value ? new Date(value).toLocaleString('pl-PL') : '');
  const money = (value: number): string => value.toFixed(2);

  const rows = order.items
    .map(
      (item, index) => `<tr><td>${index + 1}</td><td>${escape(item.productName)}</td><td>${escape(item.sku)}</td>
        <td>${item.quantity}</td><td>${money(item.unitPrice)}</td><td>${money(item.totalPrice)}</td></tr>`,
    )
    .join('');

  printWindow.document.write(`
    <html>
      <head>
        <title>${escape(order.number)}</title>
        <style>
          body { font-family: Arial, sans-serif; color: #111; margin: 32px; }
          table { border-collapse: collapse; width: 100%; margin-top: 24px; }
          th, td { border: 1px solid #999; padding: 8px; text-align: left; }
          .meta { display: grid; grid-template-columns: repeat(2, 1fr); gap: 4px 24px; }
          .total { margin-top: 16px; text-align: right; font-size: 18px; }
          .signatures { display: grid; grid-template-columns: repeat(3, 1fr); gap: 40px; margin-top: 80px; }
          .signature { border-top: 1px solid #111; padding-top: 6px; text-align: center; font-size: 13px; }
        </style>
      </head>
      <body>
        <h1>Wydanie do klienta ${escape(order.number)}</h1>
        <div class="meta">
          <p><strong>Magazyn:</strong> ${escape(order.assignedWarehouseName)}</p>
          <p><strong>Status:</strong> ${escape(order.status)}</p>
          <p><strong>Nabywca:</strong> ${escape(order.customerName)}</p>
          <p><strong>E-mail:</strong> ${escape(order.customerEmail)}</p>
          <p><strong>Telefon:</strong> ${escape(order.customerPhone)}</p>
          <p><strong>Adres dostawy:</strong> ${escape(order.deliveryAddress)}</p>
          <p><strong>Zamówiono:</strong> ${date(order.createdOnUtc)}</p>
          <p><strong>Spakował(a):</strong> ${escape(order.packedBy)} ${date(order.packedOnUtc)}</p>
          <p><strong>Przewoźnik:</strong> ${escape(order.carrierName)}</p>
          <p><strong>Nr przesyłki:</strong> ${escape(order.carrierTrackingNumber)}</p>
          <p><strong>Przekazał(a) przewoźnikowi:</strong> ${escape(order.shippedBy)} ${date(order.shippedOnUtc)}</p>
        </div>
        <table>
          <thead><tr><th>Lp.</th><th>Produkt</th><th>SKU</th><th>Ilość</th><th>Cena</th><th>Wartość</th></tr></thead>
          <tbody>${rows}</tbody>
        </table>
        <p class="total">Razem: <strong>${money(order.totalValue)} zł</strong></p>
        <div class="signatures">
          <div class="signature">Wydał (magazyn)</div>
          <div class="signature">Odebrał (przewoźnik)</div>
          <div class="signature">Podpis przyjmującego</div>
        </div>
        <script>window.addEventListener('load', function () { setTimeout(function () { window.print(); }, 150); });</script>
      </body>
    </html>
  `);
  printWindow.document.close();
}
