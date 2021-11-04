using DMT;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

public class OcbElectricityLamps
{
    // Entry class for Harmony patching
    public class OcbElectricityLamps_Init : IHarmony
    {
        public void Start()
        {
            Debug.Log("Loading OCB Electricity Lamps Patch: " + GetType().ToString());
            var harmony = new Harmony(GetType().ToString());
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }
    }

    [HarmonyPatch(typeof (TileEntity))]
    [HarmonyPatch("Instantiate")]
    public class TileEntity_Instantiate
    {
        public static bool
        Prefix(ref TileEntity __result, TileEntityType type, Chunk _chunk)
        {
            if (type == (TileEntityType) 244)
            {
                __result = (TileEntity) new TileEntityElectricityLightBlock(_chunk);
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof (GameManager))]
    [HarmonyPatch("OpenTileEntityUi")]
    public class GameManager_OpenTileEntityUi
    {
        public static void Postfix(GameManager __instance, int _entityIdThatOpenedIt, TileEntity _te, string _customUi, World ___m_World)
        {
            LocalPlayerUI uiForPlayer = LocalPlayerUI.GetUIForPlayer(___m_World.GetEntity(_entityIdThatOpenedIt) as EntityPlayerLocal);
            if (_te is TileEntityElectricityLightBlock item)
            {
                if (uiForPlayer == null) return;
                ((XUiC_ElectricityLampsWindowGroup) ((XUiWindowGroup) uiForPlayer.windowManager.GetWindow("electricitylamps")).Controller).TileEntity = item;
                uiForPlayer.windowManager.Open("electricitylamps", true);
            }
        }
    }


    // Main overload to allow wire connections between power items
    // Same patch is also present in overhaul, makes this standalone
    [HarmonyPatch(typeof(TileEntityPowered))]
    [HarmonyPatch("CanHaveParent")]
    public class TileEntityPowered_CanHaveParent
    {
        static bool Prefix(TileEntityPowered __instance, ref bool __result, IPowered powered)
        {
            if (__instance.GetChunk().GetBlock(__instance.localChunkPos) is BlockValue block) {
                var values = Block.list[block.type].Properties.Values;
                // Deny further execution if flag is set (preventing start of power connection)
                __result = !values.ContainsKey("PowerDontConnect") ||
                    !StringParsers.ParseBool(values["PowerDontConnect"]);
                return false;
            }
            return true;
        }
    }

    // Main overload to allow wire connections between power items
    // Same patch is also present in overhaul, makes this standalone
    [HarmonyPatch(typeof(ItemActionConnectPower))]
    [HarmonyPatch("OnHoldingUpdate")]
    public class ItemActionConnectPower_OnHoldingUpdate
    {
        static bool Prefix(ItemActionConnectPower __instance, ItemActionData _actionData)
        {
            Vector3i blockPos = _actionData.invData.hitInfo.hit.blockPos;
            if (_actionData.invData.world.GetBlock(blockPos) is BlockValue block) {
                var values = Block.list[block.type].Properties.Values;
                // Abort further execution if flag is set (preventing start of power connection)
                return !values.ContainsKey("PowerDontConnect") ||
                    !StringParsers.ParseBool(values["PowerDontConnect"]);
            }
            return true;
        }
    }

}