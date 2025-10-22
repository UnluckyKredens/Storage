import { Item } from "./item.model";

export interface Purchase {
  id: number;
  item: Item;
  quantity: number;
  totalPrice: number;
  purchaseDate: Date;
}
