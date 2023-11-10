import util from "util";
import childProcess from "child_process";

const exec = util.promisify(childProcess.exec);

const gitVersionPromise = exec("dotnet gitversion /showvariable SemVer");
const gitVersion = await gitVersionPromise;
if (gitVersionPromise.child.exitCode !== 0) {
    throw new Error(`GitVersion returned ${gitVersionPromise.child.exitCode}. Message:\n${gitVersion.stderr ?? gitVersion.stdout}`);
}

const version = gitVersion.stdout.trim();
console.log(version);

const npmVersionPromise = exec(`npm version --allow-same-version --no-commit-hooks --no-git-tag-version "${version}"`);
const npmVersion = await npmVersionPromise;
if (npmVersionPromise.child.exitCode !== 0) {
    throw new Error(`npm version returned ${npmVersionPromise.child.exitCode}. Message:\n${npmVersion.stderr ?? npmVersion.stdout}`);
}
