import { User } from '~common/db/entities/user.entity';

export class UserStatisticsDto {
    level: number;
    xp: number;
    user: User;
}
