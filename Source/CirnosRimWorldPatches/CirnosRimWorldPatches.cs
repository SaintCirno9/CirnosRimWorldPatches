using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace CirnosRimWorldPatches
{
    public class CirnosRimWorldPatches : Mod
    {
        public static CirnosRimWorldPatchesSettings Settings;

        public CirnosRimWorldPatches(ModContentPack content) : base(content)
        {
            Settings = GetSettings<CirnosRimWorldPatchesSettings>();
        }

        public override string SettingsCategory() => "Cirno's RimWorld Patches";

        public override void DoSettingsWindowContents(Rect canvas)
        {
            Settings.DoWindowContents(canvas);
        }
    }

    [StaticConstructorOnStartup]
    public static class Patcher
    {
        static Patcher()
        {
            var harmony = new Harmony("net.cirno.rimworld.mod.cirnospatches");
            new EmpirePatch().ApplyPatches(harmony);
            new HskPatch().ApplyPatches(harmony);
            // new VanillaPatch().ApplyPatches(harmony);
            new OutfitForcedHandlerPatch().ApplyPatches(harmony);
            new PawnColumnWorker_Outfit_Patch().ApplyPatches(harmony);
            Log.Message("Cirno's RimWorld Patches loaded successfully!");
        }
    }
}