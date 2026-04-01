/*************************************************************************
 *  Copyright © 2026 Mogoson All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  MonoSerialPortPro.cs
 *  Description  :  Default.
 *------------------------------------------------------------------------
 *  Author       :  Mogoson
 *  Version      :  1.0.0
 *  Date         :  04/01/2026
 *  Description  :  Initial development version.
 *************************************************************************/

using System;
using System.IO.Ports;
using UnityEngine;

namespace MGS.IO.Ports
{
    public class MonoSerialPortPro : MonoBehaviour
    {
        [SerializeField] protected string portName = "COM1";
        [SerializeField] protected int baudRate = 9600;
        [SerializeField] protected Parity parity = Parity.None;
        [SerializeField] protected int dataBits = 8;
        [SerializeField] protected StopBits stopBits = StopBits.One;

        [Space]
        [SerializeField] protected int readFrame = 5;
        [SerializeField] protected int readInterval = 250;
        [SerializeField] protected int writeInterval = 250;

        public event Action<byte[]> OnReadEvent;
        protected byte[] readBytes;
        protected bool isDirty;

        public SerialPortPro Port { protected set; get; }

        protected virtual void Awake()
        {
            Port = new SerialPortPro(portName, baudRate, parity, dataBits, stopBits)
            {
                ReadInterval = readInterval,
                ReadFrame = readFrame,
                WriteInterval = writeInterval
            };
            Port.OnReadEvent += Port_OnReadEvent;
        }

        private void Port_OnReadEvent(byte[] bytes)
        {
            readBytes = bytes;
            isDirty = true;
        }

        protected virtual void Update()
        {
            if (isDirty)
            {
                isDirty = false;
                OnReadEvent?.Invoke(readBytes);
            }
        }

        protected virtual void OnDestroy()
        {
            Port.Dispose();
        }
    }
}