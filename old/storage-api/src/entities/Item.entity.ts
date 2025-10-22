import { Column, Entity, JoinColumn, ManyToOne, OneToMany, PrimaryGeneratedColumn } from "typeorm";
import { Category } from "./category.entity";
import { Stock } from "./stock.entity";

@Entity()
export class Item {
   @PrimaryGeneratedColumn()
   id: number;
   
   @Column()
   name: string;
    
   @Column()
   sku: string;

   @ManyToOne(() => Category, (cat) => cat.items, {nullable: false, onDelete: 'RESTRICT'})
   category: Category

   @OneToMany(() => Stock, (st) => st.item)
   @JoinColumn()
   Stocks: Stock

   @Column('decimal', { precision: 10, scale: 2 })
   bruttoPrice: number

   @Column('decimal', { precision: 10, scale: 2 })
   nettoPrice: number
}