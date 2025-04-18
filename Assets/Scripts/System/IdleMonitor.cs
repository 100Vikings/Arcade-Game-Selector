using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;

using UnityEngine;

public class IdleMonitor : Singleton<IdleMonitor>
{
    private Process launchedProcess;
    [HideInInspector]
    public Process LaunchedProcess
    {
        get
        {
            return launchedProcess;
        }
        set
        {
            launchedProcess = value;
            if (launchedProcess != null)
                StartMonitoring();
        }
    }

    [Tooltip("Measured in seconds.")]
    public float idleThreshold = 300f;
    private uint launchLastInputTick;

    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    private void StartMonitoring()
    {
        LASTINPUTINFO info = new() { cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO)) };
        GetLastInputInfo(ref info);
        launchLastInputTick = (uint)Environment.TickCount - info.dwTime;

        StartCoroutine(MonitorProcess());
    }

    private IEnumerator MonitorProcess()
    {
        bool isRunning = true;
        while (isRunning)
        {
            yield return null;
            if (LaunchedProcess != null)
            {
                try
                {
                    isRunning = !LaunchedProcess.HasExited;
                }
                catch (InvalidOperationException)
                {
                    // No process was ever started (or it was disposed)
                    LaunchedProcess = null;
                    isRunning = false;
                }

                if (isRunning)
                {
                    float idleTime = GetIdleTimeSeconds();
                    print(idleTime);
                    if (idleTime > idleThreshold)
                    {
                        KillProcess();
                        isRunning = false;
                    }
                }
            }
        }
    }

    private float GetIdleTimeSeconds()
    {
        LASTINPUTINFO lastInputInfo = new()
        {
            cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO))
        };

        if (!GetLastInputInfo(ref lastInputInfo))
        {
            UnityEngine.Debug.LogError("Failed to get last input info.");
            return 0f;
        }

        uint idleMilliseconds = (uint)Environment.TickCount - lastInputInfo.dwTime; // - launchLastInputTick;
        return idleMilliseconds / 1000f;
    }

    private void KillProcess()
    {
        try
        {
            LaunchedProcess.Kill();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Error killing process. {e.Message}");
        }

        LaunchedProcess.WaitForExit();
        LaunchedProcess = null;
    }

}