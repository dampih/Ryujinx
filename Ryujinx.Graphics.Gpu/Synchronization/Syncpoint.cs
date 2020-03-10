﻿using System;
using System.Collections.Generic;
using System.Threading;

namespace Ryujinx.Graphics.Gpu.Synchronization
{
    /// <summary>
    /// Represent GPU hardware syncpoint.
    /// </summary>
    class Syncpoint
    {
        private int _storedValue;

        public readonly uint Id;

        // TODO: get ride of this lock
        private object _listLock = new object();

        /// <summary>
        /// The value of the syncpoint.
        /// </summary>
        public uint Value => (uint)_storedValue;

        // TODO: switch to something handling concurrency?
        private List<SyncpointWaiterInformation> _waiters;

        public Syncpoint(uint id)
        {
            Id       = id;
            _waiters = new List<SyncpointWaiterInformation>();
        }

        /// <summary>
        /// Register a new callback for a target threshold.
        /// The callback will be called once the threshold is reached and will automatically be unregistered.
        /// </summary>
        /// <param name="threshold">The target threshold</param>
        /// <param name="callback">The callback to call when the threshold is reached</param>
        /// <returns>the created SyncpointWaiterInformation object or null if already past threshold</returns>
        public SyncpointWaiterInformation RegisterCallback(uint threshold, Action callback)
        {
            lock (_listLock)
            {
                if (Value >= threshold)
                {
                    callback();

                    return null;
                }
                else
                {
                    SyncpointWaiterInformation waiterInformation = new SyncpointWaiterInformation
                    {
                        Threshold = threshold,
                        Callback  = callback
                    };

                    _waiters.Add(waiterInformation);

                    return waiterInformation;
                }
            }
        }

        public void UnregisterCallback(SyncpointWaiterInformation waiterInformation)
        {
            lock (_listLock)
            {
                _waiters.Remove(waiterInformation);
            }
        }

        /// <summary>
        /// Increment the syncpoint
        /// </summary>
        /// <returns>The incremented value of the syncpoint</returns>
        public uint Increment()
        {
            uint currentValue = (uint)Interlocked.Increment(ref _storedValue);

            lock (_listLock)
            {
                _waiters.RemoveAll(item =>
                {
                    bool isPastThreshold = currentValue >= item.Threshold;

                    if (isPastThreshold)
                    {
                        item.Callback();
                    }

                    return isPastThreshold;
                });
            }

            return currentValue;
        }

    }
}
