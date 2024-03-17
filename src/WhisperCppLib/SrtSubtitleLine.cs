// <copyright file="SrtSubtitleLine.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

#nullable disable

using System;
using System.IO;
using System.Text;

namespace WhisperCppLib
{
    /// <summary>
    /// Represents a subtitle line in SRT format.
    /// </summary>
    public class SrtSubtitleLine : ISubtitleLine
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SrtSubtitleLine"/> class.
        /// </summary>
        public SrtSubtitleLine()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SrtSubtitleLine"/> class with the specified subtitle text.
        /// </summary>
        /// <param name="subtitleText">The subtitle text.</param>
        public SrtSubtitleLine(string subtitleText)
        {
            subtitleText = subtitleText.Trim();
            using (StringReader data = new StringReader(subtitleText))
            {
                LineNumber = int.Parse(data.ReadLine().Trim());

                string secondLine = data.ReadLine();
                Start = TimeSpan.ParseExact(secondLine.Substring(0, 12), @"hh\:mm\:ss\,fff", null);
                End = TimeSpan.ParseExact(secondLine.Substring(17, 12), @"hh\:mm\:ss\,fff", null);

                Text = data.ReadToEnd().Trim();
            }
        }

        /// <inheritdoc/>
        public TimeSpan Start { get; set; }

        /// <inheritdoc/>
        public TimeSpan End { get; set; }

        /// <summary>
        /// Gets the time range of the subtitle line.
        /// </summary>
        public string Time => $"{Start} -> {End}";

        /// <inheritdoc/>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets the line number of the subtitle line.
        /// </summary>
        public int LineNumber { get; set; }

        /// <summary>
        /// Gets or sets the image data of the subtitle line.
        /// </summary>
        public byte[] Image { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(LineNumber.ToString());

            sb.Append(Start.ToString(@"hh\:mm\:ss\,fff"));
            sb.Append(" --> ");
            sb.Append(End.ToString(@"hh\:mm\:ss\,fff"));
            sb.AppendLine();

            sb.Append(Text);

            return sb.ToString();
        }
    }
}
