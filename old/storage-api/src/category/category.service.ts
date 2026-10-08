import { HttpException, HttpStatus, Injectable, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Category } from 'src/entities/category.entity';
import { Repository } from 'typeorm';
import { AddCategoryDto } from './dtos/addCategory.dto';
import { error } from 'console';

@Injectable()
export class CategoryService {

    constructor(@InjectRepository(Category) private categoryRepo: Repository<Category>) {}


   findAllCategories() {
        return this.categoryRepo.find()
   }

   async findOneCategory(id?: number, name?: string) {
      const where = id ? {id} : name ? {name} : undefined;
              const res =  await this.categoryRepo.findOne({where})
      
              if(!res) {
                  throw new NotFoundException()
              }
      
              return res 
   }

   async addCategory(name: AddCategoryDto) {
        const newCategory = this.categoryRepo.create({...name})
        await this.categoryRepo.save(newCategory)
        return await this.findOneCategory(undefined, newCategory.name)
   }

   updateCategory(category: Category) {
      const updateCategory = this.categoryRepo.update(category.id, category);

      return updateCategory
   }

   deleteCategory(category: Category) {
      try {
      this.categoryRepo.delete(category)
      return HttpStatus 
      }catch {
         return new HttpException('Cannot delete: ' + error, 400)
      }
   }



}
