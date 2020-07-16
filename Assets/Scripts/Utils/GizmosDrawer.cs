using System;
using UnityEngine;

public class GizmosDrawer : MonoBehaviour
{
    public static Action OnDrawGizmosAction;
    public static Action OnDrawGizmosSelectedAction;

    private void OnDrawGizmos()
    {
        OnDrawGizmosAction?.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        OnDrawGizmosSelectedAction?.Invoke();
    }
}