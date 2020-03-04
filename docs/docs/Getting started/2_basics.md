---
title: Basics
---
# Basics

## The file structure

All automation files is in the `netdaemon` folder directly under your configuration folder. Typically you access these files within vscode or any other editor. This should be your root directory on you hassio share in vscode:

![](img/rootdir.png)

## Get intellisense

Before coding, run the `dotnet restore` to get intellisense.

## Create new app

Automations in NetDaemon is created in apps. All apps inherit from the base class `NetDaemonApp`.

All apps have to implement and override the function:

```c#
public async override Task InitializeAsync()
```

This function is called by the daemon at start up. Never block this function! Make sure you run your initializations and return.

Example of an initialization using the fluent API:

```c#
public async override Task InitializeAsync()
{

    Entity("binary_sensor.my_pir")
        .WhenStateChange()
            .Call(OnPirChanged)
    .Execute();
}

```

This initialize the app to call a function called `OnPirChanged` when ever the entity `binary_sensor.my_pir` change state.

More of this in the example app.

## Code snippets

I provide some code snippets to create a new app. Check it out.. In vscode, in the c# code file press `ctrl+space` and select.. More will come later.

## Async model

NetDaemon is built on .NET and c# async model. It is important that you read up on async programming model. But here is some basics!

### Use the await keyword
Whenever you see a function return a `Task` and mostly these functions has the postfix `Async`. Use the keyword `await` before calling. Example using the fluent API below:

```c#
private async Task MyAsyncFunctionDoingStuff()
{
    await MediaPlayer("media_player.cool_player")
                .Pause().ExecuteAsync();
}
```

Remember that the function needs to be async containing this call as the example shows.

### Do not use Thread.Sleep()

!!! danger
    Never use `Thread.Sleep();`! It is very important that you never block async operations. Use the `await Task.Delay();` instead if you need to pause execution.

