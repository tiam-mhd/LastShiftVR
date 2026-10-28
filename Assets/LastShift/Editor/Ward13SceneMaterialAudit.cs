#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using LastShift;

/// <summary>
/// An edit-mode, deterministic final-scene audit. Its only job is to make sure
/// scene renderers cannot use missing/Built-in shaders under URP. It does not
/// touch original FBX assets or mutate their embedded materials.
/// </summary>
public static class Ward13SceneMaterialAudit
{
    private const string GeneratedRoot = "Assets/LastShift/GeneratedMaterials";
    private const string RepairFolder = GeneratedRoot + "/SceneShaderRepairs";

    public static void RepairAndReport()
    {
        var urp = Ward13UrpMaterialTools.LitShader();
        if (!AssetDatabase.IsValidFolder(GeneratedRoot))
            AssetDatabase.CreateFolder("Assets/LastShift", "GeneratedMaterials");
        if (!AssetDatabase.IsValidFolder(RepairFolder))
            AssetDatabase.CreateFolder(GeneratedRoot, "SceneShaderRepairs");

        // This includes inactive enemies, which need compatible materials
        // before their first appearance.
        Renderer[] renderers = UnityEngine.Object.FindObjectsByType<Renderer>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        int repaired = 0;
        int nullSlots = 0;
        int hiddenBackgrounds = 0;
        var used = new Dictionary<string, Material>(StringComparer.Ordinal);
        foreach (Renderer renderer in renderers)
        {
            if (renderer == null) continue;

            // The imported Cinema4D sky must NEVER cover the hospital or create
            // an enormous magenta quad at runtime.
            if (IsImportedPatientSky(renderer))
            {
                renderer.enabled = false;
                hiddenBackgrounds++;
                continue;
            }

            // TMPro, legacy TextMesh, particle/line graphics and other specialized
            // renderers need their own shaders, not the scene's URP/Lit shader.
            if (renderer is ParticleSystemRenderer ||
                renderer is LineRenderer || renderer is TrailRenderer ||
                renderer.GetComponent<TextMesh>() != null ||
                renderer.GetComponent<CanvasRenderer>() != null)
                continue;

            Material[] materials = renderer.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < materials.Length; i++)
            {
                Material src = materials[i];
                bool invalid = src == null || src.shader == null || !src.shader.isSupported ||
                    src.shader.name == "Hidden/InternalErrorShader" ||
                    (!src.shader.name.StartsWith("Universal Render Pipeline/",
                        StringComparison.OrdinalIgnoreCase) &&
                     !src.shader.name.StartsWith("Shader Graphs/",
                        StringComparison.OrdinalIgnoreCase));

                if (!invalid) continue;

                if (src == null) nullSlots++;
                string key = (src != null ? AssetDatabase.GetAssetPath(src) + "_" + src.name :
                    renderer.name) + "_slot_" + i;
                if (!used.TryGetValue(key, out Material replacement))
                {
                    string filename = "URP_Repair_" + HashKey(key) + ".mat";
                    string path = RepairFolder + "/" + filename;
                    replacement = AssetDatabase.LoadAssetAtPath<Material>(path);
                    if (replacement == null)
                    {
                        replacement = new Material(urp);
                        replacement.name = "LastShift URP repair - " + filename;
                        AssetDatabase.CreateAsset(replacement, path);
                    }
                    replacement.shader = urp;

                    Texture albedo = null;
                    Color tint = Color.white;
                    if (src != null)
                    {
                        if (src.HasProperty("_BaseMap"))
                            albedo = src.GetTexture("_BaseMap");
                        if (albedo == null && src.HasProperty("_MainTex"))
                            albedo = src.GetTexture("_MainTex");
                        if (src.HasProperty("_BaseColor"))
                            tint = src.GetColor("_BaseColor");
                        else if (src.HasProperty("_Color"))
                            tint = src.GetColor("_Color");
                    }
                    if (replacement.HasProperty("_BaseMap"))
                        replacement.SetTexture("_BaseMap", albedo);
                    if (replacement.HasProperty("_BaseColor"))
                        replacement.SetColor("_BaseColor", tint);
                    EditorUtility.SetDirty(replacement);
                    used[key] = replacement;
                }
                materials[i] = replacement;
                changed = true;
                repaired++;
            }
            if (changed)
            {
                renderer.sharedMaterials = materials;
                EditorUtility.SetDirty(renderer);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("[LastShift / Render Audit] Renderers checked: " + renderers.Length +
            ", repaired URP material slots: " + repaired + ", missing slots: " +
            nullSlots + ", disabled external FBX skies: " + hiddenBackgrounds +
            ". If magenta persists, inspect URP pipeline, GPU Resident Drawer and shader errors.");
    }

    private static bool IsImportedPatientSky(Renderer renderer)
    {
        // Avoid disabling legitimate hospital architecture merely named "Sky".
        Transform t = renderer.transform;
        bool importedPatient = false;
        bool cinematicSky = false;
        while (t != null)
        {
            if (t.name.StartsWith("The Patient - Ghost Girl Animated", StringComparison.Ordinal))
                importedPatient = true;
            if (t.name.Equals("Sky", StringComparison.OrdinalIgnoreCase) ||
                t.name.Equals("Background", StringComparison.OrdinalIgnoreCase))
                cinematicSky = true;
            t = t.parent;
        }
        return importedPatient && cinematicSky;
    }

    private static string HashKey(string value)
    {
        unchecked
        {
            uint hash = 2166136261;
            foreach (char ch in value)
                hash = (hash ^ ch) * 16777619;
            return hash.ToString("X8");
        }
    }
}
#endif