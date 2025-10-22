import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { stat } from 'fs';
import { Item } from 'src/entities/Item.entity';
import { Purchase } from 'src/entities/purchase.entity';
import { Seller } from 'src/entities/seller.entity';
import { Statistics } from 'src/entities/statistics.entity';
import { Stock } from 'src/entities/stock.entity';
import { Repository } from 'typeorm';

@Injectable()
export class StatisticsService {

    constructor(
                @InjectRepository(Stock) private stockRepo: Repository<Stock>,
                @InjectRepository(Item) private itemRepo: Repository<Item>,
                @InjectRepository(Seller) private sellerRepo: Repository<Seller>,
                @InjectRepository(Purchase) private purchaseRepo: Repository<Purchase>,
                @InjectRepository(Statistics) private statRepo: Repository<Statistics>,
    ) {}

   async getAllStatistics() {
        var statistics = {
            totalItems: await this.itemRepo.count(),
            totalStockEntries: await this.stockRepo.count(),
            totalSellers: await this.sellerRepo.count(),
            totalPurchases: await this.purchaseRepo.count(),
            createdAt: new Date(),
        };
        var body = await this.statRepo.create(statistics)
        await this.statRepo.save(body);
        return statistics;
    }

    async getStatisticsHistory() {
        return this.statRepo.find();
    }
}
