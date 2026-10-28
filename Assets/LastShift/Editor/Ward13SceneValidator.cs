#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using LastShift;

public static class Ward13SceneValidator
{
    // Internal helper; no legacy/test menu entry.
    public static void Validate()
    {
        var problems = 0;
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var all = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        bool Has(string exact)
        {
            foreach (var t in all) if (t.name == exact) return true;
            return false;
        }

        if (!Has("WARD 13")) { Debug.LogError("[Ward13] Missing environment root"); problems++; }
        if (!Has("WARD 12 / TREATMENT")) { Debug.LogError("[Ward13] Missing treatment room"); problems++; }
        if (!Has("WARD 14 / SURGERY")) { Debug.LogError("[Ward13] Missing surgery room"); problems++; }
        if (!Has("PCVR Player Rig")) { Debug.LogError("[Ward13] Missing player rig"); problems++; }
        var bounds = Object.FindFirstObjectByType<WardPlayerBounds>(FindObjectsInactive.Include);
        if (bounds == null || bounds.head == null)
        {
            Debug.LogError("[Ward13] Playable-area guard or its head reference missing.");
            problems++;
        }
        if (!Has("Treatment room solid doorway threshold") ||
            !Has("Surgery room solid doorway threshold"))
        {
            Debug.LogError("[Ward13] One or more doorway floor collision bridges missing.");
            problems++;
        }
        if (!Has("THE WATCHER")) { Debug.LogError("[Ward13] Missing ghost root"); problems++; }
        var chase = Object.FindFirstObjectByType<PatientPathChase>(FindObjectsInactive.Include);
        if (chase == null || chase.player == null)
        {
            Debug.LogError("[Ward13] AI path target is unassigned; the patient cannot track the player.");
            problems++;
        }
        var director = Object.FindFirstObjectByType<GhostDirector>(FindObjectsInactive.Include);
        if (director == null || director.monster == null || director.rig == null)
        {
            Debug.LogError("[Ward13] Persistent ghost encounter controller not connected.");
            problems++;
        }
        int activeShadowLights = 0;
        foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if (light.shadows != LightShadows.None) activeShadowLights++;
        if (activeShadowLights > 0)
            Debug.LogWarning("[Ward13 / Performance] " + activeShadowLights +
                " realtime lights still render shadows. Review URP shadow atlas warnings.");

        // Original PNG is reference artwork only. Its absence must never
        // force a fake portrait-based character back into the game.
        var rig = Object.FindFirstObjectByType<PatientCharacterRig>(FindObjectsInactive.Include);
        if (rig == null)
        {
            Debug.LogError("[Ward13] Real character importer component missing.");
            problems++;
        }
        else if (!rig.IsReady)
        {
            Debug.LogError("[Ward13] The actual FBX model did not produce renderable mesh geometry. " +
                "Check shabannn.fbx and its Unity Inspector import settings.");
            problems++;
        }
        else
        {
            Debug.Log("[Ward13] REAL 3D model verified: " + rig.characterRoot.name +
                " | Animator clips available: " + rig.HasAnimation);
            if (!rig.HasAnimation)
                Debug.LogWarning("[Ward13] Real FBX has no compatible skeletal clips: " +
                    "animation quality will be limited until source clips are added.");

            // The original Sketchfab scene includes a massive Sky mesh and
            // secondary cameras/lights. They MUST NOT be active in Ward 13.
            if (rig.characterRoot != null)
            {
                var transforms = rig.characterRoot.GetComponentsInChildren<Transform>(true);
                foreach (var t in transforms)
                {
                    if (t.name.Equals("Sky", System.StringComparison.OrdinalIgnoreCase) &&
                        t.gameObject.activeInHierarchy)
                    {
                        Debug.LogError("[Ward13] Imported FBX Sky is ACTIVE and obscuring the hospital!");
                        problems++;
                    }
                }
                foreach (var extraCamera in rig.characterRoot.GetComponentsInChildren<Camera>(true))
                    if (extraCamera.enabled && extraCamera.gameObject.activeInHierarchy)
                    {
                        Debug.LogError("[Ward13] Imported FBX camera is active!");
                        problems++;
                    }
                foreach (var extraLight in rig.characterRoot.GetComponentsInChildren<Light>(true))
                    if (extraLight.enabled && extraLight.gameObject.activeInHierarchy)
                    {
                        Debug.LogError("[Ward13] Imported FBX cinematic light is active!");
                        problems++;
                    }
            }
            string[] textures = {
                "ghost_dress_d.dds", "ghost_dress_n.dds", "ghost_dress_s.dds",
                "ghost_head_d1.dds", "ghost_head_n1.dds", "ghost_head_s1.dds",
                "hair_02_d.dds", "hair_02_n.dds", "hair_02_s.dds"
            };
            foreach (string texture in textures)
            {
                string path = PatientCharacterImporter.TextureFolder + "/" + texture;
                if (AssetDatabase.LoadAssetAtPath<Texture2D>(path) == null)
                {
                    Debug.LogError("[Ward13] Missing character DDS texture: " + path);
                    problems++;
                }
            }
        }
        var game = Object.FindFirstObjectByType<LastShiftGame>(FindObjectsInactive.Include);
        if (game == null || game.ghostDirector == null ||
            game.fusePanel == null || game.exitDoor == null || game.monster == null)
        {
            Debug.LogError("[Ward13] Game dependencies not wired"); problems++;
        }
        var fuses = Object.FindObjectsByType<LastShiftFuse>(FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        if (fuses.Length != 3)
        {
            Debug.LogError("[Ward13] Expected exactly 3 fuses; got " + fuses.Length);
            problems++;
        }
        var cameras = Object.FindObjectsByType<Camera>(FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        int usableCameras = 0;
        foreach (var camera in cameras)
            if (camera.enabled && camera.gameObject.activeInHierarchy) usableCameras++;
        // In Edit Mode a single player camera must be usable; FBX cinematic
        // cameras can be present as disabled objects without affecting gameplay.
        if (usableCameras != 1)
        {
            Debug.LogWarning("[Ward13] Expected one active game camera; found " +
                usableCameras + " (the FBX must not override the PCVR camera).");
        }

        if (problems == 0)
            Debug.Log("[Ward13] REAL FBX PIPELINE SCENE STRUCTURE OK: " + scene.name +
                ". This is an editor validation, not a gameplay or hardware test.");
        else
            Debug.LogError("[Ward13] Scene validation failed with " + problems +
                " issue(s). See errors above.");
    }
}
#endif
