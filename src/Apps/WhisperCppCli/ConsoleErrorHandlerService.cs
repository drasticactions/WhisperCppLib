// <copyright file="ConsoleErrorHandlerService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Diagnostics;
using Drastic.Services;

namespace WhisperCppCli;

public class ConsoleErrorHandlerService : IErrorHandlerService
{
    public void HandleError(Exception ex)
    {
#if DEBUG
        Debugger.Break();
#endif
    }
}
