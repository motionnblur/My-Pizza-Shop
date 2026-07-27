using Engineering.Scripts.Mono.Actors.Table;
using Engineering.Scripts.Mono.Actors.TrashStation;
using Engineering.Scripts.Mono.Player;
using UnityEditor;
using UnityEngine;

public static class WasteSystemSetup
{
    private const string PlayerPrefabPath = "Assets/Engineering/Prefabs/Player.prefab";
    private const string TablePrefabPath = "Assets/Engineering/Prefabs/Table.prefab";
    private const string TrashStationPrefabPath = "Assets/Engineering/Prefabs/TrashStation.prefab";
    private const string LeftoverPrefabPath = "Assets/Engineering/Prefabs/leftover.prefab";

    [MenuItem("Tools/Waste System/Setup All Prefabs")]
    public static void SetupAllPrefabs()
    {
        SetupPlayerPrefab();
        SetupTablePrefab();
        SetupTrashStationPrefab();
        AssetDatabase.SaveAssets();
        Debug.Log("Waste system prefab setup complete.");
    }

    [MenuItem("Tools/Waste System/Setup Player Prefab")]
    public static void SetupPlayerPrefab()
    {
        var playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerPrefabPath);
        if (playerPrefab == null)
        {
            Debug.LogError($"Could not find Player prefab at '{PlayerPrefabPath}'.");
            return;
        }

        var prefabRoot = PrefabUtility.InstantiatePrefab(playerPrefab) as GameObject;
        if (prefabRoot == null)
        {
            Debug.LogError("Failed to instantiate Player prefab.");
            return;
        }

        try
        {
            var scriptsGO = prefabRoot.transform.Find("Scripts");
            if (scriptsGO == null)
            {
                Debug.LogError("Could not find 'Scripts' GameObject in Player prefab.");
                return;
            }

            var meshGO = prefabRoot.transform.Find("Mesh");
            if (meshGO == null)
            {
                Debug.LogError("Could not find 'Mesh' GameObject in Player prefab.");
                return;
            }

            var existingAnchor = meshGO.Find("WasteStackAnchor");
            if (existingAnchor == null)
            {
                var wasteAnchor = new GameObject("WasteStackAnchor").transform;
                wasteAnchor.SetParent(meshGO);
                wasteAnchor.localPosition = new Vector3(0, 0.8f, 0);
                wasteAnchor.localRotation = Quaternion.identity;
                wasteAnchor.localScale = Vector3.one;
            }

            var existingWasteInv = scriptsGO.GetComponent<PlayerWasteInventory>();
            if (existingWasteInv == null)
            {
                var wasteInv = scriptsGO.gameObject.AddComponent<PlayerWasteInventory>();
                var serialized = new SerializedObject(wasteInv);
                serialized.Update();

                var capacityProp = serialized.FindProperty("capacity");
                if (capacityProp != null) capacityProp.intValue = 10;

                var anchorProp = serialized.FindProperty("wasteStackAnchor");
                if (anchorProp != null)
                    anchorProp.objectReferenceValue = meshGO.Find("WasteStackAnchor");

                var prefabProp = serialized.FindProperty("leftoverVisualPrefab");
                if (prefabProp != null)
                {
                    var leftoverPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LeftoverPrefabPath);
                    prefabProp.objectReferenceValue = leftoverPrefab;
                }

                serialized.ApplyModifiedProperties();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PlayerPrefabPath);
            Debug.Log("Player prefab updated with waste inventory.");
        }
        finally
        {
            Object.DestroyImmediate(prefabRoot);
        }
    }

    [MenuItem("Tools/Waste System/Setup Table Prefab")]
    public static void SetupTablePrefab()
    {
        var tablePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TablePrefabPath);
        if (tablePrefab == null)
        {
            Debug.LogError($"Could not find Table prefab at '{TablePrefabPath}'.");
            return;
        }

        var prefabRoot = PrefabUtility.InstantiatePrefab(tablePrefab) as GameObject;
        if (prefabRoot == null)
        {
            Debug.LogError("Failed to instantiate Table prefab.");
            return;
        }

        try
        {
            var table = prefabRoot.GetComponent<Table>();
            if (table == null)
            {
                Debug.LogError("Table component not found on prefab root.");
                return;
            }

            var existingTrigger = prefabRoot.GetComponentInChildren<TableWasteTrigger>();
            if (existingTrigger != null)
            {
                Debug.Log("TableWasteTrigger already exists on Table prefab.");
                return;
            }

            var triggerGO = new GameObject("WasteTrigger");
            triggerGO.transform.SetParent(prefabRoot.transform);
            triggerGO.transform.localPosition = Vector3.zero;
            triggerGO.transform.localRotation = Quaternion.identity;
            triggerGO.transform.localScale = Vector3.one;

            var collider = triggerGO.AddComponent<BoxCollider>();
            collider.isTrigger = true;
            collider.size = new Vector3(1.2f, 0.5f, 1.2f);
            collider.center = new Vector3(0, 0.5f, 0);

            var trigger = triggerGO.AddComponent<TableWasteTrigger>();
            var serialized = new SerializedObject(trigger);
            serialized.Update();
            var tableProp = serialized.FindProperty("table");
            if (tableProp != null)
                tableProp.objectReferenceValue = table;
            serialized.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, TablePrefabPath);
            Debug.Log("Table prefab updated with waste trigger.");
        }
        finally
        {
            Object.DestroyImmediate(prefabRoot);
        }
    }

    [MenuItem("Tools/Waste System/Setup TrashStation Prefab")]
    public static void SetupTrashStationPrefab()
    {
        var stationPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TrashStationPrefabPath);
        if (stationPrefab == null)
        {
            Debug.LogError($"Could not find TrashStation prefab at '{TrashStationPrefabPath}'.");
            return;
        }

        var prefabRoot = PrefabUtility.InstantiatePrefab(stationPrefab) as GameObject;
        if (prefabRoot == null)
        {
            Debug.LogError("Failed to instantiate TrashStation prefab.");
            return;
        }

        try
        {
            var station = prefabRoot.GetComponent<TrashStation>();
            if (station == null)
            {
                Debug.LogError("TrashStation component not found on prefab root.");
                return;
            }

            var serialized = new SerializedObject(station);
            serialized.Update();

            var wastePrefabProp = serialized.FindProperty("wasteVisualPrefab");
            if (wastePrefabProp != null && wastePrefabProp.objectReferenceValue == null)
            {
                var leftoverPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LeftoverPrefabPath);
                wastePrefabProp.objectReferenceValue = leftoverPrefab;
            }

            serialized.ApplyModifiedProperties();

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, TrashStationPrefabPath);
            Debug.Log("TrashStation prefab updated with waste visual prefab reference.");
        }
        finally
        {
            Object.DestroyImmediate(prefabRoot);
        }
    }
}
