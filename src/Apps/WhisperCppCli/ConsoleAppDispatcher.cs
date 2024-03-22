// <copyright file="ConsoleAppDispatcher.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using Drastic.Services;

namespace WhisperCppCli;

internal class ConsoleAppDispatcher : IAppDispatcher
{
    public bool Dispatch(Action action)
    {
        action.Invoke();
        return true;
    }
}
