#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// Creates a unique seeded albedo map in the editor for each v0.3 hospital
/// surface. Generated Texture2D objects are saved as assets and survive restarts.
/// Procedural approximation only; not scanned photogrammetry / PBR textures.
/// </summary>
public static class PatientSurfaceGenerator
{
    private const string Folder = "Assets/LastShift/GeneratedMaterials";

    public static void Apply(Material material, string name, Color baseColor)
    {
        if (material == null) return;
        bool shouldTexture = name.Contains("Concrete") || name.Contains("Mould") ||
            name.Contains("FloorCeramic") || name.Contains("Corroded") ||
            name.Contains("Rubber") || name.Contains("BrownRed") ||
            name.Contains("WhitePaint") || name.Contains("BoneIvory");
        if (!shouldTexture) return;

        var path = Folder + "/" + name + "_albedo.asset";
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (texture == null)
        {
            texture = Bake(name, baseColor, 256);
            AssetDatabase.CreateAsset(texture, path);
        }

        if (material.HasProperty("_BaseMap"))
            material.SetTexture("_BaseMap", texture);
        if (material.HasProperty("_MainTex"))
            material.SetTexture("_MainTex", texture);
    }

    private static Texture2D Bake(string name, Color tint, int size)
    {
        int seed = 17;
        foreach (char c in name) seed = unchecked(seed * 31 + c);
        float sx = (seed & 1023) * .0123f;
        float sy = ((seed >> 10) & 1023) * .0153f;

        var result = new Texture2D(size, size, TextureFormat.RGBA32, true, false);
        result.name = name + "_procedural_albedo";
        result.wrapMode = TextureWrapMode.Repeat;
        result.filterMode = FilterMode.Bilinear;
        result.anisoLevel = 4;

        Color[] pixels = new Color[size * size];
        bool corrosion = name.Contains("Corroded") || name.Contains("BrownRed");
        bool tile = name.Contains("FloorCeramic");
        bool mould = name.Contains("Mould");
        bool rubber = name.Contains("Rubber");

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float u = x / (float)size;
                float v = y / (float)size;
                float broad = Mathf.PerlinNoise(u * 5f + sx, v * 5f + sy);
                float mid = Mathf.PerlinNoise(u * 26f + sx * 2.4f, v * 26f + sy * 2.4f);
                float fine = Mathf.PerlinNoise(u * 95f + sx * 7f, v * 95f + sy * 7f);
                float n = (broad - .5f) * .22f + (mid - .5f) * .19f +
                    (fine - .5f) * .11f;
                Color col = tint * (1f + n);

                if (corrosion)
                {
                    float fleck = Mathf.PerlinNoise(u * 38f + sx + 25f,
                        v * 37f + sy + 41f);
                    if (fleck > .68f) col = Color.Lerp(col,
                        new Color(.40f, .19f, .095f), (fleck - .68f) * 2.5f);
                    if (fleck < .25f) col *= .55f;
                }
                else if (mould)
                {
                    float stain = Mathf.PerlinNoise(u * 9f + sx + 31f,
                        v * 13f + sy + 12f);
                    if (stain > .48f) col = Color.Lerp(col,
                        new Color(.026f, .058f, .055f), (stain - .48f) * .85f);
                }
                else if (tile)
                {
                    float stain = Mathf.PerlinNoise(u * 7f + sx + 14f,
                        v * 8f + sy + 34f);
                    if (stain > .67f) col = Color.Lerp(col,
                        new Color(.087f, .080f, .078f), (stain - .67f) * 1.9f);
                    // Very fine micro-scratches, not full tile grout (geometry supplies gaps).
                    float scratch = Mathf.Abs(Mathf.Sin(u * 218f + broad * 5f));
                    if (scratch > .996f) col *= .79f;
                }
                else if (rubber) col *= .78f + fine * .25f;

                col.a = 1f;
                pixels[y * size + x] = col;
            }
        result.SetPixels(pixels);
        result.Apply(true, false);
        return result;
    }
}
#endif
