import { HttpException, HttpStatus, Injectable, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Item } from 'src/entities/Item.entity';
import { Repository } from 'typeorm';
import { AddItemDto } from './dtos/addItem.dto';
import { CategoryService } from 'src/category/category.service';
import { Category } from 'src/entities/category.entity';
import { ExceptionsHandler } from '@nestjs/core/exceptions/exceptions-handler';

@Injectable()
export class ItemService {

    constructor(
        @InjectRepository(Item) private readonly itemRepo: Repository<Item>, 
        @InjectRepository(Category) private categoryRepo: Repository<Category>) {}


        getAllItems() {
            return this.itemRepo.find({relations: ['category']});
        }
    
        async findOneItem(id?: number, sku?: string) {
            const where = id ? {id} : sku ? {sku} : undefined;
            const res =  await this.categoryRepo.findOne({where})
      
            if(!res) {
                  throw new NotFoundException()
            }
      
            return res 
        }

        async addItem(item: AddItemDto) {
            const category = await this.categoryRepo.findOneByOrFail({id: item.categoryId})

            const newItem = {name: item.name, sku: item.sku, bruttoPrice: item.bruttoPrice, nettoPrice: item.nettoPrice, category}
            this.itemRepo.create(newItem)
            this.itemRepo.save(newItem)

            return {newItem}

        }
    
        async updateItem(item: Item) {
            const newItem = await this.itemRepo.update(item.id, item)
    
            return  newItem
        }
    
        async deleteItem(item: Item) {
            try {
                await this.itemRepo.delete(item.id)
                return HttpStatus.OK
            } catch (err) {
                throw new HttpException('Cannot delete', HttpStatus.BAD_REQUEST)
            }
        }
}
