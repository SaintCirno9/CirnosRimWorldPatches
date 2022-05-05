using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using Verse;

namespace CirnosRimWorldPatches
{
    public abstract class Patch
    {
        public abstract void ApplyPatches(Harmony harmony);
    }

    public class EmpirePatch : Patch
    {
        public override void ApplyPatches(Harmony harmony)
        {
            //*******************对Empire模组的建筑建造时间进行修改*******************

            try
            {
                int count = 0;
                var method = AccessTools.Method("FactionColonies.FCBuildingWindow+<>c__DisplayClass24_0:<DoWindowContents>b__1");
                if (method is not null)
                {
                    harmony.Patch(method,
                        transpiler: new HarmonyMethod(
                            typeof(EmpirePatch).GetMethod(
                                nameof(EmpireFCBuildingWindowDoWindowContentsTranspiler))));
                    Log.Message("Cirno's RimWorld Patches: Empire mod patched successfully!");
                    count++;
                }

                //*******************对Empire模组的据点升级时间进行修改*******************
                method = AccessTools.Method("FactionColonies.SettlementUpgradeWindowFc:UpgradeSettlement");
                if (method is not null)
                {
                    harmony.Patch(method,
                        transpiler: new HarmonyMethod(
                            typeof(EmpirePatch).GetMethod(
                                nameof(EmpireSettlementUpgradeWindowFcUpgradeSettlementTranspiler))));
                    count++;

                }

                if (count == 2)
                {
                    Log.Message("Cirno's RimWorld Patches: Empire mod patched successfully!");
                }
            }
            catch (Exception e)
            {
                Log.Error("Cirno's RimWorld Patches: Failed to patch Empire mod.\n" + e);
            }
        }

        public static IEnumerable<CodeInstruction> EmpireFCBuildingWindowDoWindowContentsTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            // FileLog.Reset();
            for (int i = 0; i < codes.Count; i++)
            {
                // FileLog.Log(codes[i].ToString());
                if (codes[i].opcode == OpCodes.Stloc_1)
                {
                    codes.InsertRange(i, new[]
                    {
                        new CodeInstruction(OpCodes.Call,
                            typeof(EmpirePatch).GetMethod(nameof(ModifyConstructionDuration)))
                    });
                    break;
                }
            }

            return codes;
        }

        public static IEnumerable<CodeInstruction> EmpireSettlementUpgradeWindowFcUpgradeSettlementTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            // FileLog.Reset();
            for (int i = 0; i < codes.Count; i++)
            {
                // FileLog.Log(codes[i].ToString());
                if (codes[i].ToString() == "stfld System.Int32 FactionColonies.FCEvent::timeTillTrigger")
                {
                    codes.InsertRange(i - 1, new[]
                    {
                        new CodeInstruction(OpCodes.Call,
                            typeof(EmpirePatch).GetMethod(nameof(ModifyConstructionDuration)))
                    });
                    break;
                }
            }

            return codes;
        }

        public static int ModifyConstructionDuration(int baseDuration)
        {
            // Log.Message(
            //     $"New Construction Duration Will Be {(int)(baseDuration * Settings.EmpireBuildingConstructionDurationModifier)}");
            return (int)(baseDuration * CirnosRimWorldPatches.Settings.EmpireBuildingConstructionDurationModifier);
        }
    }

    public class HskPatch : Patch
    {
        public override void ApplyPatches(Harmony harmony)
        {
            //*******************Patch Hardcore-SK, Method:SK.TraitUtility:PassionUpdate(Pawn pawn)*******************
            try
            {
                var method = AccessTools.Method("SK.TraitUtility:PassionUpdate");
                if (method is not null)
                {
                    harmony.Patch(method,
                        transpiler: new HarmonyMethod(
                            typeof(HskPatch).GetMethod(nameof(HSKTraitUtilityPassionUpdateTranspiler))));
                    Log.Message("Cirno's RimWorld Patches: Hsk mod patched successfully!");

                }
            }
            catch (Exception e)
            {
                Log.Error("Cirno's RimWorld Patches: Failed to patch Hsk mod.\n" + e);
            }
        }

        public static IEnumerable<CodeInstruction> HSKTraitUtilityPassionUpdateTranspiler(
            IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 4; i < codes.Count; i++)
            {
                // FileLog.Log(codes[i].ToString());
                if (codes[i].ToString() == "stfld RimWorld.Passion RimWorld.SkillRecord::passion")
                {
                    codes.InsertRange(i, new[]
                    {
                        new CodeInstruction(OpCodes.Ldloc_S, 25),
                        new CodeInstruction(OpCodes.Call,
                            typeof(HskPatch).GetMethod(nameof(GetMaxPassion)))
                    });
                    break;
                }
            }

            return codes;
        }

        public static Passion GetMaxPassion(Passion passion1, SkillRecord skill)
        {
            return Math.Max((int)passion1, (int)skill.passion) switch
            {
                0 => Passion.None,
                1 => Passion.Minor,
                2 => Passion.Major,
                _ => Passion.None
            };
        }
    }

    public class VanillaPatch : Patch
    {
        public override void ApplyPatches(Harmony harmony)
        {
            //*******************Patch Vanilla, Method: Verse.Pawn:Tick()*******************
            try
            {
                var method = AccessTools.Method("Verse.Pawn:Tick");
                if (method is not null)
                {
                    harmony.Patch(method,
                        transpiler: new HarmonyMethod(
                            typeof(VanillaPatch).GetMethod(nameof(Tick_Transpiler))));
                    Log.Message("Cirno's RimWorld Patches: Vanilla mod patched successfully!");
                }
            }
            catch (Exception e)
            {
                Log.Error("Cirno's RimWorld Patches: Failed to patch Vanilla mod.\n" + e);
            }
        }

        public static IEnumerable<CodeInstruction> Tick_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            for (int i = 0; i < codes.Count; i++)
            {
                // FileLog.Log(codes[i].ToString());
                if (codes[i].ToString() == "callvirt System.Void Verse.Pawn_HealthTracker::HealthTick()")
                {
                    Log.Message("Cirno's RimWorld Patches: Vanilla mod Tick_Transpiler patched successfully!");
                    codes.InsertRange(i + 1, new[]
                    {
                        new CodeInstruction(OpCodes.Callvirt, typeof(VanillaPatch).GetMethod(nameof(HealthTickAsync)))
                    });
                    codes.RemoveAt(i - 1);
                    codes.RemoveAt(i - 1);
                    break;
                }
            }

            return codes;
        }

        public static void HealthTickAsync(Pawn pawn)
        {
            var healthTick = pawn.health.HealthTick;
            if (healthTick is not null)
            {
                lock (pawn.health)
                {
                    healthTick.BeginInvoke(null, null);
                }
            }
        }
    }
}