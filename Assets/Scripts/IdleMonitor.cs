using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

using UnityEngine;

public class IdleMonitor : Singleton<IdleMonitor>
{
    [HideInInspector] public Process LaunchedProcess { get; set; }

    [Tooltip("Measured in seconds.")]
    public float idleThreshold = 300f;
    private bool processKilled = false;

    [StructLayout(LayoutKind.Sequential)]
    private struct LASTINPUTINFO
    {
        public uint cbSize;
        public uint dwTime;
    }

    [DllImport("user32.dll")]
    private static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

    public void Update()
    {
        if (!LaunchManager.Instance.Playing) return;

        if (LaunchedProcess != null && !LaunchedProcess.HasExited)
        {
            float idleTimeSeconds = GetIdleTimeSeconds();

            if (idleTimeSeconds > idleThreshold)
                KillProcess();
        }
    }

    private float GetIdleTimeSeconds()
    {
        LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
        lastInputInfo.cbSize = (uint)Marshal.SizeOf(typeof(LASTINPUTINFO));

        if (!GetLastInputInfo(ref lastInputInfo))
        {
            UnityEngine.Debug.LogError("Failed to get last input info.");
            return 0f;
        }

        uint idleMilliseconds = (uint)Environment.TickCount - lastInputInfo.dwTime;
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

        processKilled = true;
        LaunchedProcess.WaitForExit();
        LaunchedProcess = null;
    }
    
}