#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using LastShift;

/// <summary>
/// Unity Editor pipeline for a real artist-authored character.
/// Expected model: Assets/LastShift/Characters/ThePatient/Model/ThePatient.fbx
/// (or ThePatient.prefab). Imported clips go under Animations with names matching
/// PatientMotion. No geometry/animation is generated or faked.
/// </summary>
public static class PatientCharacterImporter
{
    public const string Folder = "Assets/LastShift/Characters/ThePatient";
    public const string ModelFolder = Folder + "/Model";
    public const string AnimFolder = Folder + "/Animations";
    public const string Generated = Folder + "/Generated";
    public const string ControllerPath = Generated + "/ThePatient.controller";

    public static bool AttachRealModel(LastShiftMonster monster)
    {
        if (monster == null) return false;
        var rig = monster.GetComponent<PatientCharacterRig>();
        if (rig == null) rig = monster.gameObject.AddComponent<PatientCharacterRig>();

        GameObject source = AssetDatabase.LoadAssetAtPath<GameObject>(
            ModelFolder + "/ThePatient.prefab");
        if (source == null)
            source = AssetDatabase.LoadAssetAtPath<GameObject>(ModelFolder + "/ThePatient.fbx");

        if (source == null)
        {
            Debug.LogWarning("[LastShift] Real 3D character missing. Place ThePatient.fbx " +
                "or ThePatient.prefab in " + ModelFolder + ". Enemy chase is disabled.");
            return false;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(source, monster.transform) as GameObject;
        if (instance == null)
        {
            Debug.LogError("[LastShift] Cannot instantiate the supplied model asset.");
            return false;
        }
        instance.name = "The Patient - REAL 3D FBX";
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        // Preserve model's authored scale; prefer world-units in the original FBX.

        var animator = instance.GetComponentInChildren<Animator>(true);
        if (animator == null) animator = instance.AddComponent<Animator>();
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        var clips = FindClips(source);
        int mask = 0;
        foreach (PatientMotion motion in Enum.GetValues(typeof(PatientMotion)))
            if (clips.ContainsKey(motion.ToString()))
                mask |= 1 << (int)motion;

        var controller = BuildController(clips);
        if (controller != null) animator.runtimeAnimatorController = controller;
        rig.Configure(instance, animator, mask);
        rig.SetVisible(true);

        var importedMesh = instance.GetComponentInChildren<SkinnedMeshRenderer>(true);
        if (importedMesh == null)
            Debug.LogError("[LastShift] The Patient model has no SkinnedMeshRenderer. " +
                "Import a true rigged FBX, not a billboard or a static scan.");

        if (animator.avatar == null || !animator.avatar.isValid)
            Debug.LogWarning("[LastShift] Animator Avatar missing or invalid. " +
                "Set FBX Rig > Animation Type = Humanoid and Apply in Unity Inspector.");

        if (!rig.IsReady || importedMesh == null)
        {
            Debug.LogWarning("[LastShift] Imported model is NOT production-ready. Required clips: Idle, Walk, Run, Attack. " +
                "Check the Animator states and model rig; enemy stays disabled until ready.");
            return false;
        }
        Debug.Log("[LastShift] Real 3D The Patient connected: " + instance.name +
                  " / " + clips.Count + " animation clips. No fake mesh or billboard.");
        return true;
    }

    private static Dictionary<string, AnimationClip> FindClips(GameObject modelAsset)
    {
        var found = new Dictionary<string, AnimationClip>(StringComparer.OrdinalIgnoreCase);
        var allPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        allPaths.Add(AssetDatabase.GetAssetPath(modelAsset));
        foreach (var guid in AssetDatabase.FindAssets("", new [] { AnimFolder }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.EndsWith(".anim", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
                allPaths.Add(path);
        }
        foreach (string path in allPaths)
        {
            foreach (UnityEngine.Object asset in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                var clip = asset as AnimationClip;
                if (clip == null || clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
                    continue;
                foreach (PatientMotion motion in Enum.GetValues(typeof(PatientMotion)))
                {
                    if (!string.Equals(clip.name, motion.ToString(), StringComparison.OrdinalIgnoreCase))
                        continue;
                    // Clips in the dedicated animation folder take precedence over embedded clips.
                    if (!found.ContainsKey(motion.ToString()) || path.StartsWith(AnimFolder + "/"))
                        found[motion.ToString()] = clip;
                }
            }
        }
        return found;
    }

    private static AnimatorController BuildController(Dictionary<string, AnimationClip> clips)
    {
        EnsureFolder(Generated, Folder);
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
            AssetDatabase.DeleteAsset(ControllerPath);

        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        if (controller == null) return null;

        var stateMachine = controller.layers[0].stateMachine;
        AnimatorState idleState = null;
        foreach (PatientMotion motion in Enum.GetValues(typeof(PatientMotion)))
        {
            if (!clips.TryGetValue(motion.ToString(), out var clip)) continue;
            var state = stateMachine.AddState(motion.ToString());
            state.motion = clip;
            state.writeDefaultValues = true;
            if (motion == PatientMotion.Idle) idleState = state;
        }
        if (idleState != null) stateMachine.defaultState = idleState;
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static void EnsureFolder(string path, string parent)
    {
        if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, "Generated");
    }
}
#endif
