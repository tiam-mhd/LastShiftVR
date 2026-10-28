#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using LastShift;

/// <summary>
/// Builds on the original functional Ward 13 corridor and enriches it with
/// procedural props, signs, treatment stations, lighting and a recognizable enemy.
/// No downloaded assets required. All geometry is Unity primitives.
/// </summary>
public static class Ward13AtmosphereBuilder
{
    private const string MaterialFolder = "Assets/LastShift/GeneratedMaterials";

    // Internal helper; no legacy/test menu entry.
    public static void Build()
    {
        // Existing generator creates and saves the wired base scene.
        LastShiftSceneBuilder.Build();
        var rootGo = GameObject.Find("WARD 13");
        var systems = GameObject.Find("GAME SYSTEMS");
        if (rootGo == null || systems == null)
        {
            Debug.LogError("[LastShift] Could not find generated prototype objects.");
            return;
        }

        var root = rootGo.transform;
        var game = systems.GetComponent<LastShiftGame>();
        var input = systems.GetComponent<PCVRInput>();
        var interact = systems.GetComponent<PCVRHandInteractor>();
        var torch = systems.GetComponent<FlashlightBehaviour>();
        if (game == null || input == null || interact == null || torch == null) return;

        // Readable but still dark enough for a flashlight-driven horror scene.
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(.075f, .095f, .115f);
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogDensity = .027f;
        RenderSettings.fogColor = new Color(.025f, .055f, .067f);

        var camera = Camera.main;
        if (camera != null)
        {
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(.015f, .025f, .032f);
            camera.farClipPlane = 90f;
        }

        var midnight = Mat("Ward_BlackMetal", new Color(.038f, .06f, .078f));
        var stone = Mat("Ward_WallConcrete", new Color(.22f, .27f, .28f));
        var ceiling = Mat("Ward_LightGrey", new Color(.37f, .43f, .42f));
        var tileA = Mat("Ward_TileDark", new Color(.16f, .20f, .22f));
        var tileB = Mat("Ward_TileLight", new Color(.20f, .24f, .24f));
        var pale = Mat("Ward_PalePlastic", new Color(.58f, .62f, .58f));
        var rust = Mat("Ward_Rust", new Color(.28f, .13f, .10f));
        var teal = Mat("Ward_TealGlow", new Color(.08f, .42f, .44f), new Color(.01f, .36f, .42f));
        var amber = Mat("Ward_AmberGlow", new Color(.95f, .40f, .055f), new Color(.65f, .15f, .012f));
        var red = Mat("Ward_EmergencyRed", new Color(.68f, .045f, .04f), new Color(.36f, .01f, .01f));
        var eyes = Mat("Ward_WatcherEyes", new Color(.97f, .095f, .06f), new Color(.8f, .05f, .025f));
        var pitch = Mat("Ward_WatcherSkin", new Color(.035f, .036f, .052f));
        var paper = Mat("Ward_Paper", new Color(.57f, .52f, .41f));

        // Replace the original 3 primitive stretchers with recognizable equipment.
        var oldStretcher = root.Find("Stretcher");
        while (oldStretcher != null)
        {
            UnityEngine.Object.DestroyImmediate(oldStretcher.gameObject);
            oldStretcher = root.Find("Stretcher");
        }

        // Floor checkerboard makes movement perceptible without textures.
        for (int z = 0; z < 27; z++)
            for (int x = 0; x < 4; x++)
            {
                Cube("Worn ceramic floor tile", new Vector3(-1.87f + x * 1.25f, .012f, -10.95f + z),
                    new Vector3(1.22f, .018f, .97f), (x + z) % 2 == 0 ? tileA : tileB, root, false);
            }

        // Two-tone walls, rails, and repeated structural ribs.
        for (int side = -1; side <= 1; side += 2)
        {
            float wallX = side * 2.43f;
            Cube("Hospital wall wainscot", new Vector3(wallX, .6f, 2f),
                new Vector3(.045f, 1.18f, 27.4f), stone, root, false);
            Cube("Handrail", new Vector3(side * 2.32f, 1.05f, 2f),
                new Vector3(.10f, .12f, 27.2f), pale, root, false);
            Cube("Hospital baseboard", new Vector3(side * 2.3f, .10f, 2f),
                new Vector3(.10f, .20f, 27.3f), midnight, root, false);

            for (int k = 0; k < 10; k++)
            {
                float z = -10f + k * 2.7f;
                Cube("Concrete vertical rib", new Vector3(side * 2.39f, 1.65f, z),
                    new Vector3(.15f, 2.93f, .13f), ceiling, root, false);
                if (k % 2 == 0 && k > 0)
                    SideDoor(side, z + 1.05f, root, midnight, rust, pale, amber);
            }
        }

        for (int i = 0; i < 11; i++)
        {
            float z = -10.5f + i * 2.45f;
            Cube("Ceiling service rib", new Vector3(0, 2.94f, z),
                new Vector3(4.6f, .12f, .15f), midnight, root, false);
            Cube("Ceiling lamp shroud", new Vector3(0, 2.83f, z + .5f),
                new Vector3(.68f, .12f, 1.05f), midnight, root, false);
            Cube("Flickering fluorescent tube", new Vector3(0, 2.765f, z + .5f),
                new Vector3(.35f, .02f, .79f), i % 4 == 0 ? red : teal, root, false);
            var l = new GameObject("Flickering ward light").AddComponent<Light>();
            l.transform.SetParent(root);
            l.transform.position = new Vector3(0, 2.6f, z + .5f);
            l.type = LightType.Point;
            l.range = 6.5f;
            l.intensity = i % 4 == 0 ? .38f : .50f;
            l.color = i % 4 == 0 ? new Color(.98f, .09f, .075f) : new Color(.38f, .76f, .76f);
            l.shadows = LightShadows.None;
            if (i % 3 == 0)
            {
                var flicker = l.gameObject.AddComponent<HospitalFlicker>();
                flicker.target = l;
                flicker.baseIntensity = l.intensity;
            }
        }

        // Pipes and overhead wiring.
        for (int i = 0; i < 3; i++)
            Cube("Ceiling conduit", new Vector3(-1.65f + i * .30f, 2.97f, 2),
                new Vector3(.055f, .075f, 27.2f), i == 0 ? rust : midnight, root, false);
        for (int i = 0; i < 13; i++)
            Cube("Pipe bracket", new Vector3(-1.35f, 2.91f, -10 + i * 2f),
                new Vector3(.9f, .11f, .07f), ceiling, root, false);

        // Gurneys, cabinets, oxygen tank stands and debris in the corridor.
        Bed("Abandoned gurney A", new Vector3(-1.54f, 0, -4.1f), root, pale, midnight, rust);
        Bed("Abandoned gurney B", new Vector3(1.52f, 0, 3.2f), root, pale, midnight, rust);
        Bed("Abandoned gurney C", new Vector3(-1.48f, 0, 9.2f), root, pale, midnight, rust);
        Cabinet(new Vector3(1.98f, 0, -2.5f), root, midnight, rust, pale);
        Cabinet(new Vector3(-1.98f, 0, 6.6f), root, midnight, rust, pale);
        Tank(new Vector3(2.02f, 0, 8.15f), root, pale, rust, midnight);
        Tank(new Vector3(-2.05f, 0, .65f), root, pale, rust, midnight);

        // Fuses now rest on lit wall-side stations rather than float in an empty corridor.
        foreach (var fuse in root.GetComponentsInChildren<LastShiftFuse>(true))
        {
            fuse.gameObject.AddComponent<FuseBeacon>();
            var p = fuse.transform.position;
            Cube("Fuse pedestal", new Vector3(p.x, .61f, p.z),
                new Vector3(.62f, .88f, .58f), midnight, root, true);
            Cube("Fuse pedestal indicator", new Vector3(p.x, 1.075f, p.z),
                new Vector3(.58f, .055f, .55f), amber, root, false);
            var marker = new GameObject("Fuse marker light");
            marker.transform.SetParent(root);
            marker.transform.position = p + Vector3.up * .35f;
            var l = marker.AddComponent<Light>();
            l.type = LightType.Point;
            l.color = new Color(1f, .38f, .11f);
            l.range = 2f;
            l.intensity = .55f;
            l.shadows = LightShadows.None;
        }

        var console = root.GetComponentInChildren<LastShiftConsole>();
        if (console != null)
        {
            console.transform.position = new Vector3(2.18f, 1.28f, -7f);
            Cube("Power box mount", new Vector3(2.33f, 1.28f, -7f),
                new Vector3(.22f, .91f, .89f), midnight, root, false);
            Cube("Power box hazard lamp", new Vector3(2.15f, 1.75f, -7f),
                new Vector3(.10f, .09f, .24f), amber, root, false);
        }

        // Visible signage facing the spawn point (-Z).
        Text("WARD 13  |  RESTRICTED AREA", new Vector3(0, 2.15f, -5.5f),
            .15f, teal.color, root);
        Text("3 FUSES  >  POWER BOX  >  EXIT", new Vector3(0, 1.91f, -5.5f),
            .105f, pale.color, root);
        Text("KEEP THE LIGHT ON", new Vector3(0, 2.10f, 13.2f),
            .12f, red.color, root);
        Text("EXIT", new Vector3(0, 2.54f, -11.35f), .22f, amber.color, root);
        // Old live-updating mission board remains available, but move it onto the right wall.
        if (game.objectiveBoard != null)
        {
            game.objectiveBoard.transform.position = new Vector3(2.35f, 2.33f, -8.1f);
            game.objectiveBoard.transform.rotation = Quaternion.Euler(0, 90, 0);
            game.objectiveBoard.anchor = TextAnchor.UpperLeft;
            game.objectiveBoard.characterSize = .042f;
        }

        // Add dark visual mass, shoulders, limbs, skull and glowing eyes.
        if (game.monster != null)
        {
            var m = game.monster.transform;
            for (int i = m.childCount - 1; i >= 0; i--)
                UnityEngine.Object.DestroyImmediate(m.GetChild(i).gameObject);
            Capsule("Watcher torso", m, new Vector3(0, 1.30f, 0),
                new Vector3(.60f, 1f, .43f), pitch, true);
            Sphere("Watcher head", m, new Vector3(0, 2.33f, .10f),
                new Vector3(.48f, .56f, .50f), pitch, false);
            for (int side = -1; side <= 1; side += 2)
            {
                Capsule("Unnaturally long arm", m, new Vector3(side * .52f, 1.22f, .03f),
                    new Vector3(.22f, 1.33f, .22f), pitch, false);
                Capsule("Watcher leg", m, new Vector3(side * .18f, .52f, 0),
                    new Vector3(.25f, .53f, .31f), pitch, false);
                Sphere("Watcher eye", m, new Vector3(side * .15f, 2.39f, .33f),
                    new Vector3(.115f, .09f, .055f), eyes, false);
            }
            var eyeLight = new GameObject("Watcher eye glow").AddComponent<Light>();
            eyeLight.transform.SetParent(m, false);
            eyeLight.transform.localPosition = new Vector3(0, 2.35f, .43f);
            eyeLight.type = LightType.Point;
            eyeLight.color = Color.red;
            eyeLight.range = 1.8f;
            eyeLight.intensity = .55f;
            eyeLight.shadows = LightShadows.None;
            game.monster.chaseSpeed = 1.0f;
        }

        var audio = systems.AddComponent<LastShiftAudio>();
        game.audioSystem = audio;
        systems.AddComponent<LastShiftAmbience>();
        var hud = systems.AddComponent<LastShiftHUD>();
        hud.game = game;
        hud.flashlight = torch;
        hud.interactor = interact;

        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        AssetDatabase.SaveAssets();
        Debug.Log("[LastShift] Atmospheric Ward 13 v0.2 generated. Press Play. Desktop: WASD, mouse, E, F.");
    }

    private static void SideDoor(int side, float z, Transform root,
        Material frame, Material door, Material handle, Material lamp)
    {
        float x = side * 2.36f;
        Cube("Ward door panel - decorative", new Vector3(x, 1.23f, z),
            new Vector3(.065f, 2.25f, 1.16f), door, root, false);
        Cube("Door frame upper", new Vector3(x - side * .06f, 2.43f, z),
            new Vector3(.16f, .16f, 1.32f), frame, root, false);
        for (int end = -1; end <= 1; end += 2)
            Cube("Door frame side", new Vector3(x - side * .06f, 1.22f, z + end * .62f),
                new Vector3(.16f, 2.40f, .11f), frame, root, false);
        Cube("Door handle", new Vector3(x - side * .12f, 1.02f, z + .37f),
            new Vector3(.16f, .06f, .22f), handle, root, false);
        Cube("Room indicator", new Vector3(x - side * .12f, 2.05f, z),
            new Vector3(.08f, .08f, .38f), lamp, root, false);
    }

    private static void Bed(string name, Vector3 p, Transform root,
        Material mattress, Material metal, Material stain)
    {
        var bed = new GameObject(name).transform;
        bed.SetParent(root);
        bed.position = p;
        Cube("Steel frame", p + new Vector3(0, .65f, 0),
            new Vector3(.94f, .11f, 1.76f), metal, bed, true);
        Cube("Folded mattress", p + new Vector3(0, .77f, 0),
            new Vector3(.82f, .13f, 1.56f), mattress, bed, false);
        Cube("Old stain", p + new Vector3(-.22f, .85f, .15f),
            new Vector3(.25f, .01f, .30f), stain, bed, false);
        for (int side = -1; side <= 1; side += 2)
        {
            Cube("Bed rail", p + new Vector3(side * .48f, 1.02f, 0),
                new Vector3(.06f, .07f, 1.53f), metal, bed, false);
            for (int end = -1; end <= 1; end += 2)
            {
                Cube("Bed leg", p + new Vector3(side * .41f, .36f, end * .70f),
                    new Vector3(.08f, .65f, .08f), metal, bed, false);
                Sphere("Castor", bed, new Vector3(side * .41f, .12f, end * .70f),
                    new Vector3(.16f, .16f, .16f), metal, false);
            }
        }
    }

    private static void Cabinet(Vector3 p, Transform root,
        Material body, Material rust, Material handle)
    {
        Cube("Medicine cabinet", p + new Vector3(0, 1.05f, 0),
            new Vector3(.48f, 1.92f, .91f), body, root, true);
        for (int k = 0; k < 3; k++)
        {
            Cube("Drawer front", p + new Vector3(-.26f, .53f + k * .44f, 0),
                new Vector3(.025f, .37f, .78f), rust, root, false);
            Cube("Drawer handle", p + new Vector3(-.29f, .53f + k * .44f, 0),
                new Vector3(.045f, .045f, .20f), handle, root, false);
        }
    }

    private static void Tank(Vector3 p, Transform root,
        Material body, Material cap, Material dark)
    {
        var mount = new GameObject("Oxygen tank").transform;
        mount.SetParent(root);
        mount.position = p;
        Capsule("Cylinder", mount, new Vector3(0, .74f, 0),
            new Vector3(.30f, .62f, .30f), body, true);
        Cube("Valve", p + new Vector3(0, 1.48f, 0),
            new Vector3(.20f, .12f, .20f), cap, mount, false);
        Cube("Support", p + new Vector3(0, .07f, 0),
            new Vector3(.45f, .14f, .45f), dark, mount, true);
    }

    private static GameObject Cube(string name, Vector3 pos, Vector3 scale,
        Material material, Transform parent, bool collider)
    {
        var item = GameObject.CreatePrimitive(PrimitiveType.Cube);
        item.name = name;
        item.transform.SetParent(parent);
        item.transform.position = pos;
        item.transform.localScale = scale;
        item.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider) UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
        return item;
    }

    private static GameObject Sphere(string name, Transform parent, Vector3 local,
        Vector3 scale, Material material, bool collider)
    {
        var item = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        item.name = name;
        item.transform.SetParent(parent, false);
        item.transform.localPosition = local;
        item.transform.localScale = scale;
        item.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider) UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
        return item;
    }

    private static GameObject Capsule(string name, Transform parent, Vector3 local,
        Vector3 scale, Material material, bool collider)
    {
        var item = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        item.name = name;
        item.transform.SetParent(parent, false);
        item.transform.localPosition = local;
        item.transform.localScale = scale;
        item.GetComponent<Renderer>().sharedMaterial = material;
        if (!collider) UnityEngine.Object.DestroyImmediate(item.GetComponent<Collider>());
        return item;
    }

    private static void Text(string value, Vector3 pos, float characterSize,
        Color color, Transform parent)
    {
        var sign = new GameObject("Sign: " + value);
        sign.transform.SetParent(parent);
        sign.transform.position = pos;
        // TextMesh front side must face the entrance; otherwise labels are mirrored.
        sign.transform.rotation = Quaternion.Euler(0f, value == "EXIT" ? 0f : 180f, 0f);
        var text = sign.AddComponent<TextMesh>();
        text.text = value;
        text.fontSize = 56;
        text.characterSize = characterSize;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.color = color;
    }

    private static Material Mat(string name, Color color, Color? emission = null)
    {
        if (!AssetDatabase.IsValidFolder(MaterialFolder))
            AssetDatabase.CreateFolder("Assets/LastShift", "GeneratedMaterials");
        var asset = MaterialFolder + "/" + name + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(asset);
        if (existing != null)
        {
            Ward13UrpMaterialTools.Repair(existing, color, emission);
            return existing;
        }

        var result = new Material(Ward13UrpMaterialTools.LitShader()) { name = name };
        if (result.HasProperty("_BaseColor")) result.SetColor("_BaseColor", color);
        if (result.HasProperty("_Color")) result.SetColor("_Color", color);
        if (emission.HasValue && result.HasProperty("_EmissionColor"))
        {
            result.EnableKeyword("_EMISSION");
            result.SetColor("_EmissionColor", emission.Value);
        }
        AssetDatabase.CreateAsset(result, asset);
        return result;
    }
}
#endif
