﻿/*
 * Copyright (c) 2018 Academia Sinica, Institude of Information Science
 *
 * License:
 *      GPL 3.0 : The content of this file is subject to the terms and
 *      conditions defined in file 'COPYING.txt', which is part of this source
 *      code package.
 *
 * Project Name:
 *
 *      IndoorNavigation
 *
 * File Description:
 * 
 *      Monitor and Notification module is respondsible for monitoring user's
 *      path. After the user gets to the wrong way, this module sends an event
 *      to notice the user and redirects the path.
 * 
 * File Name:
 *
 *      MaNModule.cs
 *
 * Abstract:
 *
 *       
 *
 * Authors:
 *
 *      Kenneth Tang, kenneth@gm.nssh.ntpc.edu.tw
 *
 */

using IndoorNavigation.Models;
using System;
using System.Diagnostics;
using System.Threading;

namespace IndoorNavigation.Modules
{
    /// <summary>
    /// Notification module
    /// </summary>
    public class MaNModule : IDisposable
    {
        private Thread MaNThread;
        private bool isThreadRunning = true;
        private ManualResetEvent navigationAlgorithmWait =
            new ManualResetEvent(false);
        private ManualResetEvent threadWait =
            new ManualResetEvent(false);
        public MaNEEvent Event { get; private set; }
        private INavigationAlgorithm navigationAlgorithm;

        /// <summary>
        /// Initialize a Monitor and Notification Model
        /// </summary>
        public MaNModule()
        {
            Event = new MaNEEvent();
            MaNThread = new Thread(MaNWork) { IsBackground = true };
            MaNThread.Start();
            threadWait.WaitOne();

            Debug.WriteLine("MaNModule initialization completed.");
        }

        private void MaNWork()
        {
            threadWait.Set();
            threadWait.Reset();

            while (isThreadRunning)
            {
                // 等待演算法套用
                navigationAlgorithmWait.WaitOne();
                if (isThreadRunning)
                    navigationAlgorithm.Work();
            }

            Debug.WriteLine("MaN module close");
            threadWait.Set();
            threadWait.Reset();
        }

        public void SetAlgorithm(INavigationAlgorithm NavigationAlgorithm)
        {
            navigationAlgorithm = NavigationAlgorithm;
            navigationAlgorithmWait.Set();
            navigationAlgorithmWait.Reset();
        }

        /// <summary>
        /// Stop navigation
        /// </summary>
        public void StopNavigation()
        {
            navigationAlgorithm.StopNavigation();
        }

        #region IDisposable Support
        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                isThreadRunning = false;
                navigationAlgorithmWait.Set();
                navigationAlgorithm.StopNavigation();
                threadWait.WaitOne();

                if (disposing)
                {
                    navigationAlgorithmWait.Dispose();
                    threadWait.Dispose();
                    navigationAlgorithmWait = null;
                    threadWait = null;
                }

                MaNThread = null;
                Event = null;
                navigationAlgorithm = null;

                disposedValue = true;
            }
        }

        ~MaNModule()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }

    public class MaNEEvent
    {
        public event EventHandler MaNEventHandler;

        public void OnEventCall(EventArgs e)
        {
            MaNEventHandler?.Invoke(this, e);
        }
    }

}
