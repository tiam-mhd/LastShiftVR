#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>Reusable URP preflight and shader/material restoration for regenerated wards.</summary>
public static class Ward13UrpMaterialTools
{
    public const string LitShaderName = "Universal Render Pipeline/Lit";

    public static bool ValidateEnvironment()
    {
        var active = GraphicsSettings.currentRenderPipeline;
        var shader = Shader.Find(LitShaderName);
        if (active == null || active.GetType().Name.IndexOf("Universal", System.StringComparison.OrdinalIgnoreCase) < 0)
        {
            Debug.LogError("[LastShift] URP is not the active render pipeline. " +
                "Project Settings > Graphics / Quality must use a Universal Render Pipeline Asset.");
            EditorUtility.DisplayDialog("LastShift requires URP",
                "The active render pipeline is not URP. In Project Settings > Graphics and Quality, " +
                "assign a Universal Render Pipeline Asset, then rebuild the Ward 13 scene. " +
                "Do not build into an incompatible pipeline.", "OK");
            return false;
        }
        if (shader == null || !shader.isSupported)
        {
            Debug.LogError("[LastShift] The URP/Lit shader is missing or unsupported.");
            EditorUtility.DisplayDialog("LastShift URP Shader Missing",
                "The Universal Render Pipeline/Lit shader is missing or unsupported. " +
                "Check Package Manager > Universal RP, then Assets > Reimport before building.", "OK");
            return false;
        }
        return true;
    }

    public static Shader LitShader()
    {
        var shader = Shader.Find(LitShaderName);
        if (shader == null || !shader.isSupported)
            throw new System.InvalidOperationException("URP/Lit shader missing. Fix your URP package and Graphics settings.");
        return shader;
    }

    public static void Repair(Material material, Color color, Color? emission = null, float roughness = -1f)
    {
        if (material == null) return;
        var shader = LitShader();
        if (material.shader != shader || material.shader == null || !material.shader.isSupported)
        {
            material.shader = shader;
            Debug.Log("[LastShift] Repaired missing/built-in shader on: " + material.name);
        }

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);
        if (roughness >= 0f && material.HasProperty("_Smoothness"))
            material.SetFloat("_Smoothness", 1f - Mathf.Clamp01(roughness));
        if (emission.HasValue && material.HasProperty("_EmissionColor"))
        {
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", emission.Value);
        }
        EditorUtility.SetDirty(material);
    }
}
#endif
