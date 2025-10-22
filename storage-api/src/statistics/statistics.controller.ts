import { Controller, Get } from '@nestjs/common';
import { StatisticsService } from './statistics.service';
import { get } from 'http';

@Controller('statistics')
export class StatisticsController {

    constructor(private readonly statisticsService: StatisticsService) {}
    
    @Get('all')
    getAllStatistics() {
        return this.statisticsService.getAllStatistics()
    }

    @Get('history')
    getStatisticsHistory() {
        return this.statisticsService.getStatisticsHistory()
    }
}
