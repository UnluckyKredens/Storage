import {IsDate, IsNumberString, isNumberString, isString, IsString, Length, maxLength, min, minLength } from "class-validator";

export class AddSellerDto {
    @IsString()
    name: string

    @Length(12)
    @IsNumberString()
    nip: string

    @IsString()
    adress: string;
}