import { Body, Controller, Delete, Get, HttpStatus, Logger, Param, ParseIntPipe, Patch, Post, Query } from '@nestjs/common';
import { Item } from 'src/entities/Item.entity';
import { ItemService } from './item.service';
import { AddItemDto } from './dtos/addItem.dto';

@Controller('item')
export class ItemController {

    constructor(private readonly itemService: ItemService) {}

    @Get() 
    getAllItems() {
        return this.itemService.getAllItems();
    }

    @Get()
    getItemById(@Query('id', ParseIntPipe) id: number, @Query('sku') sku: string) {
        return this.itemService.findOneItem(id, sku)
    }

    @Post()
    addItem(@Body() item: AddItemDto) {
        return this.itemService.addItem(item)
    }

    @Patch() 
    updateItem(@Body() item: Item) {
        return this.itemService.updateItem(item)
    }

    @Delete()
    async deleteItem(@Body() item: Item) {
        return await this.itemService.deleteItem(item)
        
    }
}
