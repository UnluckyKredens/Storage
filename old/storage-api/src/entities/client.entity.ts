import { Column, Entity, ManyToMany, ManyToOne, PrimaryGeneratedColumn } from "typeorm";

@Entity()
export class Client {
    @PrimaryGeneratedColumn()
    id: number;

    @Column()
    name: string;

    @Column()
    nip?: string | null;

    @Column()
    email: string;

    @Column()
    address: string;

    @Column()
    phoneNumber: string;
}
