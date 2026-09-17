import { Module } from '@nestjs/common';
import { StatisticsController } from './statistics.controller';
import { StatisticsService } from './statistics.service';
import { TypeOrmModule } from '@nestjs/typeorm';
import { Category } from 'src/entities/category.entity';
import { Item } from 'src/entities/Item.entity';
import { Stock } from 'src/entities/stock.entity';
import { Purchase } from 'src/entities/purchase.entity';
import { Seller } from 'src/entities/seller.entity';
import { Statistics } from 'src/entities/statistics.entity';

@Module({
  imports: [
    TypeOrmModule.forFeature([Stock, Item, Category, Purchase, Seller, Statistics])
  ],
  controllers: [StatisticsController],
  providers: [StatisticsService],
})
export class StatisticsModule {}
