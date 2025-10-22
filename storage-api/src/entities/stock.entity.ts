import { Column, Entity, JoinColumn, JoinTable, ManyToOne, PrimaryGeneratedColumn } from "typeorm";
import { Item } from "./Item.entity";
import { Category } from "./category.entity";

@Entity()
export class Stock {
    @PrimaryGeneratedColumn()
    id: number
    @ManyToOne(() => Item, (item) => item.Stocks, {nullable: false, onDelete: 'RESTRICT'})
    item: Item

    @Column()
    quantity: number
}