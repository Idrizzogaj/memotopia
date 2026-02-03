import { Module } from '@nestjs/common';

import { LoggerService } from './logger.service';

@Module({
    providers: [LoggerService],
    imports: [],
    exports: [LoggerService],
})
export class LoggerModule {}
