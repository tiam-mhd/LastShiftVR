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
        if (!Has("THE WATCHER")) { Debug.LogError("[Ward13] Missing ghost root"); problems++; }

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
                Debug.LogWarning("[Ward13] 3D model is present and visible, " +
                    "but FBX has no imported compatible motion clips. " +
                    "Real skeletal animations must be added for cinematic movement.");
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
        if (cameras.Length != 1)
        {
            Debug.LogWarning("[Ward13] Expected exactly 1 camera; got " + cameras.Length +
                ". Check for extra audio listener/camera from your template.");
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
