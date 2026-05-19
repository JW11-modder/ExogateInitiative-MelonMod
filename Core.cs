using HarmonyLib;
using JModder;
using MelonLoader;
using MelonLoader.Preferences;
using MelonLoader.Utils;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using static MelonLoader.MelonLogger;

[assembly: MelonInfo(typeof(JModder.ExoGater.Core), "ExoCheatMod", "1.2.0", "jw11-modder", null)]
[assembly: MelonGame("XenoBits", "ExogateInitiative")]

namespace JModder.ExoGater
{
    public class Core : MelonMod
    {
        public static Core Instance { get; private set; }

        private static MelonPreferences_Category MultiplierFloatCategory;
        private static MelonPreferences_Category MultiplierIntCategory;
        private static MelonPreferences_Category ToggleCategory;
        private static MelonPreferences_Category MinAttrCategory;

        private static MelonPreferences_Entry<KeyCode> configMenuToggle;

        private static MelonPreferences_Entry<bool> configGaterMinAttr;
        private static MelonPreferences_Entry<bool> configNoGaterBodyNeeds;
        private static MelonPreferences_Entry<bool> configNoGaterMentalNeeds;
        private static MelonPreferences_Entry<bool> configNoPowerCost;
        private static MelonPreferences_Entry<bool> configNoObjectsCost;
        private static MelonPreferences_Entry<bool> configNoCraftCost;
        private static MelonPreferences_Entry<bool> configAlwaysPassSkill;
        private static MelonPreferences_Entry<bool> configInstantProcessing;

        private static MelonPreferences_Entry<float> configCraftSpeedMultiplier;
        private static MelonPreferences_Entry<float> configResearchSpeedMultiplier;
        private static MelonPreferences_Entry<float> configBuildSpeedMultiplier;
        private static MelonPreferences_Entry<float> configInfluenceMultiplier;
        private static MelonPreferences_Entry<float> configMoneyMultiplier;
        private static MelonPreferences_Entry<float> configResearchPointsMultiplier;
        private static MelonPreferences_Entry<float> configWeaponDamageMultiplier;
        private static MelonPreferences_Entry<float> configCompoundMultiplier;
        private static MelonPreferences_Entry<float> configGaterXPMultiplier;

        private static MelonPreferences_Entry<int> configAttrBodyMin;
        private static MelonPreferences_Entry<int> configAttrMindMin;
        private static MelonPreferences_Entry<int> configAttrPerceptionMin;
        private static MelonPreferences_Entry<int> configAttrSocialMin;
        private static MelonPreferences_Entry<int> configAttrKnowledgeMin;
        private static MelonPreferences_Entry<int> configAttrLogicMin;

        // configGaterMinAttr

        [HarmonyPatch(typeof(GatersManager), "Update")]
        class GatersManagerUpdatePatch1
        {
            static bool Prefix(GatersManager __instance)
            {
                if (!configGaterMinAttr.Value)
                {
                    return true;
                }
                for (int i = 0; i < __instance.Gaters.Count; i++)
                {
                    if (__instance.Gaters[i] != null)
                    {
                        foreach (var item in __instance.Gaters[i].Data.Attributes)
                        {
                            if (item.Key == ActorGaterData.ATTRIBUTE.BODY && item.Value < configAttrBodyMin.Value)
                            {
                                __instance.Gaters[i].Data.SetAttribute(item.Key, configAttrBodyMin.Value);
                            }
                            if (item.Key == ActorGaterData.ATTRIBUTE.MINDSTRENGTH && item.Value < configAttrMindMin.Value)
                            {
                                __instance.Gaters[i].Data.SetAttribute(item.Key, configAttrMindMin.Value);
                            }
                            if (item.Key == ActorGaterData.ATTRIBUTE.PERCEPTION && item.Value < configAttrPerceptionMin.Value)
                            {
                                __instance.Gaters[i].Data.SetAttribute(item.Key, configAttrPerceptionMin.Value);
                            }
                            if (item.Key == ActorGaterData.ATTRIBUTE.SOCIAL && item.Value < configAttrSocialMin.Value)
                            {
                                __instance.Gaters[i].Data.SetAttribute(item.Key, configAttrSocialMin.Value);
                            }
                            if (item.Key == ActorGaterData.ATTRIBUTE.KNOWLEDGE && item.Value < configAttrKnowledgeMin.Value)
                            {
                                __instance.Gaters[i].Data.SetAttribute(item.Key, configAttrKnowledgeMin.Value);
                            }
                            if (item.Key == ActorGaterData.ATTRIBUTE.LOGIC && item.Value < configAttrLogicMin.Value)
                            {
                                __instance.Gaters[i].Data.SetAttribute(item.Key, configAttrLogicMin.Value);
                            }
                        }
                    }
                }
                return true;
            }
        }

        // configGaterXPMultiplier

        [HarmonyPatch(typeof(ActorGater), "GainXp")]
        class GaterGainXpPatch1
        {
            static bool Prefix(ref int p_amount)
            {
                if (configGaterXPMultiplier.Value <= 1)
                    return true;
                p_amount = Mathf.RoundToInt(p_amount * configGaterXPMultiplier.Value);
                return true;
            }
        }

        // configNoGaterBodyNeeds

        [HarmonyPatch(typeof(ActorGater), "UpdateBodyFatigue")]
        class UpdateBodyFatiguePatch1
        {
            static bool Prefix(ActorGater __instance)
            {
                if (!configNoGaterBodyNeeds.Value)
                    return true;
                __instance.Data.BodyFatigue = 0;
                __instance.Data.BodyNeeds = 0;
                __instance.Data.Health = __instance.Data.MaxHealth;

                return false;
            }
        }

        // ActorGater Hurt
        [HarmonyPatch(typeof(ActorGater), nameof(ActorGater.Hurt))]
        class GaterHurtPatch1
        {
            static bool Prefix(ActorGater __instance, ref float __result)
            {
                if (!configNoGaterBodyNeeds.Value)
                    return true;
                __instance.Data.BodyFatigue = 0;
                __instance.Data.BodyNeeds = 0;
                __instance.Data.Health = __instance.Data.MaxHealth;
                __result = 0;
                return false;
            }
        }


        // configNoGaterMentalNeeds

        [HarmonyPatch(typeof(ActorGater), "UpdateEntertainementNeeds")]
        class UpdateEntertainementNeedsPatch1
        {
            static bool Prefix(ActorGater __instance)
            {
                if (!configNoGaterMentalNeeds.Value)
                    return true;
                __instance.Data.BrainFatigue = 0;
                __instance.Data.EntertainementNeeds = 0;
                __instance.Data.SocialNeeds = 0;
                __instance.Data.MentalHealth = __instance.Data.MaxMentalHealth;

                return false;
            }
        }

        //configCraftSpeedMultiplier

        [HarmonyPatch(typeof(CraftTask), nameof(CraftTask.MakeCraftProgress))]
        class MakeCraftProgressPatch1
        {
            static bool Prefix(ref float p_amountCrafted)
            {
                if (configCraftSpeedMultiplier.Value <= 1)
                    return true;
                p_amountCrafted *= configCraftSpeedMultiplier.Value;
                return true;
            }
        }

        // configResearchPointsMultiplier

        [HarmonyPatch(typeof(ScienceManager), nameof(ScienceManager.EarnAlienSciencePoints))]
        class EarnAlienSciencePointsPatch1
        {
            static bool Prefix(ref float p_amount)
            {
                if (configResearchPointsMultiplier.Value <= 1)
                    return true;
                p_amount *= configResearchPointsMultiplier.Value;
                return true;
            }
        }

        [HarmonyPatch(typeof(ScienceManager), nameof(ScienceManager.EarnSciencePoints))]
        class EarnSciencePointsPointsPatch1
        {
            static bool Prefix(ref float p_amount)
            {
                if (configResearchPointsMultiplier.Value <= 1)
                    return true;
                p_amount *= configResearchPointsMultiplier.Value;
                return true;
            }
        }

        // configResearchSpeedMultiplier

        [HarmonyPatch(typeof(ScienceManager), nameof(ScienceManager.MakeResearchProgress))]
        class MakeResearchProgressPatch1
        {
            static bool Prefix(ref float p_searchedPoints)
            {
                if (configResearchSpeedMultiplier.Value <= 1)
                    return true;
                p_searchedPoints *= configResearchSpeedMultiplier.Value;
                return true;
            }
        }

        // configInfluenceMultiplier

        [HarmonyPatch(typeof(InfluenceManager), nameof(InfluenceManager.EarnInfluence))]
        class EarnInfluencePatch1
        {
            static bool Prefix(ref float p_influenceAmount)
            {
                if (configInfluenceMultiplier.Value <= 1)
                    return true;
                p_influenceAmount *= configInfluenceMultiplier.Value;
                return true;
            }
        }

        // configWeaponDamageMultiplier

        [HarmonyPatch(typeof(ItemWeapon), nameof(ItemWeapon.GetDamage))]
        [HarmonyPatch(typeof(NOR_Turret), "GetProjectileDamage")]
        class GetDamagePatch1
        {
            static void Postfix(ref float __result)
            {
                if (configWeaponDamageMultiplier.Value <= 1)
                    return;
                __result *= configWeaponDamageMultiplier.Value;
            }
        }

        // baseTeamFightGeneratedContact GetHumanGaterDamages
        [HarmonyPatch(typeof(baseTeamFightGeneratedContact), "GetHumanGaterDamages")]
        class GetHumanGaterDamagesPatch1
        {
            static void Postfix(ref int __result)
            {
                if (configWeaponDamageMultiplier.Value <= 1)
                    return;
                __result = Mathf.RoundToInt(__result * configWeaponDamageMultiplier.Value);
            }
        }



        // configNoObjectsCost

        [HarmonyPatch(typeof(PlaceableObject), nameof(PlaceableObject.Install))]
        class PlaceableObjectInstallPatch1
        {
            static bool Prefix(ref PlaceableObject __instance)
            {
                if (!configNoObjectsCost.Value)
                    return true;
                __instance.Data.Price = 0;
                return true;
            }

        }

        // configBuildSpeedMultiplier

        [HarmonyPatch(typeof(BuildersManager), "Start")]
        class BuildersManagerStartPatch1
        {
            static bool Prefix(ref BuildersManager __instance)
            {
                if (configBuildSpeedMultiplier.Value <= 1)
                    return true;
                __instance.DesignData.BuildersCommon.BedrockWallDestructionTime /= configBuildSpeedMultiplier.Value;
                __instance.DesignData.BuildersCommon.BlockCleanTime /= configBuildSpeedMultiplier.Value;
                __instance.DesignData.BuildersCommon.BlockDestructionTime /= configBuildSpeedMultiplier.Value;
                __instance.DesignData.BuildersCommon.ConcreteWallDestructionTime /= configBuildSpeedMultiplier.Value;
                __instance.DesignData.BuildersCommon.RoomObjectConstructionTime /= configBuildSpeedMultiplier.Value;
                __instance.DesignData.BuildersCommon.RoomTileConstructionTime /= configBuildSpeedMultiplier.Value;
                __instance.DesignData.BuildersCommon.RoomTileDestructionTime /= configBuildSpeedMultiplier.Value;
                return true;
            }

        }

        [HarmonyPatch(typeof(BuildersManager), "LoadData")]
        class BuildersManagerLoadPatch1
        {
            static bool Prefix(ref BuildersManager __instance)
            {
                if (configBuildSpeedMultiplier.Value <= 1)
                    return true;
                __instance.DesignData.BuildersCommon.BedrockWallDestructionTime /= configBuildSpeedMultiplier.Value;
                __instance.DesignData.BuildersCommon.BlockCleanTime /= configBuildSpeedMultiplier.Value;
                __instance.DesignData.BuildersCommon.BlockDestructionTime /= configBuildSpeedMultiplier.Value;
                __instance.DesignData.BuildersCommon.ConcreteWallDestructionTime /= configBuildSpeedMultiplier.Value;
                __instance.DesignData.BuildersCommon.RoomObjectConstructionTime /= configBuildSpeedMultiplier.Value;
                __instance.DesignData.BuildersCommon.RoomTileConstructionTime /= configBuildSpeedMultiplier.Value;
                __instance.DesignData.BuildersCommon.RoomTileDestructionTime /= configBuildSpeedMultiplier.Value;
                return true;
            }

        }

        // configMoneyMultiplier

        [HarmonyPatch(typeof(BudgetManager), nameof(BudgetManager.Earn))]
        class BudgetManagerEarnPatch1
        {
            static bool Prefix(ref int p_amount)
            {
                if (configMoneyMultiplier.Value <= 1)
                    return true;
                p_amount = Mathf.RoundToInt(p_amount * configMoneyMultiplier.Value);
                return true;
            }

        }

        // configCompoundMultiplier
        [HarmonyPatch(typeof(RO_CompoundStorage), nameof(RO_CompoundStorage.Capacity), MethodType.Getter)]
        class CompoundStoragePatch1
        {
            static void Postfix(ref float __result)
            {
                if (configCompoundMultiplier.Value <= 1)
                    return;
                __result *= configCompoundMultiplier.Value;
            }

        }


        // configNoPowerCost

        [HarmonyPatch(typeof(PowerManager), nameof(PowerManager.UpdateAvailablePower))]
        class UpdateAvailablePowerPatch1
        {
            private static List<PlaceableObject> l_rooms = new List<PlaceableObject>();
            static bool Prefix(PowerManager __instance)
            {
                if (!configNoPowerCost.Value)
                {
                    return true;
                }
                l_rooms.Clear();
                foreach (Room room in Manager<RoomsManager>.instance.Rooms)
                {
                    foreach (PlaceableObject item in room.RoomObjects)
                    {
                        l_rooms.Add(item);
                    }
                }
                __instance.TotalPower = 9999;
                __instance.UsedPower = 0;
                foreach (PlaceableObject placeableObject4 in l_rooms)
                {
                    if (placeableObject4.IsBuilded && !placeableObject4.IsBroken)
                    {
                        placeableObject4.IsPowered = true;
                    }
                    if (placeableObject4.CurrentState == PlaceableObject.STATE.BUILDING || placeableObject4.CurrentState == PlaceableObject.STATE.PLANNED)
                    {
                        placeableObject4.IsPowered = true;
                    }
                }
                for (int i = Manager<UniverseManager>.instance.ConnectedSectors.Count - 1; i >= 0; i--)
                {
                    SectorCellController sectorByCoordinates = Manager<UniverseManager>.instance.GetSectorByCoordinates(Manager<UniverseManager>.instance.ConnectedSectors[i]);
                    sectorByCoordinates.HasPowerShortage = false;
                }
                EventMessagingSystem.Call("PowerUpdatedEvent");
                return false;
            }
        }

        // configAlwaysPassSkill

        [HarmonyPatch(typeof(SkillCheckManager), "GetChanceToPassSkillCheck")]
        class SkillCheckPatch1
        {
            static void Postfix(ref float __result)
            {
                if (!configAlwaysPassSkill.Value)
                    return;
                __result = 1f;
            }
        }


        // configNoCraftCost

        [HarmonyPatch(typeof(WorkshopManager), nameof(WorkshopManager.CreateNewCraftTask))]
        class CreateNewCraftTaskPatch1
        {
            static bool Prefix(ref ItemCraftableData p_data, ref CraftTask __result, ref WorkshopManager __instance)
            {
                if (!configNoCraftCost.Value)
                    return true;
                CraftTask craftTask = new CraftTask(p_data);
                craftTask.Id = xUid.Get();
                if (__instance.CraftTasks == null)
                {
                    __instance.CraftTasks = new List<CraftTask>();
                }
                __instance.CraftTasks.Add(craftTask);
                __result = craftTask;
                return false;
            }

        }

        // configInstantProcessing
        [HarmonyPatch(typeof(RO_RAWProcessor), "ProcessingDuration", MethodType.Getter)]
        class UpdateProcessingPatch1
        {
            static void Postfix(ref float __result, ref RO_RAWProcessor __instance)
            {
                if (!configInstantProcessing.Value)
                    return;
                __result = 0;

            }

        }


        public override void OnInitializeMelon()
        {

            Instance = this;
            
            MultiplierFloatCategory = MelonPreferences.CreateCategory("FloatMultipliers");
            MultiplierIntCategory = MelonPreferences.CreateCategory("IntMultipliers");
            ToggleCategory = MelonPreferences.CreateCategory("Toggles");
            MinAttrCategory = MelonPreferences.CreateCategory("MinimalGaterAttributesInt");
            MinAttrCategory.DisplayName = "Minimal Gater Attributes";

            

            configGaterMinAttr = ToggleCategory.CreateEntry<bool>("configSetGaterMinAttr", true, "Set minimal attributes for Gaters");
            configNoGaterBodyNeeds = ToggleCategory.CreateEntry<bool>("configNoGaterBodyNeeds", true, "Disable all physical needs or injuries for Gaters");
            configNoGaterMentalNeeds = ToggleCategory.CreateEntry<bool>("configNoGaterMentalNeeds", true, "Disable all mental needs for Gaters");
            configNoPowerCost = ToggleCategory.CreateEntry<bool>("configNoPowerCost", true, "Disable power consumption and set max power");
            configNoObjectsCost = ToggleCategory.CreateEntry<bool>("configNoObjectsCost", true, "Disable cost for placeable objects");
            configNoCraftCost = ToggleCategory.CreateEntry<bool>("configNoCraftCost", true, "Disable cost for workshop crafting");
            configAlwaysPassSkill = ToggleCategory.CreateEntry<bool>("configAlwaysPassSkill", true, "Always pass gater skill check");
            configInstantProcessing = ToggleCategory.CreateEntry<bool>("configInstantProcessing", true, "Enable instant processing of minerals");

            configCraftSpeedMultiplier = MultiplierFloatCategory.CreateEntry<float>("configCraftSpeedMultiplier", 2f, "Multiplier for craft speed", validator: new ValueRange<float>(1f, 20f));
            configResearchSpeedMultiplier = MultiplierFloatCategory.CreateEntry<float>("configResearchSpeedMultiplier", 2f, "Multiplier for research speed", validator: new ValueRange<float>(1f, 20f));
            configResearchPointsMultiplier = MultiplierFloatCategory.CreateEntry<float>("configResearchPointsMultiplier", 2f, "Multiplier for research points gain", validator: new ValueRange<float>(1f, 20f));
            configBuildSpeedMultiplier = MultiplierFloatCategory.CreateEntry<float>("configBuildSpeedMultiplier", 2f, "Multiplier for builders work speed", validator: new ValueRange<float>(1f, 20f));
            configInfluenceMultiplier = MultiplierFloatCategory.CreateEntry<float>("configInfluenceMultiplier", 2f, "Multiplier for all influence income", validator: new ValueRange<float>(1f, 20f));
            configMoneyMultiplier = MultiplierFloatCategory.CreateEntry<float>("configMoneyMultiplier", 2f, "Multiplier for all money income", validator: new ValueRange<float>(1f, 20f));
            configWeaponDamageMultiplier = MultiplierFloatCategory.CreateEntry<float>("configWeaponDamageMultiplier", 2f, "Multiplier for Gater weapon damage", validator: new ValueRange<float>(1f, 20f));
            configCompoundMultiplier = MultiplierFloatCategory.CreateEntry<float>("configCompoundMultiplier", 2f, "Multiplier for compound storage capacity", validator: new ValueRange<float>(1f, 10f));
            configGaterXPMultiplier = MultiplierFloatCategory.CreateEntry<float>("configGaterXPMultiplier", 2f, "Multiplier for Gater XP gain", validator: new ValueRange<float>(1, 20));
            

            configAttrBodyMin = MinAttrCategory.CreateEntry<int>("configAttrBodyMin", 2, "Minimal Body Attribute for Gaters", null, false, false, validator: new ValueRange<int>(1, 10));
            configAttrMindMin = MinAttrCategory.CreateEntry<int>("configAttrMindMin", 2, "Minimal Mind Strength Attribute for Gaters", null, false, false, validator: new ValueRange<int>(1, 10));
            configAttrPerceptionMin = MinAttrCategory.CreateEntry<int>("configAttrPerceptionMin", 2, "Minimal Perception Attribute for Gaters", null, false, false, validator: new ValueRange<int>(1, 10));
            configAttrSocialMin = MinAttrCategory.CreateEntry<int>("configAttrSocialMin", 2, "Minimal Social Attribute for Gaters", null, false, false, validator: new ValueRange<int>(1, 10));
            configAttrKnowledgeMin = MinAttrCategory.CreateEntry<int>("configAttrKnowledgeMin", 2, "Minimal Knowledge Attribute for Gaters", null, false, false, validator: new ValueRange<int>(1, 10));
            configAttrLogicMin = MinAttrCategory.CreateEntry<int>("configAttrLogicMin", 2, "Minimal Logic Attribute for Gaters", null, false, false, validator: new ValueRange<int>(1, 10));

            HarmonyInstance.PatchAll();

            JMod.Init(Instance);
            configMenuToggle = JMod.configMenuToggle;
            JMod.Log("ExoCheat Mod Initialized.");
        }

        public override void OnUpdate()
        {
            if (Event.current != null)
                if (Event.current.keyCode == configMenuToggle.Value && Event.current.type == EventType.KeyDown)
                    JMod.SwitchMenu(false);

            if (Event.current != null)
                if (Event.current.keyCode == KeyCode.Escape && Event.current.type == EventType.KeyDown)
                    JMod.SwitchMenu(true);
        }

        public override void OnGUI()
        {
            JMod.ShowMenu();
        }
    }
}