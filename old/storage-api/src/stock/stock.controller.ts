import { Body, Controller, Delete, Get, Param, ParseIntPipe, Patch, Post, Query } from '@nestjs/common';
import { Stock } from 'src/entities/stock.entity';
import { StockService } from './stock.service';
import { Item } from 'src/entities/Item.entity';
import { AddStockDto } from './dtos/addStock.dto';

@Controller('stock')
export class StockController {

    constructor(private readonly stockService: StockService) {}

    @Get()
    GetAllStock() {
      return this.stockService.GetAllStock()  
    }

    @Get()
    GetOneStock(@Param('item', ParseIntPipe) itemId: number) {
        return this.stockService.GetOneStock(itemId)
    }

    @Post()
    AddItemToStock(@Body() stock: AddStockDto) {
        return this.stockService.AddItemToStock(stock)
    }

    @Patch()
    editStock(@Body() stock: AddStockDto) {
        return this.stockService.editStock(stock)
    }

    @Delete()
    deleteStock(@Body() stock: AddStockDto) {
        return this.stockService.deleteStock(stock)
    }

    @Get('clean')
    cleanStock() {
        return this.stockService.cleanStock()
    }


}
