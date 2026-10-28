using UnityEngine;

namespace LastShift
{
    public enum GamePhase { Searching, Hunted, Powered, Won, Lost }

    public sealed class LastShiftGame : MonoBehaviour
    {
        public TextMesh objectiveBoard;
        public LastShiftMonster monster;
        public LastShiftDoor exitDoor;
        public FlashlightBehaviour flashlight;
        public LastShiftAudio audioSystem;
        public GhostDirector ghostDirector;
        public FusePanelVisual fusePanel;
        public WardPowerLighting lighting;
        public WardHorrorEvents horrorEvents;

        public GamePhase Phase { get; private set; } = GamePhase.Searching;
        public int CollectedFuses { get; private set; }
        public int InstalledFuses { get; private set; }
        public const int RequiredFuses = 3;
        public bool PowerRestored { get; private set; }
        public bool HasEnded => Phase == GamePhase.Won || Phase == GamePhase.Lost;

        public string Objective { get; private set; } =
            "Find three fuses. Aim at each one and press E / Trigger.";
        public string LastEvent { get; private set; } = "You have one last shift...";

        private void Start() { Display(Objective); }

        public void FuseFound(LastShiftFuse item)
        {
            if (HasEnded || item == null || item.WasTaken) return;
            item.MarkTaken();
            CollectedFuses++;
            if (lighting != null) lighting.OnFuseCollected(CollectedFuses);
            if (horrorEvents != null) horrorEvents.OnFuseTaken(CollectedFuses);
            if (audioSystem != null) audioSystem.PlayPickup();

            if (CollectedFuses == 1)
            {
                Display(ghostDirector != null && ghostDirector.CharacterReady
                    ? "Something moved at the far end of the corridor..."
                    : "A metallic knock echoes through the empty ward...");
                if (ghostDirector != null) ghostDirector.OnFuseCountChanged(1);
            }
            else if (CollectedFuses == 2)
            {
                if (ghostDirector != null && ghostDirector.CharacterReady)
                {
                    Phase = GamePhase.Hunted;
                    ghostDirector.OnFuseCountChanged(2);
                    if (audioSystem != null) audioSystem.PlayDanger();
                    Display("IT IS HUNTING YOU. Shine your torch at it, find the final fuse.");
                }
                else
                {
                    // The stage stays completable while the artist's 3D model is missing.
                    // There can NEVER be an invisible attacking enemy.
                    Display("2/3 fuses found. Real 3D Patient model not yet installed; continue the mission.");
                }
            }
            else
            {
                Display("All three fuses found! Go to the power cabinet near the exit.");
            }
        }

        /// <summary>Each interaction at the panel inserts one carried fuse;
        /// a final interaction flips the main breaker.</summary>
        public void TryRestorePower()
        {
            if (HasEnded || PowerRestored) return;

            if (CollectedFuses == 0)
            {
                Denied("No fuses collected. Find them in the wards.");
                return;
            }
            if (InstalledFuses < RequiredFuses)
            {
                if (InstalledFuses >= CollectedFuses)
                {
                    Denied("Find another fuse before returning to this cabinet.");
                    return;
                }
                InstalledFuses++;
                if (lighting != null) lighting.OnFuseInserted(InstalledFuses);
                if (fusePanel != null) fusePanel.UpdateState(InstalledFuses, false);
                if (audioSystem != null) audioSystem.PlayPickup();

                Display(InstalledFuses < RequiredFuses
                    ? "Fuse " + InstalledFuses + "/3 inserted. E / Trigger to insert more."
                    : "All fuses installed. Press E / Trigger again to flip the BREAKER.");
                return;
            }

            PowerRestored = true;
            Phase = GamePhase.Powered;
            if (lighting != null) lighting.OnBreakerRestored();
            if (horrorEvents != null) horrorEvents.OnPowerRestored();
            if (exitDoor != null) exitDoor.SetUnlocked(true);
            if (fusePanel != null) fusePanel.UpdateState(InstalledFuses, true);
            if (audioSystem != null) audioSystem.PlayVictory();
            Display("POWER RESTORED! Reach the red EXIT door.");
        }

        public void TryEscape()
        {
            if (HasEnded) return;
            if (!PowerRestored)
            {
                Denied("EXIT LOCKED. Insert all three fuses and flip the breaker.");
                return;
            }
            Phase = GamePhase.Won;
            if (monster != null) monster.StopChasing();
            if (ghostDirector != null) ghostDirector.OnGameEnded();
            if (audioSystem != null) audioSystem.PlayVictory();
            Display("YOU ESCAPED WARD 13. YOU WIN!");
        }

        public void Caught()
        {
            if (HasEnded) return;
            Phase = GamePhase.Lost;
            if (monster != null) monster.StopChasing();
            if (ghostDirector != null) ghostDirector.OnGameEnded();
            if (audioSystem != null) audioSystem.PlayDanger();
            Display("THE PATIENT CAUGHT YOU. GAME OVER.");
        }

        private void Denied(string message)
        {
            if (audioSystem != null) audioSystem.PlayDenied();
            Display(message);
        }

        private void Display(string message)
        {
            Objective = message;
            LastEvent = message;
            if (objectiveBoard != null)
                objectiveBoard.text = "LAST SHIFT / WARD 13\n\nFUSES: " +
                    CollectedFuses + "/3\nINSTALLED: " + InstalledFuses + "/3" +
                    "\nPOWER: " + (PowerRestored ? "ONLINE" : "OFFLINE") + "\n\n" + message;
            Debug.Log("[LastShift] " + message);
        }
    }
}
