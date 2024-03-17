// <copyright file="ISubtitle.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;

namespace WhisperCppLib
{
    /// <summary>
    /// Represents a subtitle.
    /// </summary>
    public interface ISubtitle
    {
        /// <summary>
        /// Gets or sets the lines of the subtitle.
        /// </summary>
        List<ISubtitleLine> Lines { get; set; }
    }
}
