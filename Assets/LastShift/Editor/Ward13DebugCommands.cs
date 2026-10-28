#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using LastShift;

/// <summary>Play-mode-only visual diagnostics. Not included in release game controls.</summary>
public static class Ward13DebugCommands
{
    [MenuItem("Tools/Last Shift/DEBUG - Show The Patient In Front Of Me (Play Mode)")]
    private static void ShowPatient()
    {
        if (!Application.isPlaying) { Debug.LogWarning("[LastShift DEBUG] Press Play first."); return; }
        var game = Object.FindFirstObjectByType<LastShiftGame>();
        if (game == null || game.monster == null || Camera.main == null)
        {
            Debug.LogError("[LastShift DEBUG] Game/monster/MainCamera missing; regenerate Ward 13 v0.3.");
            return;
        }
        var monster = game.monster;
        monster.StopChasing();
        monster.gameObject.SetActive(true);
        monster.enabled = false;
        var cam = Camera.main.transform;
        var forward = Vector3.ProjectOnPlane(cam.forward, Vector3.up).normalized;
        if (forward.sqrMagnitude < .01f) forward = cam.forward;
        monster.transform.position = cam.position + forward * 3.2f - Vector3.up * 1.52f;
        var visual = monster.GetComponent<GhostGirlVisual>();
        if (visual != null)
        {
            visual.EnsureVisual();
            visual.SetOpacity(1f);
        }
        else
            Debug.LogError("[LastShift DEBUG] GhostGirlVisual component missing.");
        Debug.Log("[LastShift DEBUG] Patient is at 3.2m in front of the camera. Exit Play to reset the scene.");
    }

    [MenuItem("Tools/Last Shift/DEBUG - Test Fuse Circuit 1 (Play Mode)")]
    private static void Circuit1() { TestCircuit(1); }
    [MenuItem("Tools/Last Shift/DEBUG - Test Fuse Circuit 2 (Play Mode)")]
    private static void Circuit2() { TestCircuit(2); }
    [MenuItem("Tools/Last Shift/DEBUG - Test Fuse Circuit 3 (Play Mode)")]
    private static void Circuit3() { TestCircuit(3); }
    [MenuItem("Tools/Last Shift/DEBUG - Restore All Power (Play Mode)")]
    private static void AllPower()
    {
        var light = FindPower();
        if (light != null) light.OnBreakerRestored();
    }

    [MenuItem("Tools/Last Shift/DEBUG - Validate Patient and Power")]
    private static void Validate()
    {
        var game = Object.FindFirstObjectByType<LastShiftGame>();
        if (game == null)
        {
            Debug.LogError("[LastShift DEBUG] No LastShiftGame in open scene. Rebuild the scene with v0.3 generator.");
            return;
        }
        bool texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/LastShift/Resources/GhostGirlReference.png") != null;
        bool ghost = game.monster != null && game.monster.GetComponent<GhostGirlVisual>() != null;
        bool mat = ghost && game.monster.GetComponent<GhostGirlVisual>().preparedMaterial != null;
        bool power = game.lighting != null && game.lighting.circuitLights != null &&
                     game.lighting.circuitLights.Length == 3;
        Debug.Log("[LastShift DEBUG] Texture=" + texture + " GhostComponent=" + ghost +
                  " PreparedMaterial=" + mat + " ThreePowerCircuits=" + power);
        if (!texture || !ghost || !mat || !power)
            Debug.LogError("[LastShift DEBUG] Missing component. Refresh Assets/LastShift and regenerate Ward 13 v0.3.");
    }

    private static void TestCircuit(int number)
    {
        var light = FindPower();
        if (light != null) light.OnFuseInserted(number);
    }

    private static WardPowerLighting FindPower()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[LastShift DEBUG] Press Play first.");
            return null;
        }
        var power = Object.FindFirstObjectByType<WardPowerLighting>();
        if (power == null)
            Debug.LogError("[LastShift DEBUG] WardPowerLighting not in scene; regenerate Ward 13 v0.3.");
        return power;
    }
}
#endif
