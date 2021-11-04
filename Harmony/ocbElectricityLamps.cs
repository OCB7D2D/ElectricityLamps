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

}