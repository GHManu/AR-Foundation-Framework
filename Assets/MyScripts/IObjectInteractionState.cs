
using UnityEngine;

public interface IObjectInteractionState
{
    void EnterState(ARSpawnObject context, GameObject obj);
    void UpdateState(ARSpawnObject context, GameObject obj);
    void ExitState(ARSpawnObject context, GameObject obj);
}
