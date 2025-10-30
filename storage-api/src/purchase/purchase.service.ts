import { BadRequestException, HttpStatus, Inject, Injectable, Logger } from '@nestjs/common';
import { InjectRepository } from '@nestjs/typeorm';
import { Client } from 'src/entities/client.entity';
import { Purchase } from 'src/entities/purchase.entity';
import { Seller } from 'src/entities/seller.entity';
import { Repository } from 'typeorm';
import { CreatePurchaseDto } from './dtos/createPurchase.dto';
import { Item } from 'src/entities/Item.entity';
import { Stock } from 'src/entities/stock.entity';

@Injectable()
export class PurchaseService {

    constructor(
        @InjectRepository(Purchase) private purchaseRepo: Repository<Purchase>,
        @InjectRepository(Seller) private sellerRepo: Repository<Seller>,
        @InjectRepository(Item) private itemRepo: Repository<Item>,
        @InjectRepository(Stock) private stockRepo: Repository<Stock>,
    ) {}

        async findAllPurchases() {
            return await this.purchaseRepo.find({relations: ['seller', 'item']})
        }
    
        async createPurchase(createPurchaseDto: CreatePurchaseDto) {
            var purchase = {
                item: await this.itemRepo.findOne({where: {id: createPurchaseDto.itemId}}) as unknown as Item,
                quantity: createPurchaseDto.quantity,
                purchaseDate: new Date(),
                seller: await this.sellerRepo.findOne({where: {id: createPurchaseDto.sellerId}}) as unknown as Seller,
            }

            if(!purchase.item || !purchase.seller){
                throw new BadRequestException('Invalid item or seller ID');
            }

            try{
            var req = await this.purchaseRepo.create({...purchase, cost: await purchase.item.nettoPrice * purchase.quantity})

                this.stockRepo.findOne({where: {item: {id: purchase.item.id}}}).then(stock => {
                        this.stockRepo.update(purchase.item.id, {quantity : () => `quantity + ${purchase.quantity}`})
                        this.purchaseRepo.save(req)
                });

            }catch(err){
                Logger.error(err)
            }

            return await createPurchaseDto
        }
    
    
        async deletePurchase(purchase: Purchase) {
            try {
             await this.purchaseRepo.delete(purchase)
             return HttpStatus.OK
            }catch(err) {
                Logger.error(err)
                return HttpStatus.BAD_REQUEST
            }
        }

        async restockItemsPrice() {
            var purchases = this.purchaseRepo.find({relations: ['item']})
            purchases.then(res => {
                res.forEach(async purchase => {
                    purchase.cost = purchase.item.nettoPrice * purchase.quantity
                    await this.purchaseRepo.save(purchase)
                    var stock = await this.stockRepo.findOne({where: {item: {id: purchase.item.id}}})
                    if(stock) {
                        var totalQuantity = stock.quantity - purchase.quantity
                        await this.stockRepo.update(stock?.id || 0, {quantity: totalQuantity})
                    }
                })
            })
        }
    }
