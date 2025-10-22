import { Body, Controller, Delete, Get, Patch, Post } from '@nestjs/common';
import { PurchaseService } from './purchase.service';
import { CreatePurchaseDto } from './dtos/createPurchase.dto';
import { Purchase } from 'src/entities/purchase.entity';

@Controller('purchase')
export class PurchaseController {

    constructor(private readonly purchaseService: PurchaseService) {}

    @Get()
    findAllPurchases() {
        return this.purchaseService.findAllPurchases();
    }

    @Post()
    createPurchase(@Body() createPurchaseDto: CreatePurchaseDto) {
        return this.purchaseService.createPurchase(createPurchaseDto);
    }

    @Delete()
    deletePurchase() {
        // Implementation for deleting a purchase
    }

    @Post('restock')
    restockItemsPrice() {
        return this.purchaseService.restockItemsPrice();
    }
    
}
