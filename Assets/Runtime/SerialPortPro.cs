/*************************************************************************
 *  Copyright © 2026 Mogoson All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  SerialPortPro.cs
 *  Description  :  Default.
 *------------------------------------------------------------------------
 *  Author       :  Mogoson
 *  Version      :  1.0.0
 *  Date         :  04/01/2026
 *  Description  :  Initial development version.
 *************************************************************************/

using System;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace MGS.SerialPortPro
{
    public class SerialPortPro : SerialPort
    {
        #region Read Cycle
        public event Action<byte[]> OnReadEvent;

        public bool IsReading { protected set; get; }

        public int ReadInterval { set; get; } = 250;

        public int ReadFrame { set; get; } = 5;

        public void StartRead()
        {
            if (!IsOpen)
            {
                Open();
            }
            if (!IsReading)
            {
                IsReading = true;
                new Thread(Reading) { IsBackground = true }.Start();
            }
        }

        public void StopRead()
        {
            IsReading = false;
        }

        private void Reading()
        {
            while (IsReading)
            {
                try
                {
                    var frames = BytesToRead / ReadFrame;
                    if (frames > 0)
                    {
                        var length = ReadFrame * frames;
                        var buffer = new byte[length];
                        Read(buffer, 0, buffer.Length);
                        if (frames > 1)
                        {
                            buffer = buffer.TakeLast(ReadFrame).ToArray();
                        }
                        OnReadEvent?.Invoke(buffer);
                    }
                    Thread.Sleep(ReadInterval);
                }
                catch (TimeoutException tex)
                {
                    Debug.LogWarning(tex.Message);
                    Thread.Sleep(ReadInterval);
                }
                catch (Exception ex)
                {
                    IsReading = false;
                    Debug.LogException(ex);
                }
            }
        }
        #endregion

        #region Write Cycle
        public bool IsWriting { protected set; get; }

        public int WriteInterval { set; get; } = 250;

        public byte[] WriteBytes
        {
            set
            {
                lock (writeBytes)
                {
                    writeBytes = value.Clone() as byte[];
                }
            }
        }

        private byte[] writeBytes = new byte[5];

        public void StartWrite()
        {
            if (!IsOpen)
            {
                Open();
            }
            if (!IsWriting)
            {
                IsWriting = true;
                new Thread(Writing) { IsBackground = true }.Start();
            }
        }

        public void StopWrite()
        {
            IsWriting = false;
        }

        private void Writing()
        {
            while (IsWriting)
            {
                try
                {
                    lock (writeBytes)
                    {
                        Write(writeBytes, 0, writeBytes.Length);
                    }
                    Thread.Sleep(WriteInterval);
                }
                catch (TimeoutException tex)
                {
                    Debug.LogWarning(tex.Message);
                    Thread.Sleep(WriteInterval);
                }
                catch (Exception ex)
                {
                    IsWriting = false;
                    Debug.LogException(ex);
                }
            }
        }
        #endregion

        public SerialPortPro(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
            : base(portName, baudRate, parity, dataBits, stopBits) { }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                StopRead();
                StopWrite();
            }
            base.Dispose(disposing);
        }
    }
}