import { Body, Controller, Delete, Get, ParseIntPipe, Patch, Post, Query } from '@nestjs/common';
import { AddSellerDto } from './dtos/add.seller.dto';
import { SellerService } from './seller.service';
import { Seller } from 'src/entities/seller.entity';

@Controller('seller')
export class SellerController {
    
    constructor(private readonly sellerService: SellerService) {}

    @Get()
    findAllSellers() {
        return this.sellerService.findAllSellers()
    }

    @Get('one')
    findOneSeller(@Query('id') id?: number, @Query('nip') nip?: string, @Query('name') name?: string) {
        return this.sellerService.findOneSeller(id, nip, name)
    }

    @Post('add')
    addSeller(@Body() seller: AddSellerDto) {
        return this.sellerService.addSeller(seller)
    }
    @Patch('update')
    updateSeller(@Body() seller: Seller) {
        return this.sellerService.updateSeller(seller)
    }
    @Delete('delete')
    deleteSeller(@Body() seller: Seller) {
        return this.sellerService.deleteSeller(seller)
    }
}
