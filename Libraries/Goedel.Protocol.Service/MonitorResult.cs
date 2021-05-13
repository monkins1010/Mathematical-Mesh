﻿using System;
using System.Threading;

namespace Goedel.Protocol.Service {
    /// <summary>
    /// Class containing a set of monitor results.
    /// </summary>
    public class MonitorResult {
        #region // Properties

        ///<summary>The time at which the result interval began</summary> 
        public DateTime TimeBegin => new(timeBegin);

        ///<summary>The time at which the next result interval began</summary> 
        public DateTime TimeEnd => new(timeEnd);

        ///<summary>The elapsed time.</summary> 
        public long TimeElapsed => 
            (timeEnd == DateTime.MaxValue.Ticks ? DateTime.Now.Ticks : timeEnd) - timeBegin;

        long timeBegin;
        long timeEnd;

        ///<summary>Number of listener events received</summary> 
        public int[] ListenerRequests => listenerRequests;
        private int[] listenerRequests;


        ///<summary>Number of dispatcher requests started.</summary> 
        public int[] DispatcherRequests => dispatcherRequests;
        private int[] dispatcherRequests;

        ///<summary>Time the dispatcher thread was busy in milliseconds</summary> 
        public long[] DispatcherBusy => dispatcherBusy;
        private long[] dispatcherBusy;

        ///<summary>Time all threads were busy.</summary> 
        public long Busy = 0;
        #endregion
        #region // Constructors
        /// <summary>
        /// Constructor creating a new result for <paramref name="listeners"/> and
        /// <paramref name="dispatchers"/> dispatchers.
        /// </summary>
        /// <param name="listeners">The number of listener threads.</param>
        /// <param name="dispatchers">The number of dispatcher threads.</param>
        public MonitorResult(int listeners, int dispatchers) {
            timeBegin = DateTime.Now.Ticks;
            timeEnd = DateTime.MaxValue.Ticks;
            listenerRequests = new int[listeners];
            dispatcherRequests = new int[dispatchers];
            dispatcherBusy = new long[dispatchers];
            }
        #endregion
        #region // Methods 
        /// <summary>
        /// Add the continguous result <paramref name="value"/> to the current result.
        /// </summary>
        /// <param name="value">The contiguous result data.</param>
        public void Add(MonitorResult value) {
            timeEnd = value.timeEnd;
            lock (value) {
                for (var i = 0; i < listenerRequests.Length; i++) {
                    Interlocked.Add(ref listenerRequests[i], value.listenerRequests[i]);
                    }
                for (var i = 0; i < dispatcherRequests.Length; i++) {
                    Interlocked.Add(ref dispatcherRequests[i], value.dispatcherRequests[i]);
                    Interlocked.Add(ref dispatcherBusy[i], value.dispatcherBusy[i]);
                    }
                }

            }

        /// <summary>
        /// Create a copy of the current monitor result, clear the current values and return
        /// the copy. This is performed 
        /// </summary>
        /// <returns>The copy of the original values.</returns>
        public MonitorResult CopyClear() {
            var result = new MonitorResult(ListenerRequests.Length, DispatcherRequests.Length);

            result.listenerRequests = Interlocked.Exchange(ref listenerRequests, result.listenerRequests);
            result.dispatcherRequests = Interlocked.Exchange(ref dispatcherRequests, result.dispatcherRequests);
            result.dispatcherBusy = Interlocked.Exchange(ref dispatcherBusy, result.dispatcherBusy);

            return result;
            }

        /// <summary>
        /// Create a copy of the current monitor result and return
        /// the copy. This is performed 
        /// </summary>
        /// <returns>The copy of the values.</returns>
        public MonitorResult Copy() {
            var result = new MonitorResult(ListenerRequests.Length, DispatcherRequests.Length);
            result.timeEnd = result.timeBegin;
            result.timeBegin = timeBegin;
            result.listenerRequests = listenerRequests;
            result.dispatcherRequests = dispatcherRequests;
            result.dispatcherBusy = dispatcherBusy;

            return result;
            }

        #endregion
        }


    }