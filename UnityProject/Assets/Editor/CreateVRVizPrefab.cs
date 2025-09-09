#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class CreateVRVizPrefab
{
    [MenuItem("Tools/Create VR Viz Prefab and Scene")]
    public static void CreatePrefabAndScene()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Prefabs")) AssetDatabase.CreateFolder("Assets","Prefabs");
        if (!AssetDatabase.IsValidFolder("Assets/Materials")) AssetDatabase.CreateFolder("Assets","Materials");
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes");

        var shader = Shader.Find("Unlit/PointCloudUnlit");
        if (shader == null){ Debug.LogError("Shader 'Unlit/PointCloudUnlit' not found."); return; }
        var pcMat = new Material(shader);
        AssetDatabase.CreateAsset(pcMat, "Assets/Materials/PointCloudUnlit.mat");

        var root = new GameObject("VRVizRig");

        var rgbQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        rgbQuad.name = "RGBQuad";
        rgbQuad.transform.SetParent(root.transform);
        rgbQuad.transform.localPosition = new Vector3(0, 1.5f, 2.0f);
        rgbQuad.transform.localScale = new Vector3(1.6f, 0.9f, 1f);
        var rgb = rgbQuad.AddComponent<RGBSubscriber>();
        rgb.targetRenderer = rgbQuad.GetComponent<MeshRenderer>();

        var pc = new GameObject("PointCloud");
        pc.transform.SetParent(root.transform);
        var pcs = pc.AddComponent<PointCloudSubscriber>();
        pcs.pointMaterial = pcMat;
        pcs.pointSize = 4.0f;

        var ctrlViz = new GameObject("ControllerViz");
        ctrlViz.transform.SetParent(root.transform);
        var left = new GameObject("LeftController"); left.transform.SetParent(ctrlViz.transform);
        var right = new GameObject("RightController"); right.transform.SetParent(ctrlViz.transform);
        left.AddComponent<AxisGizmo>(); right.AddComponent<AxisGizmo>();
        var posePub = ctrlViz.AddComponent<ControllerPosePublisher>();
        posePub.leftViz = left.transform; posePub.rightViz = right.transform;
        ctrlViz.AddComponent<ControllerCommandPublisher>();

        var prefabPath = "Assets/Prefabs/VRViz.prefab";
        PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
        var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects);
        PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath));
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, "Assets/Scenes/VRVizSample.unity");

        GameObject.DestroyImmediate(root);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("VRViz prefab and scene created.");
    }
}
#endif