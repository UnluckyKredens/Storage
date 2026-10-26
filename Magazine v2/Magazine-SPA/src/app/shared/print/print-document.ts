export interface PrintParty {
  title: string;
  lines: (string | null | undefined)[];
}

export interface PrintColumn {
  label: string;
  align?: 'left' | 'right' | 'center';
  width?: string;
}

export interface PrintDocument {
  /** Tytuł karty przeglądarki / nazwa pliku PDF. */
  title: string;
  /** Nagłówek dokumentu, np. „Wydanie międzymagazynowe WZ/…”. */
  heading: string;
  /** Pola w prawym górnym rogu (miejsce i daty wystawienia). */
  meta: [label: string, value: string | null | undefined][];
  /** Dwie strony dokumentu: wystawca i odbiorca. */
  parties?: PrintParty[];
  columns: PrintColumn[];
  /** Komórki wierszy; wartości są escapowane. */
  rows: (string | number)[][];
  /** Wiersze podsumowania pod tabelą, np. „Razem”. */
  summary?: [label: string, value: string][];
  /** Dodatkowe informacje (przewoźnik, uwagi) w polu „Uwagi”. */
  notes?: [label: string, value: string | null | undefined][];
  /** Kody QR i kreskowy już jako gotowe adresy obrazów. */
  codes?: { qr: string; barcode: string; identifier: string };
  /** `signer` wypełnia pole podpisem elektronicznym; bez niego pole zostaje puste do podpisu odręcznego. */
  signatures?: {
    title: string;
    caption: string;
    signer?: string | null;
    signedOn?: string | null;
  }[];
}

export function escapeHtml(value: string | number | null | undefined): string {
  return String(value ?? '').replace(/[&<>"']/g, (character) => {
    const entities: Record<string, string> = {
      '&': '&amp;',
      '<': '&lt;',
      '>': '&gt;',
      '"': '&quot;',
      "'": '&#39;',
    };
    return entities[character];
  });
}

export function taxLine(taxNumber: string | null | undefined): string | null {
  return taxNumber ? `NIP: ${taxNumber}` : null;
}

export function formatPrintDate(value: string | Date | null | undefined): string {
  if (!value) return '';
  return new Date(value)
    .toLocaleDateString('pl-PL', { day: '2-digit', month: '2-digit', year: 'numeric' })
    .replace(/\./g, '-');
}

export function formatPrintDateTime(value: string | Date | null | undefined): string {
  if (!value) return '';
  const date = new Date(value);
  const time = date.toLocaleTimeString('pl-PL', { hour: '2-digit', minute: '2-digit' });
  return `${formatPrintDate(date)} ${time}`;
}

export function formatPrintMoney(value: number): string {
  return value.toLocaleString('pl-PL', { minimumFractionDigits: 2, maximumFractionDigits: 2 });
}

const printStyles = `
  @page { size: A4; margin: 14mm; }
  * { box-sizing: border-box; }
  body { margin: 0; color: #111; font-family: Arial, Helvetica, sans-serif; font-size: 13px; }
  .page { min-height: 265mm; display: flex; flex-direction: column; }
  .top { display: flex; justify-content: space-between; align-items: flex-start; gap: 24px; }
  .brand { display: flex; align-items: center; gap: 12px; }
  .brand img { width: 72px; height: 72px; object-fit: contain; }
  .brand strong { display: block; font-size: 24px; color: #1f5fbf; }
  .brand span { color: #555; font-size: 12px; }
  .meta { width: 270px; border-collapse: collapse; text-align: center; }
  .meta th { padding: 3px 6px; background: #e4e4e4; font-size: 12px; font-weight: 400; border-top: 1px solid #777; }
  .meta td { padding: 3px 6px 5px; font-weight: 700; border-bottom: 1px solid #777; }
  .parties { display: grid; grid-template-columns: 1fr 1fr; gap: 28px; margin-top: 36px; }
  .party-title { padding: 3px 6px; background: #e4e4e4; border-top: 1px solid #777; text-align: center; font-weight: 700; }
  .party-body { padding: 6px 2px; border-bottom: 1px solid #777; line-height: 1.45; }
  h1 { margin: 36px 0 18px; text-align: center; font-size: 22px; }
  table.items { width: 100%; border-collapse: collapse; }
  table.items th { padding: 5px 6px; background: #e4e4e4; border: 1px solid #333; font-size: 12px; }
  table.items td { padding: 5px 6px; border: 1px solid #333; }
  table.summary { margin-left: auto; border-collapse: collapse; }
  table.summary td { padding: 4px 8px; border: 1px solid #333; font-weight: 700; }
  table.summary td:first-child { border-right: 0; text-align: right; }
  .notes { margin-top: 22px; padding-top: 6px; border-top: 1px solid #333; line-height: 1.6; }
  .notes b { display: inline-block; min-width: 140px; }
  .codes { display: flex; flex-direction: column; align-items: center; gap: 8px; margin-top: 26px; }
  .codes .qr { width: 110px; height: 110px; }
  .codes .barcode { width: 280px; max-width: 100%; }
  .codes small { font-size: 10px; color: #555; overflow-wrap: anywhere; }
  .signatures { display: grid; gap: 28px; margin-top: auto; padding-top: 40px; }
  .signature-box { height: 84px; border: 1px solid #333; }
  .e-sign { padding: 10px 8px 0; text-align: center; }
  .e-sign small { display: block; color: #555; font-size: 10px; }
  .e-sign strong { display: block; margin: 4px 0 2px; font-size: 15px; font-family: 'Brush Script MT', 'Segoe Script', cursive; font-weight: 400; }
  .signature-box > div:first-child { padding: 3px 6px; background: #e4e4e4; border-bottom: 1px solid #333; text-align: center; font-weight: 700; }
  .signature p { margin: 4px 0 0; text-align: center; font-size: 11px; }
  footer { display: flex; justify-content: space-between; margin-top: 18px; padding-top: 6px; border-top: 1px solid #ccc; color: #666; font-size: 11px; }
  .right { text-align: right; }
  .center { text-align: center; }
`;

function renderParty(party: PrintParty): string {
  const lines = party.lines.filter((line) => !!line).map((line) => escapeHtml(line));
  return `<div><div class="party-title">${escapeHtml(party.title)}</div><div class="party-body">${lines.join('<br />')}</div></div>`;
}

export function renderPrintDocument(doc: PrintDocument): string {
  const meta = doc.meta
    .filter(([, value]) => !!value)
    .map(
      ([label, value]) =>
        `<tr><th>${escapeHtml(label)}</th></tr><tr><td>${escapeHtml(value)}</td></tr>`,
    )
    .join('');
  const head = doc.columns
    .map(
      (column) =>
        `<th class="${column.align ?? 'left'}"${column.width ? ` style="width:${column.width}"` : ''}>${escapeHtml(column.label)}</th>`,
    )
    .join('');
  const body = doc.rows
    .map(
      (row) =>
        `<tr>${row
          .map(
            (cell, index) =>
              `<td class="${doc.columns[index]?.align ?? 'left'}">${escapeHtml(cell)}</td>`,
          )
          .join('')}</tr>`,
    )
    .join('');
  const summary = doc.summary?.length
    ? `<table class="summary">${doc.summary
        .map(
          ([label, value]) =>
            `<tr><td>${escapeHtml(label)}</td><td class="right">${escapeHtml(value)}</td></tr>`,
        )
        .join('')}</table>`
    : '';
  const notes = doc.notes?.filter(([, value]) => !!value).length
    ? `<div class="notes">${doc.notes
        .filter(([, value]) => !!value)
        .map(([label, value]) => `<div><b>${escapeHtml(label)}:</b> ${escapeHtml(value)}</div>`)
        .join('')}</div>`
    : '';
  const codes = doc.codes
    ? `<div class="codes">
        <img class="qr" src="${doc.codes.qr}" alt="QR dokumentu" />
        <img class="barcode" src="${doc.codes.barcode}" alt="Kod kreskowy dokumentu" />
        <small>${escapeHtml(doc.codes.identifier)}</small>
      </div>`
    : '';
  const signatures = doc.signatures?.length
    ? `<div class="signatures" style="grid-template-columns: repeat(${doc.signatures.length}, 1fr)">${doc.signatures
        .map(
          (signature) =>
            `<div class="signature"><div class="signature-box"><div>${escapeHtml(signature.title)}</div>${
              signature.signer
                ? `<div class="e-sign"><small>Podpisano elektronicznie</small><strong>${escapeHtml(signature.signer)}</strong><small>${escapeHtml(formatPrintDateTime(signature.signedOn))}</small></div>`
                : ''
            }</div><p>${escapeHtml(signature.caption)}</p></div>`,
        )
        .join('')}</div>`
    : '';

  return `
    <div class="page">
      <div class="top">
        <div class="brand">
          <img src="${window.location.origin}/logo.png" alt="" />
          <div><strong>Magazine</strong><span>system magazynowy</span></div>
        </div>
        <table class="meta">${meta}</table>
      </div>
      ${doc.parties?.length ? `<div class="parties">${doc.parties.map(renderParty).join('')}</div>` : ''}
      <h1>${escapeHtml(doc.heading)}</h1>
      <table class="items"><thead><tr>${head}</tr></thead><tbody>${body}</tbody></table>
      ${summary}
      ${notes}
      ${codes}
      ${signatures}
      <footer><span>Magazine, system magazynowy</span><span>Wygenerowano ${escapeHtml(formatPrintDateTime(new Date()))}</span></footer>
    </div>`;
}

export function openPrintWindow(title: string, body: string): void {
  const printWindow = window.open('', '_blank', 'width=900,height=900');
  if (!printWindow) return;
  printWindow.document.write(`<!doctype html>
    <html lang="pl">
      <head>
        <meta charset="utf-8" />
        <title>${escapeHtml(title)}</title>
        <style>${printStyles}</style>
      </head>
      <body>${body}<script>window.addEventListener('load', function () { setTimeout(function () { window.print(); }, 250); });</script></body>
    </html>`);
  printWindow.document.close();
}

export function printDocument(doc: PrintDocument): void {
  openPrintWindow(doc.title, renderPrintDocument(doc));
}
