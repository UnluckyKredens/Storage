import { Body, Controller, Delete, Get, Param, ParseIntPipe, Patch, Post, Query } from '@nestjs/common';
import { CategoryService } from './category.service';
import { AddCategoryDto } from './dtos/addCategory.dto';
import { Category } from 'src/entities/category.entity';

@Controller('category')
export class CategoryController {

    constructor(private readonly categoryService: CategoryService) {}
    
    @Get()
    findAllCategories() {
        return this.categoryService.findAllCategories()
    }

    @Get()
    findOneCategory(@Query('id', ParseIntPipe) id?: number, @Query("name") name?: string) {
        return this.categoryService.findOneCategory(id, name)
    }
   
    @Post()
    addCategory(@Body() name: AddCategoryDto) {
        return this.categoryService.addCategory(name)
    }

    @Patch()
    updateCategory(@Body() category: Category) {
        return this.categoryService.updateCategory(category)
    }

    @Delete()
   async deleteCategory(@Body() category: Category) {
        return await this.categoryService.deleteCategory(category)
    }

}
