using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
/*
 Gestisce i colori dei piani sia reattivamente in base all'orientamento (Orizzontale/Verticale), 
sia dinamicamente per evidenziare il piano attualmente puntato dall'utente durante l'interazione.
 */
public class CustomPlaneManager : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private ARPlaneManager arPlaneManager;

    [Header("Colori Piani")]
    [SerializeField] private Color horizontalColor = new Color(0f, 1f, 0f, 0.35f); // Verde semitrasparente
    [SerializeField] private Color verticalColor = new Color(0f, 0.5f, 1f, 0.35f);   // Blu semitrasparente
    [SerializeField] private Color defaultColor = new Color(1f, 0.9f, 0f, 0.35f);   // Giallo semitrasparente
    [SerializeField] private Color activeColor = new Color(1f, 0.2f, 0.2f, 0.6f);   // Rosso evidenziato

    private ARPlane currentlyActivePlane;

    private void Awake()
    {
        if (arPlaneManager == null)
            arPlaneManager = GetComponent<ARPlaneManager>();
    }

    private void OnEnable()
    {
        if (arPlaneManager != null)
            arPlaneManager.trackablesChanged.AddListener(OnPlanesChanged);
    }

    private void OnDisable()
    {
        if (arPlaneManager != null)
            arPlaneManager.trackablesChanged.RemoveListener(OnPlanesChanged);
    }

    private void OnPlanesChanged(ARTrackablesChangedEventArgs<ARPlane> args)
    {
        foreach (var plane in args.added)
            ApplyDefaultPlaneColor(plane);

        foreach (var plane in args.updated)
        {
            if (plane != currentlyActivePlane)
                ApplyDefaultPlaneColor(plane);
        }
    }

    public void HighlightPlane(ARPlane plane)
    {
        if (currentlyActivePlane != null && currentlyActivePlane != plane)
        {
            ApplyDefaultPlaneColor(currentlyActivePlane);
        }

        currentlyActivePlane = plane;
        if (currentlyActivePlane != null)
        {
            SetPlaneColor(currentlyActivePlane, activeColor);
        }
    }

    public void ClearHighlight()
    {
        if (currentlyActivePlane != null)
        {
            ApplyDefaultPlaneColor(currentlyActivePlane);
            currentlyActivePlane = null;
        }
    }

    private void ApplyDefaultPlaneColor(ARPlane plane)
    {
        if (plane == null) return;

        Color targetColor = plane.alignment switch
        {
            PlaneAlignment.HorizontalUp or PlaneAlignment.HorizontalDown => horizontalColor,
            PlaneAlignment.Vertical => verticalColor,
            _ => defaultColor
        };

        SetPlaneColor(plane, targetColor);
    }

    private void SetPlaneColor(ARPlane plane, Color color)
    {
        Renderer renderer = plane.GetComponent<Renderer>() ?? plane.GetComponentInChildren<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            renderer.material.color = color;
        }
    }
}