/**
 * Replace the global beforeAll to show the error and quit
 */
if (!global._beforeAll) {
    global._beforeAll = global.beforeAll;
    global.beforeAll = (...args) => {
        global._beforeAll(async done => {
            try {
                await args[0]();
            } catch (e) {
                // the test will not fail if before all fails, so we need to .exit
                //  but the console.log is captured
                //  so we need to push to stderr, wait for it to write-out and then quit
                process.stderr.write(e.stack + '\n');
                process.exit(1);
            }
            done();
        }, args[1]);
    };
}

jest.setTimeout(1000 * 60 * 2);
