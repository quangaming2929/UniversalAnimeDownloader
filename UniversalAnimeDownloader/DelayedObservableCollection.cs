﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UniversalAnimeDownloader
{
    /// <summary>
    /// Collection that add item after an specifyed delay and notify when each item has been added. Useful for making cascading effect in wpf itemsources
    /// </summary>
    /// <typeparam name="T">The type of the children</typeparam>
    public class DelayedObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// The wait time after a item added in the collection
        /// </summary>
        public TimeSpan DelayInterval { get; set; } = TimeSpan.FromSeconds(0.05);

        public async Task AddRange(IList<T> items, CancellationToken token)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (token.IsCancellationRequested)
                    return;
                Items.Add(items[i]);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items[i]));

                for (int j = 0; j < DelayInterval.TotalMilliseconds / 100; j++)
                {
                    if (token.IsCancellationRequested)
                        return;
                    await Task.Delay(100);
                }
                await Task.Delay(TimeSpan.FromMilliseconds(DelayInterval.TotalMilliseconds - (int)(Math.Floor(DelayInterval.TotalMilliseconds / 100) * 100)));
            }
            OnPropertyChanged(new PropertyChangedEventArgs("Count"));
            OnCollectionReallyChanged();
        }

        public void RemoveAll()
        {
            while (Items.Count != 0)
            {
                Items.RemoveAt(0);
            }
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public async Task DelayedRemoveAll()
        {
            while (Items.Count != 0)
            {
                Items.RemoveAt(0);
                await Task.Delay(DelayInterval);
            }
            OnCollectionReallyChanged();
        }

        public async Task AddAndWait(T item)
        {
            Items.Add(item);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            await Task.Delay(DelayInterval);
            OnCollectionReallyChanged();
        }


        public event EventHandler<EventArgs> CollectionReallyChanged;
        private void OnCollectionReallyChanged() => CollectionReallyChanged?.Invoke(this, EventArgs.Empty);
    }
}
