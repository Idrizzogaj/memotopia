import * as os from 'os';

export function usage() {
    return {
        freemem: os.freemem(),
        totalmem: os.totalmem(),
        memory: process.memoryUsage(),
        cpu: process.cpuUsage(),
    };
}
