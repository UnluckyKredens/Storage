import { Category } from "src/entities/category.entity";

export class AddItemDto {

    name: string;
    sku: string;
    categoryId: number;
    bruttoPrice: number;
    nettoPrice: number;
}