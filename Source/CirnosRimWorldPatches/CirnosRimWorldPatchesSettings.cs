using UnityEngine;
using Verse;

namespace CirnosRimWorldPatches
{
    public class CirnosRimWorldPatchesSettings : ModSettings
    {
        public float EmpireBuildingConstructionDurationModifier = 1;

        public void DoWindowContents(Rect canvas)
        {
            var listingStandard = new Listing_Standard
            {
                ColumnWidth = 300f
            };
            listingStandard.Begin(canvas);
            listingStandard.Label("CirnoPatchesOptionHeader".Translate());

            listingStandard.Label(
                "EmpireBuildingConstructionDurationModifier".Translate() +
                $":{EmpireBuildingConstructionDurationModifier:P0}",
                tooltip: "EmpireBuildingConstructionDurationModifierLabel".Translate());
            EmpireBuildingConstructionDurationModifier =
                listingStandard.Slider((int)(EmpireBuildingConstructionDurationModifier * 100), 1f, 100f) / 100;


            listingStandard.End();
        }

        public override void ExposeData()
        {
            Scribe_Values.Look(ref EmpireBuildingConstructionDurationModifier,
                "EmpireBuildingConstructionDurationModifier", 1);
            base.ExposeData();
        }
    }
}