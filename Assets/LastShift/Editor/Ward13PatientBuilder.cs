#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using LastShift;

/// <summary>
/// The Patient v0.3. Rebuilds the disposable Ward13_Prototype scene from scratch.
/// The layout includes one corridor, a side treatment room, a surgery ward,
/// three fuse stations, a four-interaction power panel and a ghost encounter.
/// </summary>
public static class Ward13PatientBuilder
{
    private const string Base = "Assets/LastShift";
    private static Transform world;
    private static Material concrete, grime, trim, metal, ceramic, blood, pale, rubber;
    private static Material rust, red, amber, powered, unpowered, frosted;

    [MenuItem("Tools/Last Shift/Generate Ward 13 - The Patient v0.3")]
    public static void Build()
    {
        // The v0.2 scene builder intentionally creates a new scene. Warn users to save
        // any manual changes BEFORE running this generator.
        Ward13AtmosphereBuilder.Build();

        var existing = GameObject.Find("WARD 13");
        var systems = GameObject.Find("GAME SYSTEMS");
        if (existing == null || systems == null)
        {
            Debug.LogError("[LastShift v0.3] Base scene not available; fix Console errors first.");
            return;
        }
        world = existing.transform;
        var game = systems.GetComponent<LastShiftGame>();
        var monster = game != null ? game.monster : null;
        if (game == null || monster == null) return;

        concrete = MakeMaterial("Patient_Concrete", new Color(.15f, .18f, .18f), .93f);
        grime = MakeMaterial("Patient_MouldAndStains", new Color(.095f, .13f, .13f), .88f);
        trim = MakeMaterial("Patient_WhitePaint", new Color(.46f, .49f, .47f), .85f);
        metal = MakeMaterial("Patient_BlackSteel", new Color(.085f, .12f, .14f), .61f);
        ceramic = MakeMaterial("Patient_FloorCeramic", new Color(.28f, .30f, .28f), .35f);
        blood = MakeMaterial("Patient_BrownRedStain", new Color(.25f, .052f, .04f), .9f);
        pale = MakeMaterial("Patient_BoneIvory", new Color(.62f, .65f, .62f), .72f);
        rubber = MakeMaterial("Patient_Rubber", new Color(.025f, .033f, .041f), .96f);
        rust = MakeMaterial("Patient_CorrodedSteel", new Color(.26f, .11f, .078f), .91f);
        red = MakeMaterial("Patient_RedEmergency", new Color(.66f, .032f, .021f), .39f,
            new Color(.4f, .022f, .015f));
        amber = MakeMaterial("Patient_AmberFuse", new Color(.88f, .33f, .045f), .37f,
            new Color(.38f, .12f, .012f));
        powered = MakeMaterial("Patient_GreenFuse", new Color(.055f, .75f, .38f), .3f,
            new Color(.025f, .32f, .11f));
        unpowered = MakeMaterial("Patient_EmptyFuse", new Color(.033f, .047f, .055f), .86f);
        frosted = MakeMaterial("Patient_FrostedGlass", new Color(.14f, .24f, .25f), .35f);

        // Restore corridor walls as segmented walls, allowing real openings for two rooms.
        RemoveNamed(world, "Wall left", "Wall right", "Hospital wall wainscot", "Handrail",
            "Hospital baseboard");
        WallSegment(-2.59f, -12f, 6.4f);
        WallSegment(-2.59f, 8.1f, 16f);
        WallSegment(2.59f, -12f, 1.0f);
        WallSegment(2.59f, 2.7f, 16f);

        // Door at x=+2.6,z=1.85 and x=-2.6,z=7.25. Decorative side doors
        // at these two locations are removed to keep openings visible.
        RemoveDoorDecoration(2.36f, 1.85f);
        RemoveDoorDecoration(-2.36f, 7.25f);

        // Build two walkable rooms outside the original corridor.
        Room("WARD 12 / TREATMENT", 1, 1.85f, -1.3f, 5f);
        Room("WARD 14 / SURGERY", -1, 7.25f, 4.1f, 10.4f);
        TreatmentRoom();
        SurgeryRoom();

        // Hospital details on corridor itself. Geometry is procedural, not photoreal art.
        for (int i = 0; i < 9; i++)
        {
            float z = -10.2f + i * 3.1f;
            Box("Ceiling damage / duct", new Vector3(-1.2f, 2.96f, z),
                new Vector3(.77f, .08f, .55f), grime, world, false);
            Box("Wall dark water stain", new Vector3(i % 2 == 0 ? -2.43f : 2.43f,
                .45f, z + .6f), new Vector3(.034f, .52f, .85f), grime, world, false);
        }
        // Keep the original output understandable for players: move three pickups
        // into the actual rooms and rebuild matching stations.
        RemoveNamed(world, "Fuse pedestal", "Fuse pedestal indicator", "Fuse marker light");
        var fuses = new List<LastShiftFuse>(world.GetComponentsInChildren<LastShiftFuse>(true));
        fuses.Sort((a, b) => a.name.CompareTo(b.name));
        var fusePositions = new [] {
            new Vector3(1.3f, 1.17f, -1.1f),
            new Vector3(5.2f, 1.17f, 1.15f),
            new Vector3(-5.2f, 1.17f, 7.85f)
        };
        for (int i = 0; i < fuses.Count && i < 3; i++)
        {
            fuses[i].transform.position = fusePositions[i];
            FuseStation(fusePositions[i], i + 1);
            // The existing orange blocks represent pickup colliders; enrich their silhouette.
            var body = fuses[i].transform;
            body.localScale = new Vector3(.26f, .37f, .20f);
            Box("Ceramic cartridge edge", body.position + Vector3.up * .17f,
                new Vector3(.30f, .05f, .23f), pale, body, false);
        }

        // The panel uses the existing IVRUse interface and registered game code.
        var panel = world.GetComponentInChildren<LastShiftConsole>();
        if (panel != null)
        {
            var visual = panel.gameObject.AddComponent<FusePanelVisual>();
            visual.emptyMaterial = unpowered;
            visual.insertedMaterial = amber;
            visual.powerMaterial = powered;
            visual.slots = new Renderer[3];
            for (int i = 0; i < 3; i++)
            {
                var o = Box("Fuse socket " + (i + 1), new Vector3(1.87f, 1.23f, -7.26f + i * .23f),
                    new Vector3(.08f, .35f, .13f), unpowered, world, false);
                visual.slots[i] = o.GetComponent<Renderer>();
            }
            visual.breakerLever = Box("Main breaker", new Vector3(1.81f, .94f, -6.98f),
                new Vector3(.14f, .35f, .09f), rust, world, false).transform;
            Box("Panel overhead status lamp", new Vector3(1.93f, 1.76f, -7f),
                new Vector3(.09f, .09f, .29f), red, world, false);
            game.fusePanel = visual;
        }

        // Replace primitive monster with actual supplied reference art rendered on
        // a vertically standing transparent quad. Not a fully rigged model.
        for (int i = monster.transform.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(monster.transform.GetChild(i).gameObject);
        var col = monster.GetComponent<CapsuleCollider>();
        if (col == null) col = monster.gameObject.AddComponent<CapsuleCollider>();
        col.center = new Vector3(0f, .95f, 0f);
        col.height = 1.9f;
        col.radius = .35f;
        var visualGhost = monster.gameObject.AddComponent<GhostGirlVisual>();
        visualGhost.height = 1.83f;
        visualGhost.preparedMaterial = GhostVisibilityMaterial();
        monster.gameObject.AddComponent<PatientPathChase>();
        monster.chaseSpeed = 1.12f;

        var director = systems.AddComponent<GhostDirector>();
        director.game = game;
        director.monster = monster;
        director.visual = visualGhost;
        game.ghostDirector = director;

        // Dark, legible night lighting, keeping budget modest for PCVR.
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.055f, .071f, .078f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(.02f, .038f, .043f);
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = .037f;
        foreach (var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light.type != LightType.Point) continue;
            light.intensity *= .65f;
            light.shadows = LightShadows.None;
        }
        // Soft lights near room puzzles for gameplay readability.
        RoomLight("Treatment lamp", new Vector3(5f, 2.45f, 1.85f),
            new Color(.42f, .80f, .83f), 1f, 4.4f);
        RoomLight("Surgery emergency lamp", new Vector3(-5.1f, 2.5f, 7.3f),
            new Color(1f, .15f, .12f), .68f, 4.7f);
        RoomLight("Power breaker highlight", new Vector3(1.6f, 1.7f, -7f),
            new Color(1f, .48f, .09f), .8f, 2.5f);
        ConnectElectricCircuits(systems, game);

        if (Camera.main != null)
        {
            Camera.main.nearClipPlane = .045f;
            Camera.main.farClipPlane = 72f;
        }
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[LastShift v0.3] The Patient, two wards, fuse panel, horror events generated. Press Play.");
    }

    private static void RemoveDoorDecoration(float x, float z)
    {
        // Decorative fake doors from v0.2 use no colliders, but visually cover
        // the only real entrance to the new side rooms.
        var all = world.GetComponentsInChildren<Transform>(true);
        foreach (var t in all)
        {
            if (t == null || t == world) continue;
            if (Mathf.Abs(t.position.x - x) > .35f || Mathf.Abs(t.position.z - z) > .78f)
                continue;
            if (t.name.StartsWith("Ward door panel") || t.name.StartsWith("Door frame")
                || t.name.StartsWith("Door handle") || t.name.StartsWith("Room indicator")
                || t.name.StartsWith("Concrete vertical rib"))
                Object.DestroyImmediate(t.gameObject);
        }
    }

    private static void RemoveNamed(Transform parent, params string[] names)
    {
        foreach (var t in parent.GetComponentsInChildren<Transform>(true))
        {
            if (t == null || t == parent) continue;
            foreach (var name in names)
                if (t.name == name)
                {
                    Object.DestroyImmediate(t.gameObject);
                    break;
                }
        }
    }

    private static void WallSegment(float x, float a, float b)
    {
        var mid = (a + b) / 2f;
        var len = b - a;
        Box("Segmented ward wall", new Vector3(x, 1.55f, mid),
            new Vector3(.28f, 3.1f, len), concrete, world, true);
        Box("Medical white lower wall panel", new Vector3(x - Mathf.Sign(x) * .15f, .6f, mid),
            new Vector3(.045f, 1.1f, len), trim, world, false);
        Box("Baseboard", new Vector3(x - Mathf.Sign(x) * .16f, .12f, mid),
            new Vector3(.06f, .24f, len), metal, world, false);
    }

    private static void Room(string name, int side, float entryZ, float startZ, float endZ)
    {
        var room = new GameObject(name).transform;
        room.SetParent(world);
        float cx = side * 5.35f;
        float midZ = (startZ + endZ) * .5f;
        float length = endZ - startZ;
        Box("Ward floor", new Vector3(cx, -.12f, midZ),
            new Vector3(5.7f, .22f, length), ceramic, room, true);
        Box("Ward ceiling", new Vector3(cx, 3.17f, midZ),
            new Vector3(5.7f, .24f, length), grime, room, true);
        Box("Ward outer wall", new Vector3(side * 8.18f, 1.5f, midZ),
            new Vector3(.24f, 3f, length), concrete, room, true);
        Box("Ward north wall", new Vector3(cx, 1.5f, endZ),
            new Vector3(5.7f, 3f, .24f), concrete, room, true);
        Box("Ward south wall", new Vector3(cx, 1.5f, startZ),
            new Vector3(5.7f, 3f, .24f), concrete, room, true);
        // Opposite wall around door opening
        float x = side * 2.61f;
        if (entryZ - .9f > startZ)
            Box("Room inner wall section", new Vector3(x, 1.5f, (startZ + entryZ - .9f) * .5f),
                new Vector3(.24f, 3f, entryZ - .9f - startZ), concrete, room, true);
        if (entryZ + .9f < endZ)
            Box("Room inner wall section", new Vector3(x, 1.5f, (entryZ + .9f + endZ) * .5f),
                new Vector3(.24f, 3f, endZ - entryZ - .9f), concrete, room, true);

        for (int i = 0; i < 5; i++)
        {
            var z = startZ + .5f + i * (length - 1f) / 4f;
            Box("Ward wall streak", new Vector3(side * 8.025f, .84f, z),
                new Vector3(.025f, .84f, .30f), grime, room, false);
        }

        Box("Door frame lintel", new Vector3(side * 2.6f, 2.37f, entryZ),
            new Vector3(.38f, .24f, 1.95f), metal, room, false);
        for (int s = -1; s <= 1; s += 2)
            Box("Door frame upright", new Vector3(side * 2.6f, 1.16f, entryZ + s * .95f),
                new Vector3(.38f, 2.33f, .13f), metal, room, false);
        Box("Ward illuminated room label", new Vector3(side * 2.45f, 2.48f, entryZ),
            new Vector3(.05f, .07f, .95f), amber, room, false);
    }

    private static void TreatmentRoom()
    {
        // Avoid intersecting the fuse at x=5.2,z=1.15.
        Bed(new Vector3(6.8f, 0, -.13f));
        for (int i = 0; i < 3; i++)
            Box("Medical cabinet", new Vector3(7.52f, .72f, 1.0f + i * 1.05f),
                new Vector3(.45f, 1.44f, .84f), i == 1 ? rust : pale, world, true);
        Box("Treatment desk", new Vector3(4.12f, .84f, 3.9f),
            new Vector3(1.1f, .14f, .65f), metal, world, true);
        Box("Abandoned instruments", new Vector3(4.05f, .94f, 3.8f),
            new Vector3(.8f, .045f, .45f), rust, world, false);
        Box("Old monitor", new Vector3(7.92f, 1.74f, 3.6f),
            new Vector3(.11f, .75f, .87f), metal, world, false);
        Box("Monitor ghost glow", new Vector3(7.84f, 1.74f, 3.6f),
            new Vector3(.04f, .52f, .66f), frosted, world, false);
    }

    private static void SurgeryRoom()
    {
        Bed(new Vector3(-6.38f, 0, 6.2f));
        Box("Surgery instrument trolley", new Vector3(-4.15f, .88f, 9.3f),
            new Vector3(1.15f, .11f, .55f), metal, world, true);
        for (int i = 0; i < 6; i++)
            Box("Loose surgical tool", new Vector3(-4.5f + i * .13f, .96f, 9.3f),
                new Vector3(.035f, .04f, .29f), pale, world, false);
        Box("Surgical light boom", new Vector3(-6.2f, 2.42f, 6.3f),
            new Vector3(.12f, .84f, .12f), metal, world, false);
        Box("Damaged surgical lamp", new Vector3(-6.2f, 2.02f, 6.3f),
            new Vector3(.87f, .12f, .87f), pale, world, false);
        Box("Surgery wall stain", new Vector3(-8.02f, 1.4f, 9.2f),
            new Vector3(.035f, 1.45f, .8f), blood, world, false);
        for (int k = 0; k < 4; k++)
            Box("Ripped hospital curtain", new Vector3(-3.08f, 1.5f, 5.0f + k * .37f),
                new Vector3(.05f, 2.1f - (k % 2) * .45f, .30f), pale, world, false);
    }

    private static void Bed(Vector3 p)
    {
        Box("Metal bed frame", p + new Vector3(0, .61f, 0),
            new Vector3(1.15f, .10f, 2f), metal, world, true);
        Box("Hospital mattress", p + new Vector3(0, .73f, 0),
            new Vector3(1.03f, .15f, 1.88f), pale, world, false);
        Box("Soiled sheet", p + new Vector3(.07f, .815f, .31f),
            new Vector3(.82f, .02f, .52f), blood, world, false);
        for (int s = -1; s <= 1; s += 2)
            for (int end = -1; end <= 1; end += 2)
                Box("Gurney post", p + new Vector3(s * .48f, .30f, end * .8f),
                    new Vector3(.09f, .55f, .09f), metal, world, false);
        Box("Gurney pillow", p + new Vector3(0f, .85f, -.62f),
            new Vector3(.74f, .14f, .33f), pale, world, false);
    }

    private static void FuseStation(Vector3 point, int number)
    {
        var pos = new Vector3(point.x, .58f, point.z);
        Box("Fuse cabinet " + number, pos, new Vector3(.82f, 1.1f, .70f),
            metal, world, false);
        Box("Fuse cabinet glowing trim " + number, pos + new Vector3(0f, .57f, 0f),
            new Vector3(.67f, .055f, .57f), amber, world, false);
        Box("Ceramic panel " + number, pos + new Vector3(0f, .11f, .38f),
            new Vector3(.49f, .52f, .045f), trim, world, false);
        RoomLight("Fuse " + number + " small lamp", point + new Vector3(0f, .23f, 0f),
            new Color(1f, .43f, .14f), .5f, 2.15f);
    }

    private static void RoomLight(string name, Vector3 p, Color color, float intensity, float range)
    {
        var go = new GameObject(name);
        go.transform.SetParent(world);
        go.transform.position = p;
        var light = go.AddComponent<Light>();
        light.type = LightType.Point;
        light.intensity = intensity;
        light.range = range;
        light.color = color;
        light.shadows = LightShadows.None;
    }

    private static GameObject Box(string name, Vector3 p, Vector3 scale,
        Material material, Transform parent, bool solid)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.position = p;
        go.transform.localScale = scale;
        go.GetComponent<Renderer>().sharedMaterial = material;
        if (!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
        return go;
    }


    /// <summary>Explicitly referenced transparent material avoids URP shader stripping.</summary>
    private static Material GhostVisibilityMaterial()
    {
        const string directory = "Assets/LastShift/GeneratedMaterials";
        if (!AssetDatabase.IsValidFolder(directory))
            AssetDatabase.CreateFolder(Base, "GeneratedMaterials");
        const string path = directory + "/ThePatient_Visible.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader == null) shader = Shader.Find("Unlit/Transparent");
            if (shader == null) throw new System.InvalidOperationException("No transparent shader for The Patient");
            material = new Material(shader) { name = "The Patient - Always Visible" };
            AssetDatabase.CreateAsset(material, path);
        }
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(
            "Assets/LastShift/Resources/GhostGirlReference.png");
        if (texture == null)
            Debug.LogError("[LastShift] Missing GhostGirlReference.png. Download the current Assets/LastShift folder.");
        if (material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
        if (material.HasProperty("_MainTex")) material.SetTexture("_MainTex", texture);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", Color.white);
        if (material.HasProperty("_Color")) material.SetColor("_Color", Color.white);
        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Blend", 0f);
            material.SetFloat("_SrcBlend", 5f);
            material.SetFloat("_DstBlend", 10f);
            material.SetFloat("_ZWrite", 0f);
            material.SetFloat("_Cull", 0f);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }
        material.renderQueue = 3000;
        EditorUtility.SetDirty(material);
        return material;
    }

    /// <summary>Bind real-time point lights to the three physical fuse circuits.</summary>
    private static void ConnectElectricCircuits(GameObject systems, LastShiftGame game)
    {
        var lighting = systems.AddComponent<WardPowerLighting>();
        game.lighting = lighting;
        var corridor = new List<Light>();
        var emergency = new List<Light>();
        foreach (var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (l.name.StartsWith("Flickering ward light"))
                corridor.Add(l);
            else if (l.name.StartsWith("Emergency light"))
                emergency.Add(l);
        }
        lighting.corridorLights = corridor.ToArray();
        lighting.emergencyLights = emergency.ToArray();

        var positions = new [] {
            new Vector3(0f, 2.65f, -7f),
            new Vector3(5.2f, 2.55f, 1.3f),
            new Vector3(-5.2f, 2.55f, 7.7f)
        };
        lighting.circuitLights = new Light[positions.Length];
        lighting.circuitIndicators = new Renderer[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            var go = new GameObject("POWER CIRCUIT " + (i + 1));
            go.transform.SetParent(world, false);
            go.transform.position = positions[i];
            var l = go.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(.76f,.88f,1f);
            l.intensity = 0f;
            l.range = 8.5f;
            l.shadows = LightShadows.None;
            lighting.circuitLights[i] = l;

            // Signage located on the corridor ceiling; emissive state changes are immediate.
            var indicator = Box("Circuit " + (i + 1) + " POWER STATUS",
                new Vector3((i - 1) * .42f, 2.70f, -6.60f),
                new Vector3(.25f, .04f, .25f), unpowered, world, false);
            lighting.circuitIndicators[i] = indicator.GetComponent<Renderer>();
        }
        Debug.Log("[LastShift Lighting] Three circuits connected to fuse installation events.");
    }

    private static Material MakeMaterial(string name, Color color, float roughness,
        Color? emission = null)
    {
        const string dir = "Assets/LastShift/GeneratedMaterials";
        if (!AssetDatabase.IsValidFolder(dir))
            AssetDatabase.CreateFolder(Base, "GeneratedMaterials");
        var path = dir + "/" + name + ".mat";
        var saved = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (saved != null) { PatientSurfaceGenerator.Apply(saved, name, color); EditorUtility.SetDirty(saved); return saved; }
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        var m = new Material(shader) { name = name };
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Color")) m.SetColor("_Color", color);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 1f - roughness);
        if (emission.HasValue && m.HasProperty("_EmissionColor"))
        {
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", emission.Value);
        }
        AssetDatabase.CreateAsset(m, path);
        PatientSurfaceGenerator.Apply(m, name, color);
        EditorUtility.SetDirty(m);
        return m;
    }
}
#endif
