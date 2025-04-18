using System;
using System.Diagnostics;
using System.Collections;

using UnityEngine;
using UnityEngine.Events;

public class LaunchManager : Singleton<LaunchManager>
{
    public Animator animator;
    [Tooltip("Measured in seconds.")]
    [field: SerializeField] public float DelayBeforeStart { get; private set; } = 5f;
    public bool Playing { get; set; } = false;

    private static readonly int gameStartedState = Animator.StringToHash("Game Started");
    private static readonly int gameStoppedState = Animator.StringToHash("Game Stopped");

    public void LaunchGame(string fileName)
    {
        StartCoroutine(LaunchGameCoroutine(fileName));
    }

    private IEnumerator LaunchGameCoroutine(string fileName)
    {
        animator.Play(gameStartedState);
        while (!animator.GetCurrentAnimatorStateInfo(0).shortNameHash.Equals(gameStartedState))
            yield return null;
        
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        using Process myProcess = new();
        try
        {
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            myProcess.StartInfo.FileName = fileName;
            myProcess.StartInfo.CreateNoWindow = true;
            
            myProcess.Start();
            
            IdleMonitor.Instance.LaunchedProcess = myProcess;
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError($"Error: Unable to start process. {e.Message}");
        }

        while (!myProcess.HasExited)
            yield return null;

        animator.Play(gameStoppedState);
        while (!animator.GetCurrentAnimatorStateInfo(0).shortNameHash.Equals(gameStartedState))
            yield return null;

        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        while (stateInfo.normalizedTime < 1.0f)
        {
            yield return null;
            stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        }

        Playing = false;
        
        yield return new WaitForSeconds(2f);
        GameUIController.Instance.StartDemoSpin();
    }
}
