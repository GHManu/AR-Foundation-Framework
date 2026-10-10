using UnityEngine;

public class MoveObjectCommand : ICommand
{
    private readonly GameObject targetObject;
    private readonly Vector3 startPosition;
    private readonly Quaternion startRotation;
    private readonly Vector3 endPosition;
    private readonly Quaternion endRotation;

    public MoveObjectCommand(GameObject targetObject, Vector3 startPos, Quaternion startRot, Vector3 endPos, Quaternion endRot)
    {
        this.targetObject = targetObject;
        this.startPosition = startPos;
        this.startRotation = startRot;
        this.endPosition = endPos;
        this.endRotation = endRot;
    }

    public void Execute()
    {
        if (targetObject == null) return;
        targetObject.transform.position = endPosition;
        targetObject.transform.rotation = endRotation;
    }

    public void Undo()
    {
        if (targetObject == null) return;
        targetObject.transform.position = startPosition;
        targetObject.transform.rotation = startRotation;
    }
}