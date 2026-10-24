import { Entity, PrimaryColumn, Column, ManyToOne, JoinColumn } from 'typeorm';
import { User } from './user.entity';
@Entity('player_social')
export class PlayerSocial {
    @PrimaryColumn('int') userId:number;
    @Column({default:false}) visible:boolean;
    @Column({default:true}) friendsVisible:boolean;
    @Column('bigint',{default:0}) onlineUntil:number;
    @Column('int',{default:0}) declined:number;
    @Column('bigint',{default:0}) friendWindow:number;
    @Column('int',{default:0}) friendRequests:number;
    @ManyToOne(()=>User,{onDelete:'CASCADE'}) @JoinColumn({name:'userId'}) user:User;
}
@Entity('player_block')
export class PlayerBlock {
    @PrimaryColumn('int') ownerId:number;
    @PrimaryColumn('int') targetId:number;
    @ManyToOne(()=>User,{onDelete:'CASCADE'}) @JoinColumn({name:'ownerId'}) owner:User;
    @ManyToOne(()=>User,{onDelete:'CASCADE'}) @JoinColumn({name:'targetId'}) target:User;
}

@Entity('player_friend')
export class PlayerFriend {
    @PrimaryColumn('int') lowId:number;
    @PrimaryColumn('int') highId:number;
    @Column('int') requesterId:number;
    @Column('boolean',{default:false}) accepted:boolean;
    @Column('boolean',{default:false}) declined:boolean;
    @Column('bigint') requestedAt:number;
    @ManyToOne(()=>User,{onDelete:'CASCADE'}) @JoinColumn({name:'lowId'}) low:User;
    @ManyToOne(()=>User,{onDelete:'CASCADE'}) @JoinColumn({name:'highId'}) high:User;
}
