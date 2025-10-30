import { Module } from '@nestjs/common';
import { AppController } from './app.controller';
import { AppService } from './app.service';
import { TypeOrmModule } from '@nestjs/typeorm';
import { CategoryModule } from './category/category.module';
import { ItemModule } from './item/item.module';
import { join } from 'path';
import { Item } from './entities/Item.entity';
import { Category } from './entities/category.entity';
import { StockModule } from './stock/stock.module';
import { Stock } from './entities/stock.entity';
import { Purchase } from './entities/purchase.entity';
import { Seller } from './entities/seller.entity';
import { PurchaseModule } from './purchase/purchase.module';
import { SellerModule } from './seller/seller.module';
import { StatisticsModule } from './statistics/statistics.module';
import { Statistics } from './entities/statistics.entity';
import { SeedModule } from './seed/seed.module';
import { ConnectionModule } from './connection/connection.module';

@Module({
  imports: [
    ItemModule,
    CategoryModule,
    TypeOrmModule.forRoot({
      type: 'mysql',
      host: 'localhost',
      port: 3306,
      username: 'root', //Type your own username
      password: 'Sernik2025!', //Type your own password
      database: 'storage',
      entities: [Item, Category, Stock, Purchase, Seller, Statistics],
      synchronize: true,
      autoLoadEntities: true,
    }),
    StockModule,
    PurchaseModule,
    SellerModule,
    StatisticsModule,
    SeedModule,
    ConnectionModule,
  ],
  controllers: [AppController],
  providers: [AppService],
})
export class AppModule {
}
