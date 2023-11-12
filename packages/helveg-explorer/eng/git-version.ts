import util from "util";
import childProcess from "child_process";
import yargs from "yargs";
import { hideBin } from "yargs/helpers";

const exec = util.promisify(childProcess.exec);

let version = "0.0.0-local";

const argv = yargs(hideBin(process.argv))
    .options({
        unset: { type: "boolean", default: false },
    }).parseSync();

if (!argv.unset) {
    const gitVersionPromise = exec("dotnet gitversion /showvariable SemVer");
    const gitVersion = await gitVersionPromise;
    if (gitVersionPromise.child.exitCode !== 0) {
        throw new Error(`GitVersion returned ${gitVersionPromise.child.exitCode}. Message:\n${gitVersion.stderr ?? gitVersion.stdout}`);
    }
    version = gitVersion.stdout.trim();
}

console.log(version);

const npmVersionPromise = exec(`npm version --allow-same-version --no-commit-hooks --no-git-tag-version "${version}"`);
const npmVersion = await npmVersionPromise;
if (npmVersionPromise.child.exitCode !== 0) {
    throw new Error(`npm version returned ${npmVersionPromise.child.exitCode}. Message:\n${npmVersion.stderr ?? npmVersion.stdout}`);
}
