// MinimapSystem.cs
// Attach this to your MinimapCamera GameObject.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapSystem : MonoBehaviour
{
    [Header("Camera Setup")]
    [SerializeField] private float minimapOrthographicSize = 20f;

    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Objectives")]
    [SerializeField] private List<MinimapObjective> objectives = new List<MinimapObjective>();

    [Header("Minimap UI")]
    [SerializeField] private RawImage minimapDisplay;

    [Header("Marker Prefabs")]
    [SerializeField] private GameObject playerMarkerPrefab;
    [SerializeField] private GameObject objectiveMarkerPrefab;

    private Camera minimapCamera;
    private List<GameObject> spawnedMarkers = new List<GameObject>();
    private GameObject playerMarker;

    void Start()
    {
        minimapCamera = GetComponent<Camera>();
        minimapCamera.orthographic = true;
        minimapCamera.orthographicSize = minimapOrthographicSize;

        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        if (playerMarkerPrefab != null && player != null)
        {
            playerMarker = Instantiate(playerMarkerPrefab);
            playerMarker.layer = LayerMask.NameToLayer("Minimap");
        }

        RefreshObjectiveMarkers();
    }

    void LateUpdate()
    {
        if (player == null) return;

        // Follow the player — camera sits directly behind (negative Z)
        transform.position = new Vector3(player.position.x, player.position.y, player.position.z - 10f);

        if (playerMarker != null)
            playerMarker.transform.position = new Vector3(player.position.x, player.position.y, player.position.z - 9f);

        for (int i = 0; i < objectives.Count && i < spawnedMarkers.Count; i++)
        {
            if (spawnedMarkers[i] != null && objectives[i].targetTransform != null)
            {
                Vector3 objPos = objectives[i].targetTransform.position;
                spawnedMarkers[i].transform.position = new Vector3(objPos.x, objPos.y, objPos.z - 9f);
            }
        }
    }

    public void AddObjective(Transform target, string label = "")
    {
        objectives.Add(new MinimapObjective { targetTransform = target, label = label });
        RefreshObjectiveMarkers();
    }

    public void RefreshObjectiveMarkers()
    {
        foreach (GameObject marker in spawnedMarkers)
            if (marker != null) Destroy(marker);
        spawnedMarkers.Clear();

        if (objectiveMarkerPrefab == null) return;

        foreach (MinimapObjective obj in objectives)
        {
            if (obj.targetTransform == null) continue;
            GameObject marker = Instantiate(objectiveMarkerPrefab);
            marker.layer = LayerMask.NameToLayer("Minimap");
            spawnedMarkers.Add(marker);
        }
    }

    public void ClearObjectives()
    {
        objectives.Clear();
        RefreshObjectiveMarkers();
    }
}

[System.Serializable]
public class MinimapObjective
{
    public Transform targetTransform;
    public string label = "Objective";
    public Color markerColor = Color.yellow;
}