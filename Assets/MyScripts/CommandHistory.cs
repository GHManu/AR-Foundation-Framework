using System.Collections.Generic;
using UnityEngine;

public class CommandHistory
{
    private readonly LinkedList<ICommand> history = new LinkedList<ICommand>();
    private readonly int maxCapacity;

    public CommandHistory(int maxCapacity = 10)
    {
        this.maxCapacity = maxCapacity;
    }

    public void Push(ICommand command)
    {
        history.AddLast(command);

        // Se superiamo la capacità massima, rimuoviamo il primo elemento (il più vecchio)
        if (history.Count > maxCapacity)
        {
            ICommand oldestCommand = history.First.Value;
            history.RemoveFirst();

            // Se il comando eliminato è una cancellazione definitiva, puliamo la risorsa
            if (oldestCommand is DeleteObjectCommand deleteCmd)
            {
                deleteCmd.DisposeAndDestroyPermanent();
            }
        }
    }

    public ICommand Pop()
    {
        if (history.Count == 0) return null;

        ICommand lastCommand = history.Last.Value;
        history.RemoveLast();
        return lastCommand;
    }

    public bool RemoveCommand(ICommand command)
    {
        if (command == null) return false;

        LinkedListNode<ICommand> node = history.Find(command);
        if (node != null)
        {
            history.Remove(node);
            Debug.Log("[CommandHistory] Comando rimosso con successo dallo storico!");
            return true;
        }

        Debug.LogWarning("[CommandHistory] Impossibile trovare il comando da rimuovere nello storico.");
        return false;
    }

    public int Count => history.Count;
}