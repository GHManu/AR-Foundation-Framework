using System.Collections.Generic;
using UnityEngine;

//DTO(Data Transfer Object) per clean code e architecture, per non passare tutto main app o tanti parametri
//mentre per lo State posso passare main app perchè lo riguarda direttamente e bisogna interagirci
public struct SpawnContext
{
    public Pose HitPose;
    public GameObject ObjectToSpawn;
    public List<GameObject> ObjectsSpawned;
    public List<GameObject> SpawnableObjects;
    public Camera MainCamera;

    public SpawnContext(Pose hitPose, GameObject objectToSpawn, List<GameObject> objectsSpawned, List<GameObject> spawnableObjects, Camera mainCamera)
    {
        HitPose = hitPose;
        ObjectToSpawn = objectToSpawn;
        ObjectsSpawned = objectsSpawned;
        SpawnableObjects = spawnableObjects;
        MainCamera = mainCamera;
    }
}