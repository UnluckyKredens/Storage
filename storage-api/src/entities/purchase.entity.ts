import { Column, Entity, ManyToOne, PrimaryGeneratedColumn } from "typeorm";
import { Item } from "./Item.entity";

@Entity()
export class Purchase {
@PrimaryGeneratedColumn()
id: number;
@ManyToOne(() => Item)
item: Item;

@Column()
quantity: number;

@Column()
purchaseDate: Date;
}