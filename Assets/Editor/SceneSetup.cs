using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SceneSetup
{
    private const string BackgroundPath = "Assets/Backgrounds/v_01.png";
    private const string VampireName = "Meshy_AI_Vampire_Knight_in_Arm_0413111311_texture";
    private const string WitchName = "Meshy_AI_Skullbound_Witch_0413083525_texture";
    private const string TriggerPath = "Library/_AutoSetupTrigger";
    private const float GameViewAspect = 16f / 9f;

    [DidReloadScripts]
    private static void OnScriptsReloaded()
    {
        // Cleanup legacy trigger if present, but don't require it: re-frame every reload.
        if (System.IO.File.Exists(TriggerPath)) System.IO.File.Delete(TriggerPath);
        EditorApplication.delayCall += FrameCamera;
    }

    [MenuItem("Tools/Frame Camera To Background")]
    public static void FrameCamera()
    {
        var camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("[SceneSetup] No MainCamera in scene.");
            return;
        }

        var bg = GameObject.Find("Background");
        Vector3 targetPos = bg != null ? bg.transform.position : Vector3.zero;
        Vector3 bgScale = bg != null ? bg.transform.lossyScale : new Vector3(10f, 6f, 1f);

        Undo.RecordObject(camera.transform, "Frame Camera Transform");
        Undo.RecordObject(camera, "Frame Camera Settings");

        // Background is rotated -90 around Y, so its normal faces -X.
        // Place camera in front of it on the +X side, looking back at -X.
        camera.transform.position = new Vector3(targetPos.x + 10f, targetPos.y, targetPos.z);
        camera.transform.rotation = Quaternion.Euler(0f, 270f, 0f);

        camera.orthographic = true;
        // bgScale.x is the quad's local-X size (its width). bgScale.y is its height.
        float fitByWidth = (bgScale.x * 0.5f) / GameViewAspect;
        float fitByHeight = bgScale.y * 0.5f;
        camera.orthographicSize = Mathf.Max(fitByWidth, fitByHeight) * 1.05f;

        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 100f;

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log($"[SceneSetup] Camera framed. ortho size = {camera.orthographicSize:F2}.");
    }

    [MenuItem("Tools/Setup Battle Scene (Full)")]
    public static void SetupBattleScene()
    {
        SetupBackground();
        PlaceHeroes();
        SetupLighting();
        FrameCamera();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
    }

    private static void SetupBackground()
    {
        var bg = GameObject.Find("Background");
        if (bg == null)
        {
            bg = GameObject.CreatePrimitive(PrimitiveType.Quad);
            bg.name = "Background";
            Undo.RegisterCreatedObjectUndo(bg, "Create Background");
            var col = bg.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);
        }

        Undo.RecordObject(bg.transform, "Position Background");
        bg.transform.position = new Vector3(0f, 3.5f, 0f);
        bg.transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        bg.transform.localScale = new Vector3(10.65f, 7.1f, 1f);

        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(BackgroundPath);
        if (texture == null)
        {
            Debug.LogWarning("[SceneSetup] Background not found at " + BackgroundPath);
            return;
        }

        var renderer = bg.GetComponent<MeshRenderer>();
        var shader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
        var mat = new Material(shader) { mainTexture = texture };
        renderer.sharedMaterial = mat;
    }

    private static void PlaceHeroes()
    {
        var vampire = GameObject.Find(VampireName);
        var witch = GameObject.Find(WitchName);

        if (vampire != null)
        {
            Undo.RecordObject(vampire.transform, "Place Vampire");
            vampire.transform.position = new Vector3(0f, 0f, -3f);
            vampire.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
        if (witch != null)
        {
            Undo.RecordObject(witch.transform, "Place Witch");
            witch.transform.position = new Vector3(0f, 0f, -4.2f);
            witch.transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
    }

    private static void SetupLighting()
    {
        var sun = GameObject.Find("Directional Light");
        if (sun == null) return;

        Undo.RecordObject(sun.transform, "Re-aim Sun");
        sun.transform.rotation = Quaternion.Euler(15f, -45f, 0f);
        var light = sun.GetComponent<Light>();
        if (light != null)
        {
            Undo.RecordObject(light, "Sun Settings");
            light.color = new Color(0.65f, 0.75f, 1f);
            light.intensity = 1.2f;
        }
    }
}
