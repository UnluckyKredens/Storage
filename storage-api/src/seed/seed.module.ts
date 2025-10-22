import { Module } from '@nestjs/common';
import { TypeOrmModule } from '@nestjs/typeorm';
import { Item } from '../entities/Item.entity';
import { Category } from '../entities/category.entity';
import { Stock } from '../entities/stock.entity';
import { Purchase } from '../entities/purchase.entity';
import { Seller } from '../entities/seller.entity';
import { Statistics } from '../entities/statistics.entity';
import { SeedService } from './seed.service';

@Module({
  imports: [TypeOrmModule.forFeature([Item, Category, Stock, Purchase, Seller, Statistics])],
  providers: [SeedService],
  exports: [SeedService],
})
export class SeedModule {}
