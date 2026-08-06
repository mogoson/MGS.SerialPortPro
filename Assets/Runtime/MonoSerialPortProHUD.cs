/*************************************************************************
 *  Copyright © 2026 Mogoson All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  MonoSerialPortProHUD.cs
 *  Description  :  Default.
 *------------------------------------------------------------------------
 *  Author       :  Mogoson
 *  Version      :  1.0.0
 *  Date         :  04/01/2026
 *  Description  :  Initial development version.
 *************************************************************************/

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MGS.SerialPortPro
{
    [RequireComponent(typeof(MonoSerialPortPro))]
    public class MonoSerialPortProHUD : MonoBehaviour
    {
        public float top = 10;
        public float left = 10;
        public float width = 240;
        public float height = 260;
        public int lines = 5;

        private const string SPACE = "\x0020";
        private string readText = string.Empty;
        private int readLines = 0;
        private string writeText = "00 01 02 03 04";
        private MonoSerialPortPro monoPort;

        private void Awake()
        {
            monoPort = GetComponent<MonoSerialPortPro>();
            monoPort.OnReadEvent += MonoPort_OnReadEvent;
        }

        private void Start()
        {
            WriteText_OnChangedEvent(writeText);
        }

        private void MonoPort_OnReadEvent(byte[] bytes)
        {
            var text = string.Empty;
            foreach (var byt in bytes)
            {
                text += $"{byt:X2}{SPACE}";
            }
            if (readLines >= lines)
            {
                readText = string.Empty;
                readLines = 0;
            }
            readText += $"{text}\r\n";
            readLines++;
        }

        private void WriteText_OnChangedEvent(string text)
        {
            var bytes = new List<byte>();
            var byteStrs = text.Split(SPACE, StringSplitOptions.RemoveEmptyEntries);
            foreach (var byteStr in byteStrs)
            {
                try
                {
                    bytes.Add(byte.Parse(byteStr, NumberStyles.HexNumber));
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                }
            }
            monoPort.Port.WriteBytes = bytes.ToArray();
        }

        private void OnGUI()
        {
            var rect = new Rect(left, top, width, height);
            GUILayout.BeginArea(rect, monoPort.Port.PortName, "Window");

            GUILayout.BeginHorizontal();
            if (monoPort.Port.IsOpen)
            {
                if (GUILayout.Button("Close"))
                {
                    monoPort.Port.Close();
                }
            }
            else
            {
                if (GUILayout.Button("Open"))
                {
                    monoPort.Port.Open();
                }
            }
            GUILayout.EndHorizontal();

            GUI.changed = false;
            writeText = GUILayout.TextArea(writeText, GUILayout.Height(50));
            if (GUI.changed)
            {
                WriteText_OnChangedEvent(writeText);
            }

            GUILayout.BeginHorizontal();
            if (monoPort.Port.IsWriting)
            {
                if (GUILayout.Button("Stop Write"))
                {
                    monoPort.Port.StopWrite();
                }
            }
            else
            {
                if (GUILayout.Button("Start Write"))
                {
                    monoPort.Port.StartWrite();
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (monoPort.Port.IsReading)
            {
                if (GUILayout.Button("Stop Read"))
                {
                    monoPort.Port.StopRead();
                }
            }
            else
            {
                if (GUILayout.Button("Start Read"))
                {
                    monoPort.Port.StartRead();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.TextArea(readText, GUILayout.ExpandHeight(true));

            GUILayout.EndArea();
        }
    }
}