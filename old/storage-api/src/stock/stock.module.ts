import { Module } from '@nestjs/common';
import { StockController } from './stock.controller';
import { StockService } from './stock.service';
import { TypeOrmModule } from '@nestjs/typeorm';
import { Stock } from 'src/entities/stock.entity';
import { Item } from 'src/entities/Item.entity';
import { Category } from 'src/entities/category.entity';

@Module({
  imports: [
    TypeOrmModule.forFeature([Stock, Item, Category])
  ],
  controllers: [StockController],
  providers: [StockService]
})
export class StockModule {}
