using UnityEngine;

namespace LastShift
{
    /// <summary>Diegetic visual feedback for inserting three fuses and unlocking the lever.</summary>
    public sealed class FusePanelVisual : MonoBehaviour
    {
        public Renderer[] slots;
        public Transform breakerLever;
        public Material emptyMaterial;
        public Material insertedMaterial;
        public Material powerMaterial;
        private int inserted;
        private bool powerOn;
        private Quaternion offRotation;

        private void Start()
        {
            if (breakerLever != null) offRotation = breakerLever.localRotation;
            Refresh();
        }

        public void UpdateState(int count, bool powered)
        {
            inserted = Mathf.Clamp(count, 0, 3);
            powerOn = powered;
            Refresh();
        }

        private void Refresh()
        {
            if (slots != null)
                for (int i = 0; i < slots.Length; i++)
                    if (slots[i] != null)
                        slots[i].sharedMaterial = powerOn ? powerMaterial :
                            i < inserted ? insertedMaterial : emptyMaterial;

            if (breakerLever != null)
                breakerLever.localRotation = offRotation *
                    Quaternion.Euler(0f, 0f, powerOn ? -48f : 0f);
        }
    }
}