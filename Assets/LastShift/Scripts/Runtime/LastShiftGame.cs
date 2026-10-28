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

        public GamePhase Phase { get; private set; } = GamePhase.Searching;
        public int CollectedFuses { get; private set; }
        public const int RequiredFuses = 3;
        public bool PowerRestored { get; private set; }
        public bool HasEnded => Phase == GamePhase.Won || Phase == GamePhase.Lost;
        public string Objective { get; private set; } = "Find three fuses. Aim at an orange fuse and press E / Trigger.";
        public string LastEvent { get; private set; } = "Your last shift begins...";

        private void Start() { Display(Objective); }

        public void FuseFound(LastShiftFuse item)
        {
            if (HasEnded || item == null || item.WasTaken) return;
            item.MarkTaken();
            CollectedFuses++;
            if (audioSystem != null) audioSystem.PlayPickup();

            if (CollectedFuses == 2 && monster != null)
            {
                Phase = GamePhase.Hunted;
                monster.Wake(this, flashlight);
                if (audioSystem != null) audioSystem.PlayDanger();
                Display("Something woke up. The flashlight can slow it. Find the last fuse!");
                return;
            }

            Display(CollectedFuses == RequiredFuses
                ? "All fuses found! Return to the POWER BOX near the entrance."
                : "Fuse secured. Search deeper for the next one.");
        }

        public void TryRestorePower()
        {
            if (HasEnded) return;
            if (CollectedFuses < RequiredFuses)
            {
                if (audioSystem != null) audioSystem.PlayDenied();
                Display("Power box needs " + (RequiredFuses - CollectedFuses) + " more fuses.");
                return;
            }
            PowerRestored = true;
            Phase = GamePhase.Powered;
            if (exitDoor != null) exitDoor.SetUnlocked(true);
            if (audioSystem != null) audioSystem.PlayPickup();
            Display("POWER RESTORED. Reach the EXIT DOOR behind the entrance.");
        }

        public void TryEscape()
        {
            if (HasEnded) return;
            if (!PowerRestored)
            {
                if (audioSystem != null) audioSystem.PlayDenied();
                Display("Exit locked. Restore power at the fuse box first.");
                return;
            }
            Phase = GamePhase.Won;
            if (monster != null) monster.StopChasing();
            if (audioSystem != null) audioSystem.PlayVictory();
            Display("YOU ESCAPED WARD 13. You win!");
        }

        public void Caught()
        {
            if (HasEnded) return;
            Phase = GamePhase.Lost;
            if (monster != null) monster.StopChasing();
            if (audioSystem != null) audioSystem.PlayDanger();
            Display("THE WATCHER CAUGHT YOU. Game over.");
        }

        private void Display(string message)
        {
            Objective = message;
            LastEvent = message;
            if (objectiveBoard != null)
                objectiveBoard.text = "LAST SHIFT  |  WARD 13\n\nFUSES: " + CollectedFuses + "/3"
                    + "\nPOWER: " + (PowerRestored ? "ONLINE" : "OFFLINE")
                    + "\n\n" + message;
            Debug.Log("[LastShift] " + message);
        }
    }
}
