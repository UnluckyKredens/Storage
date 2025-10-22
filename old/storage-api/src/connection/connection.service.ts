import { Get, Injectable } from '@nestjs/common';

@Injectable()
export class ConnectionService {

    @Get()
    getStatus() {
        return { status: 'Connection is healthy' };
    }
}
