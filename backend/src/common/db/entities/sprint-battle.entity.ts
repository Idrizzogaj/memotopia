import { Entity, Column, PrimaryGeneratedColumn, CreateDateColumn, UpdateDateColumn, ManyToOne, JoinColumn, Index } from 'typeorm';
import { User } from './user.entity';
import { SprintResult } from '~modules/challenge/sprint-rules';
export interface SprintLeg {
    game: string; seed: number; pacer: number; starts: { [user: string]: number };
    ready?: { [user: string]: number }; liveStart?:number; progress?: { [user:string]: {ms:number;units:number} };
    results: { [user: string]: SprintResult }; winner: number | null;
}
export interface SprintBattleState {
    reactions?: {sender:number;key:string;at:number}[];
    endReason?:string; forfeitedId?:number;
    invitation?: 'pending'|'accepted'|'declined'|'expired'|'blocked'|'withdrawn'; invitationExpires?:number; live?:boolean;
    version: number; mode: 'single' | 'match'; tier: number; legs: SprintLeg[];
    chooser: number | null; winner: number | null; done: boolean;
}
@Entity()
export class SprintBattle {
    @PrimaryGeneratedColumn() id: number;
    @Index('IDX_sprint_owner') @Column('int') ownerId: number;
    @Index('IDX_sprint_recipient') @Column('int') recipientId: number;
    @Column('varchar',{default:''}) ownerName:string;
    @Column('varchar',{default:''}) recipientName:string;
    @ManyToOne(()=>User,{onDelete:'CASCADE'}) @JoinColumn({name:'ownerId'}) owner:User;
    @ManyToOne(()=>User,{onDelete:'CASCADE'}) @JoinColumn({name:'recipientId'}) recipient:User;
    @Column('jsonb') state: SprintBattleState;
    @CreateDateColumn() createdAt: Date;
    @UpdateDateColumn() updatedAt: Date;
}
