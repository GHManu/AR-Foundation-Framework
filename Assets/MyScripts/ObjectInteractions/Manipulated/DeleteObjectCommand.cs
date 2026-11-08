using UnityEngine;

public class DeleteObjectCommand : ICommand
{
    private readonly ARMainApplication context;
    private readonly GameObject target_object;
    private Coroutine destroyCoroutine;

    // Proprietà per verificare se l'oggetto è stato definitivamente distrutto
    public bool IsDestroyed => target_object == null;

    public DeleteObjectCommand(ARMainApplication context, GameObject targetObject)
    {
        this.context = context;
        this.target_object = targetObject;
    }

    public void Execute()
    {
        if (target_object == null) return;

        // Rimuove l'oggetto dalla lista della scena e dalla selezione
        if (context.Objects_Spawned.Contains(target_object))
        {
            context.Objects_Spawned.Remove(target_object);
        }

        if (context.Selected_Object == target_object)
        {
            context.Selected_Object = null;
        }

        target_object.SetActive(false);

        // Avvia la coroutine usando il contesto ARMainApplication che è sempre attivo in scena
        destroyCoroutine = context.StartTimer(15f, OnTimerExpired);
    }

    public void Undo()
    {
        if (target_object == null) return;

        // Annulla il timer fermando la Coroutine su ARMainApplication
        if (destroyCoroutine != null)
        {
            context.StopCoroutine(destroyCoroutine);
            destroyCoroutine = null;
        }

        target_object.SetActive(true);

        if (!context.Objects_Spawned.Contains(target_object))
        {
            context.Objects_Spawned.Add(target_object);
        }

        context.Selected_Object = target_object;
    }

    // Callback eseguita se il timer scade
    private void OnTimerExpired()
    {
        // Rimuove questo comando dalla cronologia dell'Undo
        context.Command_History.RemoveCommand(this);

        // Distrugge definitivamente il GameObject
        DisposeAndDestroyPermanent();
    }

    public void DisposeAndDestroyPermanent()
    {
        if (target_object != null)
        {
            Object.Destroy(target_object);
        }
    }
}