import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Repository } from 'typeorm';
import { Category } from '../entities/category.entity';
import { Item } from '../entities/Item.entity';
import { Stock } from '../entities/stock.entity';
import { Purchase } from '../entities/purchase.entity';
import { Seller } from '../entities/seller.entity';
import { Statistics } from '../entities/statistics.entity';

import mysql from 'mysql2/promise';



@Injectable()
export class SeedService {
    
  constructor(
    @InjectRepository(Category) private catRepo: Repository<Category>,
    @InjectRepository(Item) private itemRepo: Repository<Item>,
    @InjectRepository(Stock) private stockRepo: Repository<Stock>,
    @InjectRepository(Purchase) private purchaseRepo: Repository<Purchase>,
    @InjectRepository(Seller) private sellerRepo: Repository<Seller>,
    @InjectRepository(Statistics) private statsRepo: Repository<Statistics>,
  ) {}

  // ——— Pomocnicze ———
  private rand<T>(arr: T[]): T {
    return arr[Math.floor(Math.random() * arr.length)];
  }
  private randInt(min: number, max: number): number {
    return Math.floor(Math.random() * (max - min + 1)) + min;
  }
  private pad(n: number, len: number) {
    return n.toString().padStart(len, '0');
  }
  private makeNip(i: number) {
    // Prosty 10-cyfrowy „NIP” bez sprawdzania sumy kontrolnej, unikalny
    return `${this.pad(856000000 + i, 10)}`;
  }
  private makeSku(catCode: string, brand: string, idx: number) {
    const b = brand.replace(/[^A-Z0-9]/gi, '').toUpperCase().slice(0, 3);
    return `${catCode}-${b}-${this.pad(idx, 5)}`;
  }

  async seedLargeData() {
  // 1) dzieci
  await this.purchaseRepo.createQueryBuilder().delete().where('1=1').execute();
  await this.stockRepo.createQueryBuilder().delete().where('1=1').execute();

  // 2) rodzice
  await this.itemRepo.createQueryBuilder().delete().where('1=1').execute();
  await this.sellerRepo.createQueryBuilder().delete().where('1=1').execute();
  await this.catRepo.createQueryBuilder().delete().where('1=1').execute();

  // 3) inne niezależne tabele na końcu
  await this.statsRepo.createQueryBuilder().delete().where('1=1').execute();

    const categoriesSpec: Array<{ name: string; code: string; brands: string[]; models: string[] }> = [
      { name: 'Elektronika', code: 'EL', brands: ['Samsung', 'Xiaomi', 'Apple', 'Sony', 'LG'], models: ['A14', 'M3', 'Nord 2', 'Bravia X85', 'ThinQ 55'] },
      { name: 'AGD małe', code: 'AM', brands: ['Philips', 'Bosch', 'Tefal', 'Amica', 'Zelmer'], models: ['HD9252', 'ErgoMixx 750', 'Express 4', 'FKV 203', 'ZVC231'] },
      { name: 'AGD duże', code: 'AD', brands: ['Bosch', 'Whirlpool', 'Beko', 'Electrolux', 'Samsung'], models: ['WAN242', 'W7X', 'RCNA406', 'EW7F', 'WW80'] },
      { name: 'Komputery', code: 'PC', brands: ['Lenovo', 'Dell', 'HP', 'Acer', 'Asus'], models: ['ThinkPad E15', 'Latitude 5440', 'ProBook 450', 'Aspire 7', 'VivoBook 15'] },
      { name: 'Gaming', code: 'GM', brands: ['MSI', 'Gigabyte', 'ASUS ROG', 'Razer', 'SteelSeries'], models: ['RTX 4060 Ventus', 'Aorus Elite', 'Strix G16', 'Basilisk V3', 'Arctis Nova'] },
      { name: 'Oświetlenie', code: 'OS', brands: ['Philips Hue', 'Yeelight', 'Osram', 'Kanlux', 'LEDVANCE'], models: ['White Ambiance E27', 'YL Smart Bulb', 'Smart+ GU10', 'Orin 12W', 'RGB Strip 5m'] },
      { name: 'Narzędzia', code: 'NZ', brands: ['Makita', 'Bosch', 'DeWalt', 'Yato', 'Graphite'], models: ['DHP453', 'GBH 2-26', 'DCD796', 'YT-82050', '59G020'] },
      { name: 'Biuro', code: 'BR', brands: ['Canon', 'Brother', 'HP', 'Epson', 'Fellowes'], models: ['PIXMA TS5350', 'HL-1222WE', 'LaserJet M234', 'EcoTank L3250', 'AeraMax DX55'] },
      { name: 'Audio', code: 'AU', brands: ['JBL', 'Sony', 'Marshall', 'Bose', 'Sennheiser'], models: ['Flip 6', 'WH-1000XM5', 'Stanmore II', 'QC45', 'HD 450BT'] },
      { name: 'Foto', code: 'FT', brands: ['Canon', 'Nikon', 'Sony', 'Fujifilm', 'Olympus'], models: ['EOS R10', 'Z50', 'A6400', 'X-S10', 'OM-D E-M10'] },
      { name: 'Sieci i IoT', code: 'NT', brands: ['TP-Link', 'Ubiquiti', 'MikroTik', 'Netgear', 'D-Link'], models: ['Archer AX55', 'UniFi 6 Lite', 'hAP ac^2', 'Orbi RBK', 'DIR-2150'] },
      { name: 'Akcesoria', code: 'AK', brands: ['Sandisk', 'Kingston', 'Logitech', 'Baseus', 'UGREEN'], models: ['Ultra 128GB', 'NV2 1TB', 'MX Master 3S', 'PowerBank 20k', 'USB-C Hub 8w1'] },
    ];

    const categories: Category[] = categoriesSpec.map(cs =>
      this.catRepo.create({ name: cs.name }),
    );
    await this.catRepo.save(categories);

    const cities = ['Warszawa', 'Kraków', 'Wrocław', 'Poznań', 'Gdańsk', 'Katowice', 'Łódź', 'Lublin', 'Rzeszów', 'Szczecin'];
    const streets = ['ul. Długa', 'ul. Mickiewicza', 'al. Jana Pawła II', 'ul. Piotrkowska', 'ul. Karmelicka', 'ul. Grunwaldzka', 'ul. Półwiejska', 'ul. Puławska', 'ul. Zakopiańska', 'ul. Kościuszki'];
    const sellerNames = [
      'TechPol Sp. z o.o.', 'Dom i AGD S.A.', 'CompMasters Sp. z o.o.', 'Świat Światła Sp. z o.o.',
      'AudioMax Polska Sp. z o.o.', 'FotoKadr S.C.', 'NetPoint S.A.', 'GamingPro Sp. z o.o.',
      'BiuroSerwis Sp. z o.o.', 'Narzędziownia S.A.', 'Electro House Sp. z o.o.', 'PixelTrade S.C.',
      'SmartStore Sp. z o.o.', 'MegaMarket S.A.', 'eKomputery Sp. z o.o.', 'AGD Expert Sp. k.',
      'InfoLan S.A.', 'ProOffice Sp. z o.o.', 'Sound&Vision S.C.', 'FotoWorld Sp. z o.o.',
      'GigaNet S.A.', 'LEDMarket Sp. z o.o.', 'PowerTools Sp. z o.o.', 'PrintCare S.C.',
      'Click&Buy Sp. z o.o.',
    ];
    const sellers: Seller[] = sellerNames.map((name, i) => {
      const city = this.rand(cities);
      const street = this.rand(streets);
      const nr = this.randInt(1, 250);
      return this.sellerRepo.create({
        name,
        nip: this.makeNip(1000 + i),
        adress: `${street} ${nr}, ${city}`,
      } as any) as unknown as Seller;
    });
    await this.sellerRepo.save(sellers, { chunk: 200 });


    const items: Item[] = [];
    let globalIdx = 1;

    for (let cIdx = 0; cIdx < categoriesSpec.length; cIdx++) {
      const cat = categories[cIdx];
      const spec = categoriesSpec[cIdx];

      const perCat = this.randInt(12, 18);

      for (let i = 0; i < perCat; i++) {
        const brand = this.rand(spec.brands);
        const model = this.rand(spec.models);

        const variantsByCat: Record<string, string[]> = {
          EL: ['64GB', '128GB', '256GB'],
          AM: ['1.7L', '2L', 'XL'],
          AD: ['7kg', '8kg', '9kg'],
          PC: ['i5/16/512', 'i7/16/1TB', 'Ryzen5/16/512'],
          GM: ['8GB', '12GB', '16GB'],
          OS: ['Ciepła', 'Zimna', 'RGB'],
          NZ: ['18V', '20V', 'SDS-Plus'],
          BR: ['Mono', 'Color', 'A3'],
          AU: ['BT', 'ANC', 'Hi-Res'],
          FT: ['Kit 15-45', 'Body', '18-55'],
          NT: ['AX1800', 'AX3000', 'AX5400'],
          AK: ['Basic', 'Pro', 'Plus'],
        };
        const v = this.rand(variantsByCat[spec.code] || ['Std']);
        const name = `${brand} ${model} ${v}`.replace(/\s+/g, ' ').trim();
        const sku = this.makeSku(spec.code, brand, globalIdx);

        const netto = this.randInt(79, 5999) + [0, 0.5, 0.9][this.randInt(0, 2)];
        const vat = 23; // PL standard
        const brutto = Math.round(netto * (1 + vat / 100) * 100) / 100;

        const it = this.itemRepo.create({
          name,
          sku,
          category: cat,
          nettoPrice: netto,
          bruttoPrice: brutto,
        } as any) as unknown as Item;

        items.push(it);
        globalIdx++;
      }
    }
    await this.itemRepo.save(items, { chunk: 200 });

    // ——— Stany magazynowe ———
    const stocks: Stock[] = items.map((it) =>
      this.stockRepo.create({
        item: it,
        quantity: this.randInt(0, 120),
      }),
    );
    await this.stockRepo.save(stocks, { chunk: 200 });

    // ——— Zakupy (wiązane z itemami; daty z ostatnich 180 dni) ———
    const purchases: Purchase[] = [];
    const purchasesCount = 600; // sporo transakcji

    for (let i = 0; i < purchasesCount; i++) {
      const item = this.rand(items);
      const quantity = this.randInt(1, 20);

      const daysAgo = this.randInt(0, 180);
      const d = new Date();
      d.setDate(d.getDate() - daysAgo);
      d.setHours(this.randInt(9, 18), this.randInt(0, 59), this.randInt(0, 59), 0);

      const purchase = this.purchaseRepo.create({
        item,
        quantity,
        purchaseDate: d,
      } as any) as unknown as Purchase;

      purchases.push(purchase);
    }
    await this.purchaseRepo.save(purchases, { chunk: 200 });

    // ——— Statystyki agregujące ———
    const stats = this.statsRepo.create({
      createdAt: new Date(),
      totalItems: items.length,
      totalStockEntries: stocks.length,
      totalSellers: sellers.length,
      totalPurchases: purchases.length,
    } as any);
    await this.statsRepo.save(stats);

    return {
      categories: categories.length,
      items: items.length,
      stocks: stocks.length,
      sellers: sellers.length,
      purchases: purchases.length,
    };
  }
}
