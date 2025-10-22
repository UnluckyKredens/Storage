import { Item } from "./item.model";
import { Seller } from "./seller.moder";

export interface Purchase {
  id: number;
  item: Item;
  quantity: number;
  totalPrice: number;
  purchaseDate: Date;
  seller: Seller
}
