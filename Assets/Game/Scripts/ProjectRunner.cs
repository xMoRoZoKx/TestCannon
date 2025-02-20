using System.Collections;
using System.Collections.Generic;
using UniTools;
using UnityEngine;

public class ProjectRunner : MonoBehaviour
{
    [SerializeField] private CannonView cannon;
    private void Awake()
    {
        WindowManager.Instance.Show<GameHUD>(window =>
        {
            window.Show(cannon);
        });
    }
}
