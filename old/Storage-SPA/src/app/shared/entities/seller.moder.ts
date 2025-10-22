import { Purchase } from "./purchase.model";

export interface Seller {
  id: number;
  name: string;
  nip: string;
  address: string;
  purchases: Purchase[];
}
