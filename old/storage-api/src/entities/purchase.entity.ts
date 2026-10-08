import { Column, Entity, ManyToOne, OneToMany, PrimaryGeneratedColumn } from "typeorm";
import { Item } from "./Item.entity";
import { Client } from "./client.entity";
import { Seller } from "./seller.entity";

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

@Column()
cost: number;

@ManyToOne(() => Seller)
seller: Seller;
}