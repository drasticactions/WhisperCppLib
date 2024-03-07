// <copyright file="ISubtitleLine.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System;

namespace WhisperCppCli.Models
{
    /// <summary>
    /// Represents a subtitle line.
    /// </summary>
    public interface ISubtitleLine
    {
        /// <summary>
        /// Gets or sets the start time of the subtitle line.
        /// </summary>
        TimeSpan Start { get; set; }

        /// <summary>
        /// Gets or sets the end time of the subtitle line.
        /// </summary>
        TimeSpan End { get; set; }

        /// <summary>
        /// Gets or sets the text of the subtitle line.
        /// </summary>
        string Text { get; set; }
    }
}
