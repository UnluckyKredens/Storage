import { Column, Entity, PrimaryColumn, PrimaryGeneratedColumn} from "typeorm";

@Entity()
export class Statistics {
    @PrimaryGeneratedColumn()
    id: number

    @Column()
    createdAt: Date

    @Column('int')
    totalItems: number

    @Column('int')
    totalStockEntries: number

    @Column('int')
    totalSellers: number

    @Column('int')
    totalPurchases: number
}