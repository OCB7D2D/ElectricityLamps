using Audio;
using UnityEngine;

public class XUiC_ElectricityLampsWindowGroup : XUiController
{

    private XUiC_ElectricityLampsStats ElectricityLampsStats;

    private TileEntityElectricityLightBlock tileEntity;

    public override void Init()
    {
        base.Init();
        XUiController childByType1 = (XUiController) this.GetChildByType<XUiC_ElectricityLampsStats>();
        if (childByType1 != null)
        {
            this.ElectricityLampsStats = (XUiC_ElectricityLampsStats) childByType1;
            this.ElectricityLampsStats.Owner = this;
        }
    }

    public TileEntityElectricityLightBlock TileEntity
    {
        get => this.tileEntity;
        set
        {
            this.tileEntity = value;
            this.ElectricityLampsStats.TileEntity = this.tileEntity;
        }
    }

    public override bool AlwaysUpdate() => false;

    public override bool OpenBackpackOnOpen() => false;

    public override void OnOpen()
    {
        base.OnOpen();
        if (this.ViewComponent != null && !this.ViewComponent.IsVisible)
        {
            this.ViewComponent.OnOpen();
            this.ViewComponent.IsVisible = true;
        }
        this.xui.RecenterWindowGroup(this.windowGroup);
        for (int index = 0; index < this.children.Count; ++index)
            this.children[index].OnOpen();
        if (this.ElectricityLampsStats != null && this.TileEntity != null)
            this.ElectricityLampsStats.OnOpen();
        if (this.OpenBackpackOnOpen() && (Object) GameManager.Instance != (Object) null && !this.xui.playerUI.windowManager.IsWindowOpen("backpack"))
            this.xui.playerUI.windowManager.Open("backpack", false);
        if (this.xui.playerUI.windowManager.IsWindowOpen("compass"))
            this.xui.playerUI.windowManager.Close("compass");
        Manager.BroadcastPlayByLocalPlayer(this.TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "open_vending");
        this.IsDirty = true;
        this.TileEntity.Destroyed += new XUiEvent_TileEntityDestroyed(this.TileEntity_Destroyed);
    }

    public override void OnClose()
    {
        base.OnClose();
        if (this.xui.playerUI.windowManager.Contains("compass") && !this.xui.playerUI.windowManager.IsWindowOpen("compass"))
            this.xui.playerUI.windowManager.Open("compass", false);
        Manager.BroadcastPlayByLocalPlayer(this.TileEntity.ToWorldPos().ToVector3() + Vector3.one * 0.5f, "close_vending");
        this.TileEntity.Destroyed -= new XUiEvent_TileEntityDestroyed(this.TileEntity_Destroyed);
    }

    private void TileEntity_Destroyed(global::TileEntity te)
    {
        if (this.TileEntity == te)
        {
            if (GameManager.Instance == null) return;
            this.xui.playerUI.windowManager.Close("electricitylamps");
        }
        else
        te.Destroyed -= new XUiEvent_TileEntityDestroyed(this.TileEntity_Destroyed);
    }

}
