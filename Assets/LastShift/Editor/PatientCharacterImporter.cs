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
/// Imports ONLY the actual Zombie_Walk/Ghost character from shabannn.fbx.
/// The Sketchfab FBX also contains a Cinema4D sky, cameras, gravity and lights.
/// None of those cinematic elements belong in the hospital scene.
/// </summary>
public static class PatientCharacterImporter
{
    public const string Folder = "Assets/LastShift/Characters/ThePatient";
    public const string ModelFolder = Folder + "/Model";
    public const string TextureFolder = Folder + "/Textures";
    public const string AnimFolder = Folder + "/Animations";
    public const string Generated = Folder + "/Generated";
    public const string ControllerPath = Generated + "/ThePatient.controller";

    private static readonly PatientMotion[] Motions = (PatientMotion[])Enum.GetValues(typeof(PatientMotion));
    private static readonly Dictionary<PatientMotion, string[]> Aliases =
        new Dictionary<PatientMotion, string[]>
    {
        {PatientMotion.Idle, new[] {"idle","breath","stand","default"}},
        {PatientMotion.Walk, new[] {"walk","creep","locomotion","move","zombie"}},
        {PatientMotion.Run, new[] {"run","sprint","chase"}},
        {PatientMotion.Stare, new[] {"stare","look","watch"}},
        {PatientMotion.Frightened, new[] {"fear","fright","recoil","flinch","hit"}},
        {PatientMotion.Scare, new[] {"scare","scream","roar","jump"}},
        {PatientMotion.Attack, new[] {"attack","strike","grab","lunge","bite"}},
        {PatientMotion.Appear, new[] {"appear","spawn","rise"}},
        {PatientMotion.Vanish, new[] {"vanish","disappear","fade"}},
        {PatientMotion.Stunned, new[] {"stun","hit","recoil"}}
    };

    public static bool AttachRealModel(LastShiftMonster monster)
    {
        if (monster == null) return false;
        var rig = monster.GetComponent<PatientCharacterRig>();
        if (rig == null) rig = monster.gameObject.AddComponent<PatientCharacterRig>();
        var filename = PickModel();
        if (string.IsNullOrEmpty(filename))
        {
            Debug.LogError("[LastShift / FBX] shabannn.fbx missing under " + ModelFolder);
            return false;
        }

        var importer = AssetImporter.GetAtPath(filename) as ModelImporter;
        if (importer != null &&
            (importer.animationType == ModelImporterAnimationType.Legacy ||
             importer.animationType == ModelImporterAnimationType.None || !importer.importAnimation))
        {
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.importAnimation = true;
            importer.SaveAndReimport();
        }

        var source = AssetDatabase.LoadAssetAtPath<GameObject>(filename);
        if (source == null) return false;
        bool wasActive = monster.gameObject.activeSelf;
        monster.gameObject.SetActive(true);
        try
        {
            var instance = PrefabUtility.InstantiatePrefab(source, monster.transform) as GameObject;
            if (instance == null) return false;
            instance.name = "The Patient - Ghost Girl Animated (real FBX)";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;

            // Vital: imported Sky is an enormous background mesh from Cinema4D.
            // It was responsible for the beige/blue gradient obscuring the hospital.
            var character = FindCharacterBranch(instance.transform);
            if (character == null)
            {
                Debug.LogError("[LastShift / FBX] No actual zombie_Walk character branch. " +
                    "Refusing to render the imported Cinema4D sky as the enemy.");
                UnityEngine.Object.DestroyImmediate(instance);
                return false;
            }
            DisableEverythingExceptBranch(instance.transform, character);

            foreach (var camera in instance.GetComponentsInChildren<Camera>(true)) camera.enabled = false;
            foreach (var light in instance.GetComponentsInChildren<Light>(true)) light.enabled = false;
            foreach (var audio in instance.GetComponentsInChildren<AudioListener>(true)) audio.enabled = false;
            foreach (var audio in instance.GetComponentsInChildren<AudioSource>(true)) audio.enabled = false;
            foreach (var particles in instance.GetComponentsInChildren<ParticleSystem>(true))
            {
                // Do not deactivate a character armature just because an FBX
                // attaches a particle emitter to it.
                particles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                var particleRenderer = particles.GetComponent<ParticleSystemRenderer>();
                if (particleRenderer != null) particleRenderer.enabled = false;
            }

            var meshes = character.GetComponentsInChildren<Renderer>(true)
                .Where(r => r is SkinnedMeshRenderer || (r is MeshRenderer &&
                         r.GetComponent<MeshFilter>() != null)).ToArray();
            if (meshes.Length == 0)
            {
                Debug.LogError("[LastShift / FBX] Selected zombie_Walk branch has no 3D mesh.");
                UnityEngine.Object.DestroyImmediate(instance);
                return false;
            }

            instance.SetActive(true);
            FitAndGround(instance, meshes, 1.72f);
            BindDdsTextures(meshes);
            var animator = instance.GetComponent<Animator>();
            if (animator == null) animator = instance.AddComponent<Animator>();
            animator.applyRootMotion = false;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            var clips = FindClips(filename);
            int nativeMask;
            string report;
            var map = MapClips(clips, out nativeMask, out report);
            animator.runtimeAnimatorController = BuildController(map);
            rig.Configure(instance, animator, nativeMask);
            rig.SetVisible(true);

            Debug.Log("[LastShift / FBX] Character branch: " + character.name +
                " | character renderers: " + meshes.Length +
                " | removed cinematic background/cameras/lights: YES" +
                " | clips: " + clips.Count + "\n" + report);

            if (!rig.IsReady)
            {
                Debug.LogError("[LastShift / FBX] Imported geometry was not usable.");
                return false;
            }
            return true;
        }
        finally
        {
            monster.gameObject.SetActive(wasActive);
        }
    }

    private static string PickModel()
    {
        var preferred = ModelFolder + "/shabannn.fbx";
        if (AssetDatabase.LoadAssetAtPath<GameObject>(preferred) != null) return preferred;
        foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { ModelFolder }))
        {
            var p = AssetDatabase.GUIDToAssetPath(guid);
            if (p.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase)) return p;
        }
        return null;
    }

    private static Transform FindCharacterBranch(Transform root)
    {
        var transforms = root.GetComponentsInChildren<Transform>(true);
        foreach (var t in transforms)
            if (t != root && t.name.Equals("zombie_Walk", StringComparison.OrdinalIgnoreCase))
                return t;
        foreach (var t in transforms)
            if (t != root && (t.name.IndexOf("zombie", StringComparison.OrdinalIgnoreCase) >= 0 ||
                              t.name.IndexOf("ghost", StringComparison.OrdinalIgnoreCase) >= 0) &&
                t.GetComponentsInChildren<SkinnedMeshRenderer>(true).Length > 0)
                return t;
        // No guess based on the sky mesh's renderer size: refuse the unsafe import.
        return null;
    }

    private static void DisableEverythingExceptBranch(Transform parent, Transform character)
    {
        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            if (child == character) { child.gameObject.SetActive(true); continue; }
            if (character.IsChildOf(child))
            {
                child.gameObject.SetActive(true);
                DisableEverythingExceptBranch(child, character);
            }
            else child.gameObject.SetActive(false);
        }
        // Renderers and scripts on ancestors of character may include sky geometry.
        for (var current = character.parent; current != null; current = current.parent)
        {
            foreach (var r in current.GetComponents<Renderer>()) r.enabled = false;
            foreach (var l in current.GetComponents<Light>()) l.enabled = false;
        }
    }

    private static void FitAndGround(GameObject instance, Renderer[] renderers, float height)
    {
        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        if (bounds.size.y < .0001f || bounds.size.y > 10000f) return;
        float ratio = Mathf.Clamp(height / bounds.size.y, .0001f, 500f);
        instance.transform.localScale *= ratio;
        bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        instance.transform.position +=
            new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z);
        Debug.Log("[LastShift / FBX] Grounded character at 0 and scaled to 1.72m. Ratio: " + ratio);
    }

    private static void BindDdsTextures(Renderer[] renderers)
    {
        if (!AssetDatabase.IsValidFolder(Generated))
            AssetDatabase.CreateFolder(Folder, "Generated");

        var sets = new [] {
            new [] {"ghost_head_d1.dds","ghost_head_n1.dds","ghost_head_s1.dds"},
            new [] {"ghost_dress_d.dds","ghost_dress_n.dds","ghost_dress_s.dds"},
            new [] {"hair_02_d.dds","hair_02_n.dds","hair_02_s.dds"}
        };
        var groupLabels = new [] {"Head", "Dress", "Hair"};
        Texture2D[][] maps = new Texture2D[3][];
        for (int group = 0; group < 3; group++)
        {
            maps[group] = new Texture2D[3];
            for (int map = 0; map < 3; map++)
            {
                string path = TextureFolder + "/" + sets[group][map];
                if (map == 1)
                {
                    var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null && importer.textureType != TextureImporterType.NormalMap)
                    {
                        importer.textureType = TextureImporterType.NormalMap;
                        importer.SaveAndReimport();
                    }
                }
                maps[group][map] = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
                if (maps[group][map] == null)
                    Debug.LogWarning("[LastShift / DDS] Texture unavailable: " + path);
            }
        }

        int materialOrdinal = 0;
        foreach (Renderer renderer in renderers)
        {
            var originals = renderer.sharedMaterials;
            var mapped = new Material[originals.Length];
            for (int slot = 0; slot < originals.Length; slot++, materialOrdinal++)
            {
                var original = originals[slot];
                string descriptor = (renderer.name + " " + (original != null ? original.name : "") +
                    " " + (original != null && original.mainTexture != null ? original.mainTexture.name : "")).ToLowerInvariant();
                int category = -1;
                if (descriptor.Contains("hair") || descriptor.Contains("capelli")) category = 2;
                else if (descriptor.Contains("dress") || descriptor.Contains("cloth") ||
                         descriptor.Contains("gown") || descriptor.Contains("shirt")) category = 1;
                else if (descriptor.Contains("head") || descriptor.Contains("face") ||
                         descriptor.Contains("skin") || descriptor.Contains("body")) category = 0;

                // If FBX material labels are generic, use slot order as best effort
                // but log exactly what was chosen for correction in Unity Inspector.
                if (category < 0) category = materialOrdinal % 3;
                string assetPath = Generated + "/Patient_" + groupLabels[category] + "_Material_" + materialOrdinal + ".mat";
                Material m = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
                if (m == null)
                {
                    m = new Material(Ward13UrpMaterialTools.LitShader());
                    m.name = "The Patient " + groupLabels[category] + " (" + slot + ")";
                    AssetDatabase.CreateAsset(m, assetPath);
                }
                m.shader = Ward13UrpMaterialTools.LitShader();
                var diffuse = maps[category][0];
                var normal = maps[category][1];
                var spec = maps[category][2];
                // Prefer the artist's FBX diffuse when different from expected DDS.
                if (original != null && original.mainTexture != null)
                    diffuse = original.mainTexture as Texture2D ?? diffuse;
                if (m.HasProperty("_BaseMap")) m.SetTexture("_BaseMap", diffuse);
                if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", Color.white);
                if (normal != null && m.HasProperty("_BumpMap"))
                {
                    m.SetTexture("_BumpMap", normal);
                    m.EnableKeyword("_NORMALMAP");
                }
                if (spec != null && m.HasProperty("_SpecGlossMap"))
                {
                    m.SetFloat("_WorkflowMode", 0f); // Specular workflow
                    m.SetTexture("_SpecGlossMap", spec);
                    m.EnableKeyword("_SPECULAR_SETUP");
                    m.EnableKeyword("_SPECGLOSSMAP");
                }
                if (category == 2)
                {
                    // Hair alpha is in the original DDS diffuse.
                    m.SetFloat("_AlphaClip", 1f);
                    m.SetFloat("_Cutoff", .33f);
                    m.EnableKeyword("_ALPHATEST_ON");
                    m.SetFloat("_Cull", 0f);
                }
                else
                {
                    m.SetFloat("_AlphaClip", 0f);
                    m.DisableKeyword("_ALPHATEST_ON");
                    m.SetFloat("_Cull", 2f);
                }
                EditorUtility.SetDirty(m);
                mapped[slot] = m;
                Debug.Log("[LastShift / DDS] " + renderer.name + "/" + slot +
                    " : " + (original != null ? original.name : "(none)") + " -> " +
                    groupLabels[category] + " | diffuse=" + (diffuse != null ? diffuse.name : "MISSING"));
            }
            renderer.sharedMaterials = mapped;
        }
    }

    private static List<AnimationClip> FindClips(string modelPath)
    {
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { modelPath };
        foreach (var guid in AssetDatabase.FindAssets("t:AnimationClip", new[] { AnimFolder }))
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));
        foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { AnimFolder }))
            paths.Add(AssetDatabase.GUIDToAssetPath(guid));
        var result = new List<AnimationClip>();
        foreach (var p in paths)
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(p))
                if (obj is AnimationClip clip && !clip.legacy &&
                    !clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase))
                    result.Add(clip);
        return result;
    }

    private static Dictionary<PatientMotion, AnimationClip> MapClips(
        List<AnimationClip> clips, out int nativeMask, out string report)
    {
        nativeMask = 0;
        var result = new Dictionary<PatientMotion, AnimationClip>();
        var sb = new StringBuilder("FBX animation clips: " +
            (clips.Count == 0 ? "none" : string.Join(", ", clips.Select(x=>x.name).ToArray())) + "\n");
        foreach (var motion in Motions)
        {
            AnimationClip match = null;
            foreach (var alias in Aliases[motion])
            {
                match = clips.FirstOrDefault(x => x.name.Equals(alias, StringComparison.OrdinalIgnoreCase));
                if (match == null)
                    match = clips.FirstOrDefault(x => x.name.IndexOf(alias, StringComparison.OrdinalIgnoreCase) >= 0);
                if (match != null) break;
            }
            if (match != null)
            {
                nativeMask |= 1 << (int)motion;
                result[motion] = match;
            }
            // A genuine walk clip can be sped up for the running state.
            else if (motion == PatientMotion.Run && result.ContainsKey(PatientMotion.Walk))
                result[motion] = result[PatientMotion.Walk];
            else if (motion == PatientMotion.Walk && clips.Count == 1 &&
                     clips[0].name.IndexOf("take", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                // Cinema4D commonly exports its one walking take as "Take 001".
                // Restrict this inference to locomotion; never label it an attack.
                result[motion] = clips[0];
            }
            sb.AppendLine(motion + " = " +
                (result.TryGetValue(motion, out var selected) ? selected.name : "unavailable"));
        }
        report = sb.ToString();
        return result;
    }

    private static AnimatorController BuildController(Dictionary<PatientMotion, AnimationClip> map)
    {
        if (!AssetDatabase.IsValidFolder(Generated))
            AssetDatabase.CreateFolder(Folder, "Generated");
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath) != null)
            AssetDatabase.DeleteAsset(ControllerPath);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        var machine = controller.layers[0].stateMachine;
        AnimatorState idle = null;
        foreach (var motion in Motions)
        {
            var state = machine.AddState(motion.ToString());
            if (map.TryGetValue(motion, out var clip)) state.motion = clip;
            state.speed = motion == PatientMotion.Run ? 1.4f :
                          motion == PatientMotion.Walk ? .85f : 1f;
            if (motion == PatientMotion.Idle) idle = state;
        }
        if (idle != null) machine.defaultState = idle;
        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }
}
#endif