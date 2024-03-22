// <copyright file="WhisperDownload.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.ComponentModel;
using System.Runtime.CompilerServices;
using Downloader;
using Drastic.Services;
using Drastic.Tools;
using WhisperCppGui.Services;

namespace WhisperCppGui.Models;

/// <summary>
/// Represents a Whisper download.
/// </summary>
public class WhisperDownload : INotifyPropertyChanged, IDisposable, IErrorHandlerService
{
    private DownloadService download;
    private CancellationTokenSource source;
    private bool downloadStarted;
    private double precent;
    private bool disposedValue;
    private IAppDispatcher dispatcher;
    private WhisperModelService service;

    /// <summary>
    /// Initializes a new instance of the <see cref="WhisperDownload"/> class.
    /// </summary>
    /// <param name="model">The Whisper model.</param>
    /// <param name="service">The Whisper model service.</param>
    /// <param name="dispatcher">The application dispatcher.</param>
    public WhisperDownload(WhisperModel model, WhisperModelService service, IAppDispatcher dispatcher)
    {
        this.service = service;
        this.dispatcher = dispatcher;
        this.Model = model;

        this.download = new DownloadService(new DownloadConfiguration()
        {
            ChunkCount = 8,
            ParallelDownload = true,
        });

        this.download.DownloadStarted += this.Download_DownloadStarted;
        this.download.DownloadFileCompleted += this.Download_DownloadFileCompleted;
        this.download.DownloadProgressChanged += this.Download_DownloadProgressChanged;

        this.source = new CancellationTokenSource();

        this.DownloadCommand = new AsyncCommand(this.DownloadAsync, () => !string.IsNullOrEmpty(this.Model.DownloadUrl), this.dispatcher, this);
        this.CancelCommand = new AsyncCommand(this.CancelAsync, null, this.dispatcher, this);
        this.DeleteCommand = new AsyncCommand(this.DeleteAsync, null, this.dispatcher, this);
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the Whisper model.
    /// </summary>
    public WhisperModel Model { get; private set; }

    /// <summary>
    /// Gets the download service.
    /// </summary>
    public DownloadService DownloadService => this.download;

    /// <summary>
    /// Gets or sets the download progress in percentage.
    /// </summary>
    public double Precent
    {
        get { return this.precent; }
        set { this.SetProperty(ref this.precent, value); }
    }

    /// <summary>
    /// Gets or sets a value indicating whether the download has started.
    /// </summary>
    public bool DownloadStarted
    {
        get { return this.downloadStarted; }
        set { this.SetProperty(ref this.downloadStarted, value); }
    }

    /// <summary>
    /// Gets a value indicating whether to show the download button.
    /// </summary>
    public bool ShowDownloadButton => !this.Model.Exists && !this.DownloadStarted;

    /// <summary>
    /// Gets a value indicating whether to show the cancel button.
    /// </summary>
    public bool ShowCancelButton => this.DownloadStarted;

    /// <summary>
    /// Gets a value indicating whether to show the delete button.
    /// </summary>
    public bool ShowDeleteButton => this.Model.Exists && !this.DownloadStarted;

    /// <summary>
    /// Gets the download command.
    /// </summary>
    public AsyncCommand DownloadCommand { get; }

    /// <summary>
    /// Gets the cancel command.
    /// </summary>
    public AsyncCommand CancelCommand { get; }

    /// <summary>
    /// Gets the delete command.
    /// </summary>
    public AsyncCommand DeleteCommand { get; }

    /// <inheritdoc/>
    public void HandleError(Exception ex)
    {
    }

    /// <inheritdoc/>
    void IDisposable.Dispose()
    {
        this.Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        this.dispatcher?.Dispatch(() =>
        {
            var changed = this.PropertyChanged;
            if (changed == null)
            {
                return;
            }

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        });
    }

    /// <summary>
    /// Sets the property value and raises the PropertyChanged event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="backingStore">The backing store of the property.</param>
    /// <param name="value">The new value of the property.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="onChanged">The action to be executed when the property value changes.</param>
    /// <returns>True if the property value has changed, otherwise false.</returns>
    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "", Action? onChanged = null)
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
        {
            return false;
        }

        backingStore = value;
        onChanged?.Invoke();
        this.OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Disposes the resources used by the class.
    /// </summary>
    /// <param name="disposing">True if called from the Dispose method, false if called from the finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!this.disposedValue)
        {
            if (disposing)
            {
                this.source?.Cancel();
                this.download.DownloadStarted -= this.Download_DownloadStarted;
                this.download.DownloadFileCompleted -= this.Download_DownloadFileCompleted;
                this.download.DownloadProgressChanged -= this.Download_DownloadProgressChanged;
                this.download.Dispose();
            }

            this.disposedValue = true;
        }
    }

    private void UpdateButtons()
    {
        this.OnPropertyChanged(nameof(this.ShowDownloadButton));
        this.OnPropertyChanged(nameof(this.ShowCancelButton));
        this.OnPropertyChanged(nameof(this.ShowDeleteButton));

        this.DownloadCommand.RaiseCanExecuteChanged();
        this.CancelCommand.RaiseCanExecuteChanged();
        this.DeleteCommand.RaiseCanExecuteChanged();

        this.service.UpdateAvailableModels();
    }

    private void Download_DownloadProgressChanged(object? sender, DownloadProgressChangedEventArgs e)
    {
        this.Precent = e.ProgressPercentage / 100;
    }

    private void Download_DownloadFileCompleted(object? sender, System.ComponentModel.AsyncCompletedEventArgs e)
    {
        this.DownloadStarted = false;
        if (e.Cancelled && e.UserState is Downloader.DownloadPackage package)
        {
            this.DeleteAsync().FireAndForgetSafeAsync();
        }

        this.UpdateButtons();
    }

    private void Download_DownloadStarted(object? sender, DownloadStartedEventArgs e)
    {
        this.DownloadStarted = true;
        this.UpdateButtons();
    }

    private async Task DownloadAsync()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(this.Model.FileLocation)!);
        await this.download.DownloadFileTaskAsync(this.Model.DownloadUrl, this.Model.FileLocation, this.source.Token);
    }

    private Task CancelAsync()
    {
        this.download.CancelAsync();
        this.UpdateButtons();
        return Task.CompletedTask;
    }

    private Task DeleteAsync()
    {
        if (File.Exists(this.Model.FileLocation))
        {
            File.Delete(this.Model.FileLocation);
        }

        this.OnPropertyChanged(nameof(this.Model.Exists));
        this.UpdateButtons();
        return Task.CompletedTask;
    }
}
