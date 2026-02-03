import {
    ConflictException,
    Injectable,
    InternalServerErrorException,
    NotFoundException,
} from '@nestjs/common';
import { Not } from 'typeorm';

import { ChallengeStatus } from '~common/constants/enums/challenge-status.enum';
import { ParticipantStatus } from '~common/constants/enums/participant.enum';
import { Challenge } from '~common/db/entities/challenge.entity';
import { Participant } from '~common/db/entities/participant.entity';
import { UserStatistics } from '~common/db/entities/user-statistics.entity';
import { User } from '~common/db/entities/user.entity';
import { achievementConstants } from '~modules/achievements/achievements.constants';
import { AchievementsService } from '~modules/achievements/achievements.service';
import { GameRepository } from '~modules/level/game.repository';
import { UserStatisticsService } from '~modules/user-statistics/user-statistics.service';
import { UserService } from '~modules/user/user.service';

import { ChallengeRepository } from './challenge.repository';
import { ChallengeResDto } from './dto/challenge-res.dto';
import { CreateChallengeDto } from './dto/create-challenge.dto';
import { GetChallengeDto } from './dto/get-challenge.dto';
import { GetHistoryRes } from './dto/get-history-res.dto';
import { PaginationChallengeDto } from './dto/pagination-challenge.dto';
import { UpdateChallengeDto } from './dto/update-challenge.dto';
import { ParticipantRepository } from './participant.repository';

@Injectable()
export class ChallengeService {
    constructor(
        private challengeRepository: ChallengeRepository,
        private participantRepository: ParticipantRepository,
        private gameRepository: GameRepository,
        private userService: UserService,
        private userStatisticsService: UserStatisticsService,
        private achievementsService: AchievementsService,
    ) {}

    async create(createChallengeDto: CreateChallengeDto, user: User): Promise<ChallengeResDto> {
        const { level, challengerId, gameName } = createChallengeDto;
        const game = await this.gameRepository.findOne({ gameName });

        if (!game) throw new NotFoundException(`Game with name "${gameName}" not found!`);

        const challenge = new Challenge();

        challenge.level = level;
        challenge.game = game;

        try {
            await this.challengeRepository.save(challenge);
        } catch (error) {
            throw new InternalServerErrorException('Challenge failed to be created.');
        }

        const challenger = await this.userService.getById(challengerId);
        const participant1 = new Participant();
        const participant2 = new Participant();

        participant1.user = user;
        participant1.challenge = challenge;

        participant2.user = challenger as User;
        participant2.challenge = challenge;

        let participants;

        try {
            participants = await this.participantRepository.save([participant1, participant2]);
        } catch (error) {
            throw new InternalServerErrorException('Participant failed to be created.');
        }

        return {
            challenge,
            participants,
        };
    }

    async update(updateChallengeDto: UpdateChallengeDto, user: User): Promise<ChallengeResDto> {
        const { score, challengeId, status } = updateChallengeDto;

        const challenge = await this.challengeRepository.findOne({ id: challengeId });

        if (!challenge)
            throw new NotFoundException(`Challenge with id "${challengeId}" not found!`);

        const participant = await this.participantRepository.findOne({
            user: user,
            challenge: challenge,
        });

        if (!participant || participant.status !== ParticipantStatus.PENDING)
            throw new ConflictException('Cannot update this challenge');

        if (status === ParticipantStatus.CANCELED) {
            participant.status = ParticipantStatus.CANCELED;
        } else {
            participant.score = score;
            participant.status = ParticipantStatus.DONE;
        }

        let participants;

        try {
            participants = await this.participantRepository.save(participant);
        } catch (error) {
            throw new InternalServerErrorException('Participant failed to be updated');
        }

        const challenger = await this.participantRepository.findOne({
            where: { challenge: challenge, user: Not(user.id) },
            relations: ['user'],
        });

        if (
            participant.status == ParticipantStatus.DONE &&
            challenger.status == ParticipantStatus.DONE
        ) {
            let participantStatistics = await this.userStatisticsService.getByUser(user);
            let challengerStatistics = await this.userStatisticsService.getByUser(challenger.user);

            let winner: Participant;
            let mostXp: Participant;
            const difference: number = Math.abs(participantStatistics.xp - challengerStatistics.xp);

            if (participant.score > challenger.score) winner = participant;
            else if (participant.score < challenger.score) winner = challenger;

            if (participantStatistics.xp > challengerStatistics.xp) mostXp = participant;
            else if (participantStatistics.xp < challengerStatistics.xp) mostXp = challenger;

            if (difference < 100) {
                if (!winner) {
                    if (mostXp == participant) {
                        participantStatistics.xp += 1;
                        challengerStatistics.xp += 2;
                    } else if (mostXp == challenger) {
                        participantStatistics.xp += 2;
                        challengerStatistics.xp += 1;
                    }
                } else {
                    if (winner == participant) {
                        if (mostXp == participant) {
                            participantStatistics.xp += 15;
                            challengerStatistics.xp += 1;
                        } else if (mostXp == challenger) {
                            participantStatistics.xp += 16;
                            challengerStatistics.xp += 0;
                        }
                    } else if (winner == challenger) {
                        if (mostXp == participant) {
                            participantStatistics.xp += 0;
                            challengerStatistics.xp += 16;
                        } else if (mostXp == challenger) {
                            participantStatistics.xp += 1;
                            challengerStatistics.xp += 15;
                        }
                    }
                }
            } else if (difference >= 100 && difference < 200) {
                if (!winner) {
                    if (mostXp == participant) {
                        participantStatistics.xp += 1;
                        challengerStatistics.xp += 2;
                    } else if (mostXp == challenger) {
                        participantStatistics.xp += 2;
                        challengerStatistics.xp += 1;
                    }
                } else {
                    if (winner == participant) {
                        if (mostXp == participant) {
                            participantStatistics.xp += 10;
                            challengerStatistics.xp += 1;
                        } else if (mostXp == challenger) {
                            participantStatistics.xp += 21;
                            challengerStatistics.xp += 0;
                        }
                    } else if (winner == challenger) {
                        if (mostXp == participant) {
                            participantStatistics.xp += 0;
                            challengerStatistics.xp += 21;
                        } else if (mostXp == challenger) {
                            participantStatistics.xp += 1;
                            challengerStatistics.xp += 10;
                        }
                    }
                }
            } else if (difference >= 200 && difference < 300) {
                if (!winner) {
                    if (mostXp == participant) {
                        participantStatistics.xp += 1;
                        challengerStatistics.xp += 2;
                    } else if (mostXp == challenger) {
                        participantStatistics.xp += 2;
                        challengerStatistics.xp += 1;
                    }
                } else {
                    if (winner == participant) {
                        if (mostXp == participant) {
                            participantStatistics.xp += 5;
                            challengerStatistics.xp += 1;
                        } else if (mostXp == challenger) {
                            participantStatistics.xp += 26;
                            challengerStatistics.xp += 0;
                        }
                    } else if (winner == challenger) {
                        if (mostXp == participant) {
                            participantStatistics.xp += 0;
                            challengerStatistics.xp += 26;
                        } else if (mostXp == challenger) {
                            participantStatistics.xp += 1;
                            challengerStatistics.xp += 5;
                        }
                    }
                }
            } else {
                if (!winner) {
                    if (mostXp == participant) {
                        participantStatistics.xp += 1;
                        challengerStatistics.xp += 2;
                    } else if (mostXp == challenger) {
                        participantStatistics.xp += 2;
                        challengerStatistics.xp += 1;
                    }
                } else {
                    if (winner == participant) {
                        if (mostXp == participant) {
                            participantStatistics.xp += 5;
                            challengerStatistics.xp += 1;
                        } else if (mostXp == challenger) {
                            participantStatistics.xp += 31;
                            challengerStatistics.xp += 0;
                        }
                    } else if (winner == challenger) {
                        if (mostXp == participant) {
                            participantStatistics.xp += 0;
                            challengerStatistics.xp += 31;
                        } else if (mostXp == challenger) {
                            participantStatistics.xp += 1;
                            challengerStatistics.xp += 5;
                        }
                    }
                }
            }

            let participantChallengeStatus: string;
            let challengerChallengeStatus: string;

            if (!winner) {
                participantChallengeStatus = ChallengeStatus.DRAW;
                challengerChallengeStatus = ChallengeStatus.DRAW;
            } else if (winner == challenger) {
                participantChallengeStatus = ChallengeStatus.LOSS;
                challengerChallengeStatus = ChallengeStatus.WON;
            } else if (winner == participant) {
                participantChallengeStatus = ChallengeStatus.WON;
                challengerChallengeStatus = ChallengeStatus.LOSS;
            }

            try {
                participantStatistics = await this.userStatisticsService.levelUp(
                    participantStatistics,
                );
                const participantUserStatistics = await this.userStatisticsService.update({
                    level: participantStatistics.level,
                    xp: participantStatistics.xp,
                    user: user,
                });

                await this.userStatisticsService.increseStatisticValues(
                    participantUserStatistics.id,
                    participantChallengeStatus,
                );
            } catch (error) {
                throw new InternalServerErrorException(
                    'Participant statistics failed to update, on challenge update.',
                );
            }

            try {
                challengerStatistics = await this.userStatisticsService.levelUp(
                    challengerStatistics,
                );
                const challengerUserStatistics = await this.userStatisticsService.update({
                    level: challengerStatistics.level,
                    xp: challengerStatistics.xp,
                    user: challenger.user,
                });
                await this.userStatisticsService.increseStatisticValues(
                    challengerUserStatistics.id,
                    challengerChallengeStatus,
                );
            } catch (error) {
                throw new InternalServerErrorException(
                    'Challenger statistics failed to update, on challenge update.',
                );
            }

            // get number of challenges that are done for the challenger
            const challenges = await this.getHistory(
                {
                    limit: 20,
                    page: 1,
                },
                challenger.user,
            );
            const wonChallenges = challenges.filter(c => c.winOrLose == 'Win');
            
            // if number of challenges is one add the achievement of first challenge
            if (wonChallenges.length == 1) {
                await this.achievementsService.create(
                    { achievements: [achievementConstants[10]] },
                    challenger.user,
                );
            }

            // if number of challenges is ten add the achievemnet of ten challenges
            if (wonChallenges.length == 10) {
                await this.achievementsService.create(
                    { achievements: [achievementConstants[11]] },
                    challenger.user,
                );
            }
        } else {
            let userStatistics: UserStatistics = await this.userStatisticsService.getByUser(user);
            userStatistics.xp += 1;

            try {
                userStatistics = await this.userStatisticsService.levelUp(userStatistics);
                await this.userStatisticsService.update({
                    level: userStatistics.level,
                    xp: userStatistics.xp,
                    user,
                });
            } catch (error) {
                throw new InternalServerErrorException(
                    'User statistics failed to be updated on challenge update.',
                );
            }
        }

        const loggedinUser: User = await this.userService.getUserEntityById(user.id);

        return {
            challenge,
            participants: [participants],
            user: loggedinUser,
        };
    }

    async getChallenges(
        getChallengeDto: GetChallengeDto,
        user: User,
    ): Promise<PaginationChallengeDto> {
        try {
            return await this.participantRepository.findChallenges(
                getChallengeDto,
                user,
                ParticipantStatus.PENDING,
                ParticipantStatus.DONE,
            );
        } catch (err) {
            return {
                results: [],
                limit: 20,
                page: 1,
                total: 0,
            };
        }
    }

    async getHistory(getChallengeDto: GetChallengeDto, user: User): Promise<GetHistoryRes[]> {
        // TODO: Remove old challenges from the history

        let paginationChallenges;

        try {
            paginationChallenges = await this.participantRepository.findChallenges(
                getChallengeDto,
                user,
                ParticipantStatus.DONE,
                ParticipantStatus.DONE,
            );
        } catch (err) {
            return [];
        }
        const challenges: Challenge[] = paginationChallenges.results.filter(
            i => i.participants.length > 1,
        );
        const result: GetHistoryRes[] = [];

        for (const challenge of challenges) {
            let challenger: Participant;
            let me: Participant;
            let winOrLose: string;

            for (const participant of challenge.participants) {
                if (participant.user.id == user.id) me = participant;
                else challenger = participant;
            }

            if (me && me.status == ParticipantStatus.DONE) {
                if (challenger.status == ParticipantStatus.PENDING) {
                    winOrLose = 'Pending';
                } else {
                    if (me.score == challenger.score) winOrLose = 'Draw';
                    else if (me.score > challenger.score) winOrLose = 'Win';
                    else winOrLose = 'Loss';
                }

                result.push({
                    challenger: challenger.user,
                    winOrLose,
                    gameName: challenge.game.gameName,
                    level: challenge.level,
                });
            }
        }

        return result;
    }

    // async addAchievements(): Promise<void> {
    //     const users: User[] = await this.userService.getAll();

    //     for (const user of users) {
    //         console.log(user.id);
            
    //         const challenges = await this.getHistory(
    //             {
    //                 limit: 20,
    //                 page: 1,
    //             },
    //             user
    //         );
    //         const wonChallenges = challenges.filter(c => c.winOrLose == 'Win');
    
    //         // if number of challenges is one add the achievement of first challenge
    //         if (wonChallenges.length >= 1) {
    //             console.log(`Added ${achievementConstants[10]} to ${user.id}`);
                
    //             await this.achievementsService.create(
    //                 { achievements: [achievementConstants[10]] },
    //                 user,
    //             );
    //         }
    
    //         // if number of challenges is ten add the achievemnet of ten challenges
    //         if (wonChallenges.length >= 10) {
    //             console.log(`Added ${achievementConstants[11]} to ${user.id}`);
    
    //             await this.achievementsService.create(
    //                 { achievements: [achievementConstants[11]] },
    //                 user,
    //             );
    //         }
    //     }

    //     console.log("DONE");
        
    // }
}
