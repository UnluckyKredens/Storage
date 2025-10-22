import { Injectable, Logger, NotFoundException } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Seller } from 'src/entities/seller.entity';
import { Repository } from 'typeorm';
import { AddSellerDto } from './dtos/add.seller.dto';
import { Category } from 'src/entities/category.entity';

@Injectable()
export class SellerService {

    constructor(@InjectRepository(Seller) private sellerRepo: Repository<Seller>) {}

    async findAllSellers() {
        return (await this.sellerRepo.find());
    }

    async findOneSeller(id?: number, nip?: string, name?: string) {
        Logger.log({id, nip, name})
        const where = id ? {id} : name ? {name} : nip ? {nip} : undefined;
        const res =  await this.sellerRepo.findOne({where})

        if(!res) {
            throw new NotFoundException()
        }

        return res 
    }

    async addSeller(dto: AddSellerDto) {
        const newSeller = await this.sellerRepo.create({...dto})
                Logger.log(newSeller)

        this.sellerRepo.save(newSeller)
        return this.findOneSeller(undefined, newSeller.nip)
    }

    async updateSeller(seller: Seller) {
        const updatedSeller = await this.sellerRepo.update(seller.id, seller)
        return this.findOneSeller(seller.id)
    }

    deleteSeller(seller: Seller) {
        return this.sellerRepo.delete(seller)
    }

}
