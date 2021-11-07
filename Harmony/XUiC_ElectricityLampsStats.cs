using Audio;
using UnityEngine;

public class XUiC_ElectricityLampsStats : XUiController
{

    public XUiC_ElectricityLampsWindowGroup Owner { get; set; }

    private int BlockType;

    private TileEntityElectricityLightBlock tileEntity;

    private PowerItem powerItem;

    public TileEntityElectricityLightBlock TileEntity
    {
        get => this.tileEntity;
        set
        {
            this.tileEntity = value;
            if (!SingletonMonoBehaviour<ConnectionManager>.Instance.IsServer)
                return;
            this.powerItem = this.tileEntity.GetPowerItem() as PowerItem;
        }
    }

    private XUiC_ColorPicker uiColorPicker;
    private XUiC_ComboBoxBool uiUseKelvin;
    private XUiC_ComboBoxInt uiTemperature;
    private XUiC_ComboBoxFloat uiIntensity;
    private XUiC_ComboBoxFloat uiSpotAngle;
    private XUiC_ComboBoxFloat uiRange;


    public override void Init()
    {
        base.Init();

        var uiColorPicker = this.GetChildById("uiColorPicker");

        this.uiUseKelvin = (XUiC_ComboBoxBool) this.GetChildById("uiUseKelvin");
        if (this.uiUseKelvin != null) {
            this.uiUseKelvin.OnValueChanged +=
                new XUiC_ComboBox<bool>.XUiEvent_ValueChanged(this.uiUseKelvin_OnValueChanged);
        } else {
            Log.Warning("ElectricityLampsStats missing uiUseKelvin");
        }

        this.uiTemperature = (XUiC_ComboBoxInt) this.GetChildById("uiTemperature");
        if (this.uiTemperature != null) {
            this.uiTemperature.OnValueChanged +=
                new XUiC_ComboBox<long>.XUiEvent_ValueChanged(this.uiTemperature_OnValueChanged);
        } else {
            Log.Warning("ElectricityLampsStats missing uiTemperature");
        }

        this.uiIntensity = (XUiC_ComboBoxFloat) this.GetChildById("uiIntensity");
        if (this.uiIntensity != null) {
            this.uiIntensity.OnValueChanged +=
                new XUiC_ComboBox<double>.XUiEvent_ValueChanged(this.uiIntensity_OnValueChanged);
        } else {
            Log.Warning("ElectricityLampsStats missing uiIntensity");
        }

        this.uiSpotAngle = (XUiC_ComboBoxFloat) this.GetChildById("uiSpotAngle");
        if (this.uiSpotAngle != null) {
            this.uiSpotAngle.OnValueChanged +=
                new XUiC_ComboBox<double>.XUiEvent_ValueChanged(this.uiSpotAngle_OnValueChanged);
        } else {
            Log.Warning("ElectricityLampsStats missing uiSpotAngle");
        }

        this.uiRange = (XUiC_ComboBoxFloat) this.GetChildById("uiRange");
        if (this.uiRange != null) {
            this.uiRange.OnValueChanged +=
                new XUiC_ComboBox<double>.XUiEvent_ValueChanged(this.uiRange_OnValueChanged);
        } else {
            Log.Warning("ElectricityLampsStats missing uiRange");
        }

        this.uiColorPicker = (XUiC_ColorPicker) this.GetChildById("uiColorPicker");
        if (this.uiColorPicker != null) {
            this.uiColorPicker.OnSelectedColorChanged +=
                new XUiEvent_SelectedColorChanged(this.uiColorPicker_OnValueChanged);
        } else {
            Log.Warning("ElectricityLampsStats missing uiColorPicker");
        }

    }

    private void uiColorPicker_OnValueChanged(Color _newValue)
    {
        if (TileEntity != null) {
            TileEntity.LightColor = _newValue;
            BlockEntityData blockEntity = TileEntity.GetChunk().GetBlockEntity(TileEntity.ToWorldPos());
            if (blockEntity != null) TileEntity.UpdateLightState(blockEntity);
        }
        this.RefreshBindings();
    }


    private void uiSpotAngle_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
    {
        if (TileEntity != null) {
            TileEntity.LightSpotAngle = (float)_newValue;
            BlockEntityData blockEntity = TileEntity.GetChunk().GetBlockEntity(TileEntity.ToWorldPos());
            if (blockEntity != null) TileEntity.UpdateLightState(blockEntity);
        }
        this.RefreshBindings();
    }

    private void uiRange_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
    {
        if (TileEntity != null) {
            TileEntity.LightRange = (float)_newValue;
            BlockEntityData blockEntity = TileEntity.GetChunk().GetBlockEntity(TileEntity.ToWorldPos());
            if (blockEntity != null) TileEntity.UpdateLightState(blockEntity);
        }
        this.RefreshBindings();
    }

    private void uiIntensity_OnValueChanged(XUiController _sender, double _oldValue, double _newValue)
    {
        if (TileEntity != null) {
            TileEntity.LightIntensity = (float)_newValue;
            BlockEntityData blockEntity = TileEntity.GetChunk().GetBlockEntity(TileEntity.ToWorldPos());
            if (blockEntity != null) TileEntity.UpdateLightState(blockEntity);
        }
        this.RefreshBindings();
    }

    private void uiTemperature_OnValueChanged(XUiController _sender, long _oldValue, long _newValue)
    {
        if (TileEntity != null) {
            TileEntity.LightKelvin = (ushort)_newValue;
            BlockEntityData blockEntity = TileEntity.GetChunk().GetBlockEntity(TileEntity.ToWorldPos());
            if (blockEntity != null) TileEntity.UpdateLightState(blockEntity);
        }
        this.RefreshBindings();
    }

    private void uiUseKelvin_OnValueChanged(XUiController _sender, bool _oldValue, bool _newValue)
    {
        if (TileEntity != null) {
            TileEntity.IsKelvinScale = _newValue;
            BlockEntityData blockEntity = TileEntity.GetChunk().GetBlockEntity(TileEntity.ToWorldPos());
            if (blockEntity != null) TileEntity.UpdateLightState(blockEntity);
        }
        this.RefreshBindings();
    }

    public string GetBlockProperty(string name, string fallback)
    {
        if (Block.list[BlockType].Properties == null) return fallback;
        if (Block.list[BlockType].Properties.Values.ContainsKey(name)) {
            return Block.list[BlockType].Properties.Values[name];
        }
        return fallback;
    }

    public override bool GetBindingValue(ref string value, BindingItem binding)
    {
        switch (binding.FieldName)
        {	
        case "IsColorScale":
            value = tileEntity != null && tileEntity.IsColorScale ? "true" : "false";
            return true;
        case "IsKelvinScale":
            value = tileEntity != null && tileEntity.IsKelvinScale ? "true" : "false";
            return true;
        case "IsSpotLight":
            value = tileEntity != null && tileEntity.IsSpotLight ? "true" : "false";
            return true;
        case "IsPointLight":
            value = tileEntity != null && tileEntity.IsPointLight ? "true" : "false";
            return true;
        case "MinLightIntensity":
            value = GetBlockProperty("MinLightIntensity", "0");
            return true;
        case "MaxLightIntensity":
            value = GetBlockProperty("LightMaxIntensity", "2");
            return true;
        case "LightIntensityStep":
            value = GetBlockProperty("LightIntensityStep", "0.1");
            return true;
        case "MinLightRange":
            value = GetBlockProperty("LightMinRange", "0");
            return true;
        case "MaxLightRange":
            value = GetBlockProperty("LightMaxRange", "80");
            return true;
        case "LightRangeStep":
            value = GetBlockProperty("LightRangeStep", "0.5");
            return true;
        case "MinSpotAngle":
            value = GetBlockProperty("LightMinSpotAngle", "30");
            return true;
        case "MaxSpotAngle":
            value = GetBlockProperty("LightMaxSpotAngle", "180");
            return true;
        case "SpotAngleStep":
            value = GetBlockProperty("LightSpotAngleStep", "3");
            return true;
        case "IsModeNotLocked":
            value = (!StringParsers.ParseBool(GetBlockProperty("LightModeLocked", "false"))).ToString();
            return true;
        case "IsPoweredPOI":
            value = StringParsers.ParseBool(GetBlockProperty("PoweredPOI", "false")).ToString();
            return true;
        default:
            return false;
        }
    }

    public override void Update(float _dt)
    {
        if ((UnityEngine.Object) GameManager.Instance == (UnityEngine.Object) null && GameManager.Instance.World == null || this.tileEntity == null) return;
        base.Update(_dt);
        this.RefreshBindings();
    }

    public override void OnOpen()
    {
        if (this.TileEntity != null) {
            BlockType = TileEntity.GetChunk().GetBlock(TileEntity.localChunkPos).type;
            if (uiColorPicker != null) uiColorPicker.SelectedColor = this.tileEntity.LightColor;
            if (uiUseKelvin != null) uiUseKelvin.Value = this.TileEntity.IsKelvinScale;
            if (uiIntensity != null) uiIntensity.Value = this.tileEntity.LightIntensity;
            if (uiTemperature != null) uiTemperature.Value = this.tileEntity.LightKelvin;
            if (uiRange != null) uiRange.Value = this.tileEntity.LightRange;
            if (uiSpotAngle != null) uiSpotAngle.Value = this.tileEntity.LightSpotAngle;
        }
        base.OnOpen();
        // Start copy from XUiC_PowerSourceStats
        this.tileEntity.SetUserAccessing(true);
        this.RefreshBindings();
        this.tileEntity.SetModified();
        // End copy from XUiC_PowerSourceStats
    }
    public override void OnClose()
    {
        // Start copy from XUiC_PowerSourceStats
        GameManager instance = GameManager.Instance;
        Vector3i worldPos = this.tileEntity.ToWorldPos();
        if (!XUiC_CameraWindow.hackyIsOpeningMaximizedWindow)
        {
            this.tileEntity.SetUserAccessing(false);
            var uiColorPicker = this.GetChildById("uiColorPicker");
            if (uiColorPicker is XUiC_ColorPicker cp) {
                this.tileEntity.LightColor = cp.SelectedColor;
            }
            instance.TEUnlockServer(this.tileEntity.GetClrIdx(), worldPos, this.tileEntity.entityId);
            this.tileEntity.SetModified();
            this.powerItem = (PowerItem) null;
        }
        base.OnClose();
    }

}
