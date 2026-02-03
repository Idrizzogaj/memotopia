import { ApiProperty } from '@nestjs/swagger';
import { IsString } from 'class-validator';

import { PaymentStatus } from '~common/constants/enums/payment-status';

export class PaymentStatusReqDto {
    @ApiProperty({ description: 'Pyament Status', nullable: true, enum: PaymentStatus })
    @IsString()
    paymentStatus: PaymentStatus;
}
