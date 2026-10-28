using UnityEngine;

namespace LastShift
{
    /// <summary>Temporary desktop-only HUD. No headset UI overlay: VR uses a physical mission board.</summary>
    public sealed class LastShiftHUD : MonoBehaviour
    {
        public LastShiftGame game;
        public FlashlightBehaviour flashlight;
        public PCVRHandInteractor interactor;
        private GUIStyle title, body, muted, alert;
        private void EnsureStyles()
        {
            if (title != null) return;
            title = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
            title.normal.textColor = new Color(.92f, .25f, .16f);
            body = new GUIStyle(GUI.skin.label) { fontSize = 16, wordWrap = true };
            body.normal.textColor = new Color(.91f, .95f, .93f);
            muted = new GUIStyle(GUI.skin.label) { fontSize = 13, wordWrap = true };
            muted.normal.textColor = new Color(.59f, .71f, .73f);
            alert = new GUIStyle(body) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 22 };
            alert.normal.textColor = new Color(1f, .38f, .26f);
        }

        private void OnGUI()
        {
            if (!DesktopPreview.IsPreviewActive || game == null) return;
            EnsureStyles();
            float scale = Mathf.Max(.75f, Mathf.Min(Screen.width / 1500f, Screen.height / 850f));
            var previousMatrix = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));
            float w = Screen.width / scale, h = Screen.height / scale;

            GUI.color = new Color(.025f, .04f, .055f, .86f);
            GUI.DrawTexture(new Rect(22, 22, 410, 190), Texture2D.whiteTexture);
            GUI.color = Color.white;
            GUI.Label(new Rect(39, 34, 380, 38), "LAST SHIFT  /  WARD 13", title);
            GUI.Label(new Rect(39, 76, 380, 23), "FUSES  " + game.CollectedFuses + "/3   |   SLOTS  " + game.InstalledFuses + "/3    |    POWER  " +
                      (game.PowerRestored ? "ONLINE" : "OFFLINE"), body);
            GUI.Label(new Rect(39, 110, 380, 75), game.Objective, body);
            GUI.Label(new Rect(38, h - 84, 600, 30), "WASD MOVE  |  MOUSE LOOK  |  E INTERACT  |  F FLASHLIGHT  |  ESC CURSOR", muted);

            float cx = w * .5f, cy = h * .5f;
            GUI.color = Color.white;
            GUI.DrawTexture(new Rect(cx - 1, cy - 7, 2, 14), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(cx - 7, cy - 1, 14, 2), Texture2D.whiteTexture);
            if (interactor != null && !string.IsNullOrEmpty(interactor.CurrentTarget))
            {
                GUI.Label(new Rect(cx - 220, cy + 20, 440, 32), "E / CLICK  -  " + interactor.CurrentTarget, alert);
            }

            if (flashlight != null)
            {
                GUI.Label(new Rect(w - 228, 25, 205, 32),
                    "TORCH  " + (flashlight.IsOn ? "ON" : "OFF") + "  " + Mathf.CeilToInt(flashlight.battery) + "%", body);
            }

            if (game.HasEnded)
            {
                GUI.color = new Color(.01f, .01f, .02f, .85f);
                GUI.DrawTexture(new Rect(w * .5f - 250, h * .5f - 90, 500, 180), Texture2D.whiteTexture);
                GUI.color = Color.white;
                GUI.Label(new Rect(w * .5f - 230, h * .5f - 40, 460, 100),
                    game.Phase == GamePhase.Won ? "YOU ESCAPED!" : "THE WATCHER FOUND YOU", alert);
            }

            GUI.matrix = previousMatrix;
            GUI.color = Color.white;
        }
    }
}