// <copyright file="WhisperModelService.cs" company="Drastic Actions">
// Copyright (c) Drastic Actions. All rights reserved.
// </copyright>

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Drastic.Services;
using Microsoft.Extensions.DependencyInjection;
using WhisperCppGui.Models;

namespace WhisperCppGui.Services;

/// <summary>
/// Represents a service for managing Whisper models.
/// </summary>
public class WhisperModelService : INotifyPropertyChanged
{
    private IAppDispatcher dispatcher;
    private WhisperModel? selectedModel;

    /// <summary>
    /// Initializes a new instance of the <see cref="WhisperModelService"/> class.
    /// </summary>
    /// <param name="provider">The service provider.</param>
    public WhisperModelService(IServiceProvider provider)
    {
        this.dispatcher = provider.GetRequiredService<IAppDispatcher>();

        foreach (var item in Enum.GetValuesAsUnderlyingType(typeof(GgmlType)))
        {
            foreach (var qType in Enum.GetValuesAsUnderlyingType(typeof(QuantizationType)))
            {
                this.AllModels.Add(new WhisperModel((GgmlType)item, (QuantizationType)qType));
            }
        }

        this.UpdateAvailableModels();
    }

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Update the selected model.
    /// </summary>
    public event EventHandler? OnUpdatedSelectedModel;

    /// <summary>
    /// Available models have been updated.
    /// </summary>
    public event EventHandler? OnAvailableModelsUpdate;

    /// <summary>
    /// Gets all the Whisper models.
    /// </summary>
    public ObservableCollection<WhisperModel> AllModels { get; } = new ObservableCollection<WhisperModel>();

    /// <summary>
    /// Gets the available Whisper models.
    /// </summary>
    public ObservableCollection<WhisperModel> AvailableModels { get; } = new ObservableCollection<WhisperModel>();

    /// <summary>
    /// Gets or sets the selected Whisper model.
    /// </summary>
    public WhisperModel? SelectedModel
    {
        get
        {
            return this.selectedModel;
        }

        set
        {
            this.SetProperty(ref this.selectedModel, value);
            this.OnUpdatedSelectedModel?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Updates the available Whisper models.
    /// </summary>
    public void UpdateAvailableModels()
    {
        lock (this)
        {
            this.dispatcher.Dispatch(() =>
            {
                this.AvailableModels.Clear();
                var models = this.AllModels.Where(n => n.Exists);
                foreach (var model in models)
                {
                    this.AvailableModels.Add(model);
                }

                if (this.SelectedModel is not null && !this.AvailableModels.Contains(this.SelectedModel))
                {
                    this.SelectedModel = null;
                }

                this.SelectedModel ??= this.AvailableModels.FirstOrDefault();
            });
        }

        this.OnAvailableModelsUpdate?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Raises the PropertyChanged event.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
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
    /// <param name="backingStore">The backing field for the property.</param>
    /// <param name="value">The new value of the property.</param>
    /// <param name="propertyName">The name of the property.</param>
    /// <param name="onChanged">An optional action to be executed when the property value changes.</param>
    /// <returns><c>true</c> if the property value changed; otherwise, <c>false</c>.</returns>
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
}
