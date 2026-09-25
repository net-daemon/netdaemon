---
name: netdaemon-nuget-upgrade
description: Upgrade all or selected NuGet packages in net-daemon/netdaemon with dotnet-outdated, validate the full solution, and optionally publish a dependency-update PR with gh-axi. Use for routine NuGet refreshes, consolidated Dependabot replacements, and package-upgrade branches in the NetDaemon repository.
---

# NetDaemon NuGet Upgrade

## Purpose

Use this project-local workflow to upgrade NetDaemon NuGet packages and prove the resulting dependency set before publishing it.

## Ground Rules

- Work from the NetDaemon repo root unless the user gives another checkout.
- Inspect `git status` first and preserve unrelated work. When the user requests a branch without a worktree, work directly in the checkout and keep the dependency change on its own branch.
- Follow `AGENTS.md`: use `gh-axi` for GitHub work and never add an agent co-author.
- Use the existing repo label `pr: dependency-update` with the space after the colon. Do not create a duplicate no-space label.
- Upgrade every package reported by the dry-run when the user asks for all packages. Do not silently exclude MQTT or other package families. Call out major-version updates before applying them.
- Only commit, push, or open a PR when the user authorizes those actions.
- If the task includes merging a PR, ask the user immediately before each merge.

## Workflow

1. Fetch `origin`, confirm that the worktree is clean, update local `main` with a fast-forward only, and create a branch such as `codex/upgrade-nuget-packages`. Use a date or short suffix if that branch already exists.
2. Check the tool:
   ```bash
   dotnet outdated --version
   ```
   If `dotnet-outdated` is missing, install or restore it only after confirming the appropriate project-local/global approach.
   If .NET first-run output mentions `dotnet workload update`, treat that as informational; do not upgrade the SDK or workloads unless the user requests it. If an isolated execution environment fails while the user's CLI works, retry with normal local .NET/NuGet access before changing `global.json`, the SDK, or project files.
3. Run the dry-run exactly:
   ```bash
   dotnet outdated
   ```
   Review the proposed package IDs and highlight major-version changes.
4. Apply the update only after the dry-run is clean:
   ```bash
   dotnet outdated -u
   ```
5. Verify the package diff before testing:
   ```bash
   git diff --stat
   git diff --check
   ```
   Confirm that only intended project files and package versions changed.
6. Run the full test command:
   ```bash
   dotnet test NetDaemon.sln --configuration Release --logger "trx;LogFileName=netdaemon-tests.trx" --verbosity quiet
   ```
   If the only failure is the Docker-backed integration project reporting `DockerUnavailableException`, inactive Docker, or an unreachable Docker socket, treat it as a local environment limitation. The owner will handle Docker; do not block the dependency-update PR solely on that local Docker failure.
7. If Docker blocks local integration tests, still run the CI unit-test projects explicitly:
   ```bash
   dotnet test src/HassModel/NetDaemon.HassModel.Tests --no-build --configuration Release --logger "trx;LogFileName=unit-hassmodel.trx" --verbosity quiet
   dotnet test src/Extensions/NetDaemon.Extensions.Scheduling.Tests --no-build --configuration Release --logger "trx;LogFileName=unit-scheduling.trx" --verbosity quiet
   dotnet test src/Client/NetDaemon.HassClient.Tests --no-build --configuration Release --logger "trx;LogFileName=unit-hassclient.trx" --verbosity quiet
   dotnet test src/AppModel/NetDaemon.AppModel.Tests --no-build --configuration Release --logger "trx;LogFileName=unit-appmodel.trx" --verbosity quiet
   dotnet test src/Runtime/NetDaemon.Runtime.Tests --no-build --configuration Release --logger "trx;LogFileName=unit-runtime.trx" --verbosity quiet
   ```
   Fix any non-Docker test, build, lint, or diff-check failure before publishing.
8. Re-run the dry-run and require this result before publishing:
   ```bash
   dotnet outdated
   ```
   Confirm that it reports `No outdated dependencies were detected`.
9. Stage only intended project-file changes and any user-requested process files. Do not stage `TestResults` or other generated outputs. Then verify the staged diff:
   ```bash
   git diff --cached --check
   git diff --cached --stat
   ```
10. Commit with a plain message such as:
   ```bash
   git commit -m "Upgrade NuGet packages"
   ```
   Preserve repository signing behavior and verify the resulting commit signature and clean worktree before pushing.
11. Push the branch and create the PR with `gh-axi`:
   ```bash
   git push -u origin <branch>
   gh-axi pr create --title "Upgrade NuGet packages" --body-file <body-file> --base main --head <branch> --label "pr: dependency-update"
   ```
   Make the PR ready for review unless the user asks for a draft or a non-Docker validation issue remains.
12. Check initial CI:
    ```bash
    gh-axi pr checks <number>
    ```

## PR Body

Include:

- `dotnet outdated` dry-run completed successfully.
- `dotnet outdated -u` applied the upgrades.
- Test commands and outcomes.
- The post-upgrade `dotnet outdated` result.
- Any build warnings, even when they are in unchanged files and do not fail validation.
- A short note if local Docker prevented the integration project from running, while CI is expected to cover it.
