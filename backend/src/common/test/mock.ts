import { DynamicModule } from '@nestjs/common/interfaces/modules/dynamic-module.interface';
import { ForwardReference } from '@nestjs/common/interfaces/modules/forward-reference.interface';
import { Type } from '@nestjs/common/interfaces/type.interface';
import { Test } from '@nestjs/testing';

import { configModuleFactory } from '~common/config';

export async function createTestingModule(
    imports: (Type<any> | DynamicModule | Promise<DynamicModule> | ForwardReference)[],
) {
    let mb = Test.createTestingModule({
        imports: [configModuleFactory(), ...imports],
    });

    return await mb.compile();
}

export function apiHeaders(more: object = {}) {
    return {
        accept: 'application/json',
        ...more,
    };
}
