import { Category } from "./category.model";

export interface Item {
  id: number,
  name: string,
  sku: string,
  category: Category,
  bruttoPrice: number,
  nettoPrice: number
}
