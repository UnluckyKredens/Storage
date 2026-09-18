import { Module } from '@nestjs/common';
import { PurchaseService } from './purchase.service';
import { PurchaseController } from './purchase.controller';
import { TypeOrmModule } from '@nestjs/typeorm';
import { Purchase } from 'src/entities/purchase.entity';
import { Seller } from 'src/entities/seller.entity';
import { Item } from 'src/entities/Item.entity';
import { Stock } from 'src/entities/stock.entity';

@Module({
  imports: [
    TypeOrmModule.forFeature([Purchase, Seller, Item, Stock])
  ],
  providers: [PurchaseService],
  controllers: [PurchaseController]
})
export class PurchaseModule {}
