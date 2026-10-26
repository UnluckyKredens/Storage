import {
  formatPrintDate,
  formatPrintDateTime,
  formatPrintMoney,
  printDocument as printPrintDocument,
  taxLine,
} from '../../../shared/print/print-document';
import { StorefrontOrderHistory } from '../../models/storefront.models';

export function printSalesDocument(order: StorefrontOrderHistory): void {
  printPrintDocument({
    title: order.number,
    heading: `Wydanie do klienta ${order.number}`,
    meta: [
      ['Miejsce wystawienia', order.assignedWarehouseName],
      ['Data zamówienia', formatPrintDate(order.createdOnUtc)],
      ['Data wydania towaru', formatPrintDate(order.shippedOnUtc ?? order.packedOnUtc)],
      ['Status', order.status],
    ],
    parties: [
      {
        title: 'Wydający',
        lines: [
          order.assignedWarehouseName,
          order.assignedWarehouseAddress,
          taxLine(order.assignedWarehouseTaxNumber),
        ],
      },
      {
        title: 'Nabywca',
        lines: [
          order.customerName,
          order.deliveryAddress,
          order.customerEmail,
          order.customerPhone,
        ],
      },
    ],
    columns: [
      { label: 'Lp.', width: '36px', align: 'center' },
      { label: 'Nazwa towaru' },
      { label: 'SKU' },
      { label: 'Ilość', align: 'right', width: '60px' },
      { label: 'Cena', align: 'right', width: '80px' },
      { label: 'Wartość', align: 'right', width: '90px' },
    ],
    rows: order.items.map((item, index) => [
      index + 1,
      item.productName,
      item.sku,
      item.quantity,
      formatPrintMoney(item.unitPrice),
      formatPrintMoney(item.totalPrice),
    ]),
    summary: [['Razem', `${formatPrintMoney(order.totalValue)} zł`]],
    notes: [
      [
        'Spakował(a)',
        order.packedBy ? `${order.packedBy} ${formatPrintDateTime(order.packedOnUtc)}` : null,
      ],
      ['Przewoźnik', order.carrierName],
      ['Nr przesyłki', order.carrierTrackingNumber],
      [
        'Przekazał(a) przewoźnikowi',
        order.shippedBy ? `${order.shippedBy} ${formatPrintDateTime(order.shippedOnUtc)}` : null,
      ],
    ],
    signatures: [
      {
        title: 'Wydał(a):',
        caption: 'Podpis osoby upoważnionej do wydania',
        signer: order.shippedBy ?? order.packedBy ?? order.acceptedBy,
        signedOn: order.shippedOnUtc ?? order.packedOnUtc ?? order.acceptedOnUtc,
      },
      { title: 'Odebrał(a):', caption: 'Podpis przewoźnika lub odbiorcy' },
    ],
  });
}
