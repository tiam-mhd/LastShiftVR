#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using LastShift;

/// <summary>
/// Imports the artist-authored Sketchfab FBX already committed to the repository.
/// Detects any model filename (including shabannn.fbx) and records the actual
/// imported clips, so state names do not have to match the original FBX takes.
/// </summary>
public static class PatientCharacterImporter
{
    public const string Folder = "Assets/LastShift/Characters/ThePatient";
    public const string ModelFolder = Folder + "/Model";
    public const string AnimFolder = Folder + "/Animations";
    public const string Generated = Folder + "/Generated";
    public const string ControllerPath = Generated + "/ThePatient.controller";

    private static readonly PatientMotion[] MotionOrder = {
        PatientMotion.Idle, PatientMotion.Walk, PatientMotion.Run, PatientMotion.Stare,
        PatientMotion.Frightened, PatientMotion.Scare, PatientMotion.Attack,
        PatientMotion.Appear, PatientMotion.Vanish, PatientMotion.Stunned
    };

    public static bool AttachRealModel(LastShiftMonster monster)
    {
        if (monster == null) return false;
        var rig = monster.GetComponent<PatientCharacterRig>();
        if (rig == null) rig = monster.gameObject.AddComponent<PatientCharacterRig>();

        string modelPath = PickModel();
        if (string.IsNullOrEmpty(modelPath))
        {
            Debug.LogError("[LastShift] No FBX/prefab under " + ModelFolder +
                ". Expected shabannn.fbx or another authentic 3D model.");
            return false;
        }

        var source = AssetDatabase.LoadAssetAtPath<GameObject>(modelPath);
        if (source == null)
        {
            Debug.LogError("[LastShift] AssetDatabase failed to load " + modelPath);
            return false;
        }

        GameObject instance = PrefabUtility.InstantiatePrefab(source, monster.transform) as GameObject;
        if (instance == null) return false;
        instance.name = "The Patient - Ghost Girl Animated (real FBX)";
        instance.transform.localPosition = Vector3.zero;
        instance.transform.localRotation = Quaternion.identity;
        instance.SetActive(true);

        var meshes = instance.GetComponentsInChildren<Renderer>(true)
            .Where(r => (r is SkinnedMeshRenderer sk && sk.sharedMesh != null) ||
                        (r is MeshRenderer mr && mr.GetComponent<MeshFilter>() != null &&
                         mr.GetComponent<MeshFilter>().sharedMesh != null))
            .ToArray();
        if (meshes.Length == 0)
        {
            Debug.LogError("[LastShift] Model has no renderable 3D meshes: " + modelPath);
            UnityEngine.Object.DestroyImmediate(instance);
            return false;
        }

        FitAndGround(instance, meshes, 1.72f);
        EnsureUrpMaterials(meshes);

        var animator = instance.GetComponent<Animator>();
        if (animator == null) animator = instance.AddComponent<Animator>();
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        // If imported as Generic, preserve the Generic rig and original animation hierarchy.

        var actualClips = FindRealClips(modelPath);
        var mapping = MapMotionClips(actualClips, out int nativeMask, out string report);
        var controller = BuildController(mapping);
        if (controller != null) animator.runtimeAnimatorController = controller;

        rig.Configure(instance, animator, nativeMask);
        rig.SetVisible(true);

        string rigType = "Prefab";
        var imported = AssetImporter.GetAtPath(modelPath) as ModelImporter;
        if (imported != null) rigType = imported.animationType.ToString();
        Debug.Log("[LastShift / Model] Imported " + modelPath +
                  " | real mesh renderers: " + meshes.Length +
                  " | importer rig: " + rigType +
                  " | original AnimationClips: " + actualClips.Count +
                  "\n" + report);

        if (actualClips.Count == 0)
            Debug.LogWarning("[LastShift / Anim] No imported motion clips were found inside shabannn.fbx " +
                "or Animations folder. The actual model WILL still appear and pursue the player, " +
                "but artist-authored walking/running/attack motions require compatible clips.");

        if (!rig.IsReady)
        {
            Debug.LogError("[LastShift] The FBX has renderable meshes but the real character rig was not bound.");
            return false;
        }

        Debug.Log("[LastShift] The Patient is VISIBLE and READY for Ward 13: " + instance.name);
        return true;
    }

    private static string PickModel()
    {
        string[] priority = {
            ModelFolder + "/shabannn.fbx",
            ModelFolder + "/ThePatient.fbx",
            ModelFolder + "/ThePatient.prefab"
        };
        foreach (string p in priority)
            if (AssetDatabase.LoadAssetAtPath<GameObject>(p) != null) return p;
        var all = AssetDatabase.FindAssets("t:GameObject", new[] { ModelFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(p => p.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase) ||
                        p.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToArray();
        if (all.Length > 1)
            Debug.LogWarning("[LastShift] Multiple models found; using " + all[0]);
        return all.Length == 0 ? null : all[0];
    }

    private static void EnsureUrpMaterials(Renderer[] meshes)
    {
        if (!AssetDatabase.IsValidFolder(Generated))
            AssetDatabase.CreateFolder(Folder, "Generated");

        int slot = 0;
        foreach (Renderer renderer in meshes)
        {
            Material[] materials = renderer.sharedMaterials;
            bool modified = false;
            for (int i = 0; i < materials.Length; i++, slot++)
            {
                Material original = materials[i];
                if (original == null)
                {
                    Debug.LogWarning("[LastShift / Materials] Null material on " + renderer.name +
                        ". A neutral URP material will be used.");
                }
                else if (original.shader != null && original.shader.isSupported &&
                         original.shader.name.StartsWith("Universal Render Pipeline/",
                             StringComparison.OrdinalIgnoreCase))
                    continue;

                // Imported FBX materials can use Standard or unavailable external shaders.
                // Convert only the instance, preserving the FBX and source textures.
                string dst = Generated + "/Patient_URP_Material_" + slot + ".mat";
                Material conversion = AssetDatabase.LoadAssetAtPath<Material>(dst);
                if (conversion == null)
                {
                    conversion = new Material(Ward13UrpMaterialTools.LitShader());
                    conversion.name = "Patient - URP material " + slot;
                    AssetDatabase.CreateAsset(conversion, dst);
                }
                else conversion.shader = Ward13UrpMaterialTools.LitShader();

                Texture texture = null;
                Color tint = Color.white;
                if (original != null)
                {
                    if (original.HasProperty("_BaseMap"))
                        texture = original.GetTexture("_BaseMap");
                    if (texture == null && original.HasProperty("_MainTex"))
                        texture = original.GetTexture("_MainTex");
                    if (original.HasProperty("_BaseColor"))
                        tint = original.GetColor("_BaseColor");
                    else if (original.HasProperty("_Color"))
                        tint = original.GetColor("_Color");
                }
                if (conversion.HasProperty("_BaseMap"))
                    conversion.SetTexture("_BaseMap", texture);
                if (conversion.HasProperty("_BaseColor"))
                    conversion.SetColor("_BaseColor", tint);
                EditorUtility.SetDirty(conversion);
                materials[i] = conversion;
                modified = true;
                if (texture == null)
                    Debug.LogWarning("[LastShift / Materials] No diffuse texture was found for " +
                        renderer.name + " / slot " + i + ". The actual mesh will still render, " +
                        "but its FBX texture may need to be extracted or re-linked.");
            }
            if (modified) renderer.sharedMaterials = materials;
        }
    }

    private static void FitAndGround(GameObject instance, Renderer[] meshes, float targetHeight)
    {
        // Fit the actual mesh to human scale and place the lowest visible vertex at floor level.
        Bounds bounds = meshes[0].bounds;
        foreach (var r in meshes) bounds.Encapsulate(r.bounds);
        if (bounds.size.y < .001f || bounds.size.y > 10000f)
        {
            Debug.LogWarning("[LastShift] FBX has unusual bounds; adjust model Transform manually.");
            return;
        }

        float ratio = Mathf.Clamp(targetHeight / bounds.size.y, .001f, 1000f);
        instance.transform.localScale *= ratio;
        bounds = meshes[0].bounds;
        foreach (var r in meshes) bounds.Encapsulate(r.bounds);
        Vector3 offset = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
        instance.transform.position += offset;
        Debug.Log("[LastShift / Model] Fitted real 3D FBX to " +
                  bounds.size.y.ToString("F2") + " meters (scale factor " +
                  ratio.ToString("F4") + ").");
    }

    private static List<AnimationClip> FindRealClips(string modelPath)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { modelPath };
        foreach (var guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { AnimFolder }))
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));
        foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { AnimFolder }))
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));

        var clips = new List<AnimationClip>();
        foreach (var p in paths.OrderBy(x => x))
        {
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(p))
            {
                if (!(asset is AnimationClip clip) || clip.legacy ||
                    clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
                    continue;
                if (!clips.Contains(clip)) clips.Add(clip);
            }
        }
        return clips;
    }

    private static Dictionary<PatientMotion, AnimationClip> MapMotionClips(
        List<AnimationClip> actual, out int nativeMask, out string report)
    {
        var map = new Dictionary<PatientMotion, AnimationClip>();
        nativeMask = 0;
        var sb = new StringBuilder();
        sb.AppendLine("Imported FBX clips: " +
            (actual.Count == 0 ? "(none)" : string.Join(", ", actual.Select(c => c.name).ToArray())));

        // Original user-supplied clip names are not known ahead of time.
        // Prefer semantic matches, then reuse a valid existing clip when necessary.
        var aliases = new Dictionary<PatientMotion, string[]>
        {
            {PatientMotion.Idle, new[] {"idle","breath","stand","default","take 001","take 01","take 1"}},
            {PatientMotion.Walk, new[] {"walk","creep","locomotion","move"}},
            {PatientMotion.Run, new[] {"run","sprint","chase"}},
            {PatientMotion.Stare, new[] {"stare","look","watch","idle"}},
            {PatientMotion.Frightened, new[] {"fear","fright","recoil","flinch","hit","stun"}},
            {PatientMotion.Scare, new[] {"scare","scream","roar","jump","gesture"}},
            {PatientMotion.Attack, new[] {"attack","strike","slash","grab","lunge","bite"}},
            {PatientMotion.Appear, new[] {"appear","spawn","rise","start","idle"}},
            {PatientMotion.Vanish, new[] {"vanish","disappear","fade","die","death","idle"}},
            {PatientMotion.Stunned, new[] {"stun","hit","fear","flinch","recoil"}}
        };

        foreach (PatientMotion motion in MotionOrder)
        {
            AnimationClip best = null;
            foreach (string alias in aliases[motion])
            {
                best = actual.FirstOrDefault(c =>
                    c.name.Equals(alias, StringComparison.OrdinalIgnoreCase));
                if (best != null) break;
                best = actual.FirstOrDefault(c =>
                    c.name.IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0);
                if (best != null) break;
            }

            bool exactSemantic = best != null;
            if (exactSemantic)
                nativeMask |= (1 << (int)motion);
            else if (actual.Count > 0)
                best = map.TryGetValue(PatientMotion.Idle, out AnimationClip idle)
                    ? idle : actual[0];

            if (best != null) map[motion] = best;
            sb.AppendLine("  " + motion + " -> " +
                (best != null ? best.name + (exactSemantic ? " [matched]" : " [fallback]") : "(no clip)"));
        }

        report = sb.ToString();
        return map;
    }

    private static AnimatorController BuildController(Dictionary<PatientMotion, AnimationClip> map)
    {
        if (!AssetDatabase.IsValidFolder(Generated))
            AssetDatabase.CreateFolder(Folder, "Generated");

        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        // Recreating saved controller is intentional; scene gets the newly generated one.
        if (controller != null) AssetDatabase.DeleteAsset(ControllerPath);
        controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);

        var states = controller.layers[0].stateMachine;
        AnimatorState idle = null;
        foreach (PatientMotion motion in MotionOrder)
        {
            var state = states.AddState(motion.ToString());
            if (map.TryGetValue(motion, out AnimationClip clip)) state.motion = clip;
            state.speed = motion == PatientMotion.Run ? 1.55f :
                          motion == PatientMotion.Walk ? .9f : 1f;
            state.writeDefaultValues = true;
            if (motion == PatientMotion.Idle) idle = state;
        }
        if (idle != null) states.defaultState = idle;
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }
}
#endif