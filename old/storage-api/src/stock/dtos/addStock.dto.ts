import { Entity } from "typeorm"

@Entity()
export class AddStockDto {
    itemId: number
    quantity: number
}