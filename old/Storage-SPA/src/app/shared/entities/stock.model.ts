import { Item } from "./item.model";

export interface Stock {
  id: number;
  item: Item;
  quantity: number;
}
