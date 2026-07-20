---
name: netdaemon-nuget-upgrade
description: NetDaemon project workflow for consolidating Dependabot and NuGet dependency updates with dotnet-outdated. Use when working in net-daemon/netdaemon to upgrade NuGet packages, run tests, and publish a dependency-update PR with gh-axi.
---

# NetDaemon NuGet Upgrade

## Purpose

Use this project-local workflow when upgrading NetDaemon NuGet packages, especially when replacing several Dependabot PRs with one dependency-update branch.

## Ground Rules

- Work from the NetDaemon repo root unless the user gives another checkout.
- Follow `AGENTS.md`: use `gh-axi` for GitHub work and never add an agent co-author.
- Use the existing repo label `pr: dependency-update` with the space after the colon. Do not create a duplicate no-space label.
- If the task includes merging a PR, ask the user immediately before each merge.

## Workflow

1. Start from current `main` and create a branch such as `codex/upgrade-nuget-packages`. Use a date or short suffix if that branch already exists.
2. Check the tool:
   ```bash
   dotnet outdated --version
   ```
   If `dotnet-outdated` is missing, install or restore it only after confirming the appropriate project-local/global approach.
3. Run the dry-run exactly:
   ```bash
   dotnet outdated
   ```
   Review the proposed package IDs.
4. Apply the update only after the dry-run is clean:
   ```bash
   dotnet outdated -u
   ```
5. Verify the package diff before testing:
   ```bash
   git diff --stat
   git diff --check
   ```
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
8. Stage only intended project-file changes and any user-requested skill/process files. Do not stage `TestResults` or other generated outputs.
9. Commit with a plain message such as:
   ```bash
   git commit -m "Upgrade NuGet packages"
   ```
10. Push the branch and create the PR with `gh-axi`:
   ```bash
   git push -u origin <branch>
   gh-axi pr create --title "Upgrade NuGet packages" --body-file <body-file> --base main --head <branch> --label "pr: dependency-update"
    ```
    Make the PR ready for review unless the user asks for a draft or a non-Docker validation issue remains.
11. Check initial CI:
    ```bash
    gh-axi pr checks <number>
    ```

## PR Body

Include:

- `dotnet outdated` dry-run completed successfully.
- `dotnet outdated -u` applied the upgrades.
- Test commands and outcomes.
- A short note if local Docker prevented the integration project from running, while CI is expected to cover it.
