
using UnityEngine;

public interface IObjectInteractionState
{
    void EnterState(ARMainApplication context, GameObject obj);
    void UpdateState(ARMainApplication context, GameObject obj);
    void ExitState(ARMainApplication context, GameObject obj);
}
