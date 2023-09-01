﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhisperCppLib;

internal class AsyncAutoResetEvent
{
    private static readonly Task Completed = Task.CompletedTask;
    private TaskCompletionSource<bool>? waitTcs;
    private int isSignaled; // 0 for false, 1 for true

    public Task WaitAsync()
    {
        if (Interlocked.CompareExchange(ref isSignaled, 0, 1) == 1)
        {
            return Completed;
        }
        else
        {
            var tcs = new TaskCompletionSource<bool>();
            var oldTcs = Interlocked.Exchange(ref waitTcs, tcs);
            oldTcs?.TrySetCanceled();
            return tcs.Task;
        }
    }

    public void Set()
    {
        var toRelease = Interlocked.Exchange(ref waitTcs, null);
        if (toRelease != null)
        {
            toRelease.SetResult(true);
        }
        else
        {
            Interlocked.Exchange(ref isSignaled, 1);
        }
    }
}