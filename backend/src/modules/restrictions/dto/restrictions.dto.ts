import { ApiProperty } from '@nestjs/swagger';

export class RestrictionsDto {
    @ApiProperty({ description: 'Current Date', nullable: false })
    currentDate: string;
}
