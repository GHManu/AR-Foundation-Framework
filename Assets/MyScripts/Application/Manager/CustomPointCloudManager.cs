using UnityEngine;
using UnityEngine.XR.ARFoundation;
/*
 Gestisce la resa visiva dei punti di tracciamento AR, modificandone il colore e la dimensione 
(ad esempio in base all'altezza o quando l'utente sta interagendo).
 */
public class CustomPointCloudManager : MonoBehaviour
{
   
    [Header("Riferimenti")]
    [SerializeField] private ARMainApplication ar_app;
    [SerializeField] private ARPointCloudManager pointCloudManager;

    [Header("Configurazione Colore Punti")]
    [SerializeField] private Color pointColorNormal = Color.cyan;
    [SerializeField] private Color pointColorActive = Color.yellow;
    [SerializeField] private float pointSize = 0.02f;

    private void Awake()
    {
        if (pointCloudManager == null)
            pointCloudManager = GetComponent<ARPointCloudManager>();

        ar_app = GetComponent<ARMainApplication>();
    }

    private void OnEnable()
    {
        if (pointCloudManager != null)
            pointCloudManager.trackablesChanged.AddListener(OnPointCloudChanged);

        if (ar_app != null)                                          
            ar_app.On_dragging_state_changed += OnDraggingStateChanged;
    }

    private void OnDisable()
    {
        if (pointCloudManager != null)
            pointCloudManager.trackablesChanged.RemoveListener(OnPointCloudChanged);

        if (ar_app != null)                                          
            ar_app.On_dragging_state_changed -= OnDraggingStateChanged;

    }

    private void OnDraggingStateChanged(bool is_dragging)
    {
        SetPointCloudActiveState(is_dragging);
    }

    private void OnPointCloudChanged(ARTrackablesChangedEventArgs<ARPointCloud> args)
    {
        foreach (var pointCloud in args.added)
            CustomizePointCloud(pointCloud, pointColorNormal);

        foreach (var pointCloud in args.updated)
            CustomizePointCloud(pointCloud, pointColorNormal);
    }

    public void SetPointCloudActiveState(bool isActive)
    {
        Color targetColor = isActive ? pointColorActive : pointColorNormal;

        if (pointCloudManager == null) return;
        foreach (var pointCloud in pointCloudManager.trackables)
        {
            CustomizePointCloud(pointCloud, targetColor);
        }
    }

    private void CustomizePointCloud(ARPointCloud pointCloud, Color color)
    {
        if (pointCloud == null) return;

        // Se la PointCloud usa un ParticleSystem per la resa visiva
        ParticleSystem ps = pointCloud.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startColor = color;
            main.startSize = pointSize;
        }
    }
}