import { Controller, Get } from '@nestjs/common';

@Controller('connection')
export class ConnectionController {

    @Get()
    getStatus() {
        return { status: 'Connection is healthy' };
    }
}
