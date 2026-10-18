using System;
using Unity.VisualScripting;
using UnityEngine;


public class LaunchManager : MonoBehaviour
{
    public static LaunchManager Instance;
    public event Action LaunchStarted;
    public event Action LaunchEnded;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
        }
        Instance = this.gameObject.GetComponent<LaunchManager>();
    }

    public void StartLaunch()
    {
        LaunchStarted?.Invoke();
    }

    public void EndLaunch()
    {
        LaunchEnded?.Invoke();
    }
}
