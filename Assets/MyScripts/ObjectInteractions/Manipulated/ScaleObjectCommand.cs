using UnityEngine;

public class ScaleObjectCommand : ICommand
{
    private readonly GameObject target_object;
    private readonly Vector3 start_scale;
    private readonly Vector3 end_scale;

    public ScaleObjectCommand(GameObject target_object, Vector3 start_scale, Vector3 end_scale)
    {
        this.target_object = target_object;
        this.start_scale = start_scale;
        this.end_scale = end_scale;
    }

    public void Execute()
    {
        if (target_object == null) return;
        target_object.transform.localScale = end_scale;
    }

    public void Undo()
    {
        if (target_object == null) return;
        target_object.transform.localScale = start_scale;
    }
}