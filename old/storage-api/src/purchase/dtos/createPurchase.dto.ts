import { IsNumber } from "class-validator";

export class CreatePurchaseDto {
    @IsNumber()
    itemId: number;
    @IsNumber()
    quantity: number;
    @IsNumber()
    sellerId: number;

}
