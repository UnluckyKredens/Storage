import { Injectable } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Stock } from 'src/entities/stock.entity';
import { Repository } from 'typeorm';
import { AddStockDto } from './dtos/addStock.dto';
import { Item } from 'src/entities/Item.entity';

@Injectable()
export class StockService {

    constructor(
        @InjectRepository(Stock) private stockRepo: Repository<Stock>,
        @InjectRepository(Item) private itemRepo: Repository<Item>
    ) {}

        GetAllStock() {
            return this.stockRepo.find({relations: ['item', 'item.category']})
        }
        
        async GetOneStock(itemId: number) {
            return await this.stockRepo.findOneByOrFail({item: {id: itemId}})
        }

        async AddItemToStock(stock: AddStockDto) {
            const newItem = await this.stockRepo.create(stock)
            return newItem
        }

        async editStock(stock: AddStockDto) {
            const newStock = await this.stockRepo.update(stock.itemId, stock) 

            return stock
        }

        async deleteStock( stock: AddStockDto) {
            return await this.stockRepo.delete(stock)
        }

        async cleanStock() {
            const newStock = await this.stockRepo.findBy({quantity: 0})
            await this.stockRepo.createQueryBuilder().delete().from(Stock).where('quantity = 0').execute()
            return newStock
        }
    

}
