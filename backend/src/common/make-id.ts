import * as short from 'short-uuid';
import { v4, v5 } from 'uuid';

const translator = short(short.constants.flickrBase58);

/**
 * A short, unique, user-friendly id
 * - generated from UUID and can be converted back to one
 *    translator.toUUID(shortId)
 */
export function makeId() {
    return translator.generate();
}

/**
 * A Random UUID
 */
export function makeUUID() {
    return v4();
}

/**
 * A non-random UUID
 */
export function computeUUID(s: string, ns: string = '59366cad-c208-4e9e-84aa-2bcfa06a89da') {
    return v5(s, ns);
}
