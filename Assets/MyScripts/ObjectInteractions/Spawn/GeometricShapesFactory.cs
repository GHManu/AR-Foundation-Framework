using System.Collections.Generic;
using UnityEngine;
public class GeometricShapesFactory : ASpawnObjectFactory
{
        public override GameObject SpawnObject(SpawnContext context)
        {
            GameObject prefabToSpawn = ChoosePrefabToSpawn(context.SpawnableObjects, context.ObjectToSpawn);
            if (prefabToSpawn == null) return null;

            GameObject prefabSpawned = UnityEngine.Object.Instantiate(prefabToSpawn, context.HitPose.position, context.HitPose.rotation);

            if (context.MainCamera != null)
            {
                Vector3 cameraView = context.MainCamera.transform.position - prefabSpawned.transform.position;
                cameraView.y = 0f;
                if (cameraView != Vector3.zero)
                {
                    prefabSpawned.transform.rotation = Quaternion.LookRotation(cameraView);
                }
            }

            if (prefabSpawned.GetComponent<Collider>() == null)
            {
                prefabSpawned.AddComponent<BoxCollider>();
            }

            context.ObjectsSpawned?.Add(prefabSpawned);
            return prefabSpawned;
        }

        private GameObject ChoosePrefabToSpawn(List<GameObject> list, GameObject toFind)
        {
            if (toFind == null) return null;
            if (list != null)
            {
                foreach (GameObject obj in list)
                {
                    if (obj != null && obj.name.Contains(toFind.name))
                        return obj;
                }
            }
            return toFind;
        }
    
}
