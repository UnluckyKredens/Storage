import { Module } from '@nestjs/common';
import { SellerService } from './seller.service';
import { SellerController } from './seller.controller';
import { TypeOrmModule } from '@nestjs/typeorm';
import { Seller } from 'src/entities/seller.entity';
import { Purchase } from 'src/entities/purchase.entity';

@Module({
  imports: [TypeOrmModule.forFeature([Seller, Purchase])],
  providers: [SellerService],
  controllers: [SellerController]
})
export class SellerModule {}
