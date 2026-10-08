import { Column, Entity, ManyToOne, PrimaryGeneratedColumn } from "typeorm";
import { Purchase } from "./purchase.entity";

@Entity()
export class Seller {
    @PrimaryGeneratedColumn()
    id: number

    @Column()
    name: string

    @Column()
    nip: string

    @Column()
    adress: string

    @ManyToOne(() => Purchase, (Purchase) => Purchase.id)
    purchases: Purchase[]
}