﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using UADAPI;

namespace UniversalAnimeDownloader
{
    public sealed class TaskCompletionNotifier<TResult> : INotifyPropertyChanged
    {
        public TaskCompletionNotifier(Task<TResult> task)
        {
            Task = task;
            if (task.IsCompleted) return;
            task.ContinueWith(t =>
            {
                try
                {
                    System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                    {
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Result"));
                    }, System.Windows.Threading.DispatcherPriority.Background);
                }
                catch
                {

                }
            });
        }

        // Gets the task being watched. This property never changes and is never <c>null</c>.
        public Task<TResult> Task { get; private set; }

        // Gets the result of the task. Returns the default value of TResult if the task has not completed successfully.
        public TResult Result { get { return (Task.Status == TaskStatus.RanToCompletion) ? Task.Result : default(TResult); } }

        public event PropertyChangedEventHandler PropertyChanged;

    }

}