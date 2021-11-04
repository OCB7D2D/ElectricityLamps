using System.IO;
using UnityEngine;

public class TileEntityElectricityLightBlock : TileEntityPoweredBlock
{

    private static ushort defKelvin = 3200;

    private static Color defColor = KelvinToColor(defKelvin);

    public static Color KelvinToColor(ushort kelvin)
    {
        float temp = (float)kelvin / 100f;
        float red, green, blue;
        if( temp <= 66 ){ 
            red = 255; 
            green = temp;
            green = 99.4708025861f * Mathf.Log(green) - 161.1195681661f;
            if( temp <= 19){
                blue = 0;
            }
            else {
                blue = temp-10;
                blue = 138.5177312231f * Mathf.Log(blue) - 305.0447927307f;
            }
        }
        else {
            red = temp - 60f;
            red = 329.698727446f * Mathf.Pow(red, -0.1332047592f);
            green = temp - 60f;
            green = 288.1221695283f * Mathf.Pow(green, -0.0755148492f );
            blue = 255;
        }
        return new Color(
            Mathf.Clamp(red / 255f, 0f, 1f),
            Mathf.Clamp(green / 255f, 0f, 1f),
            Mathf.Clamp(blue / 255f, 0f, 1f)
        );
    }

    int lightMode = 0;

    ushort lightTemperature = defKelvin;

    Color lightColor = defColor;

    float lightIntensity = 1f;
    float lightMinIntensity = 0f;
    float lightMaxIntensity = 1f;
    float lightIntensityStep = 0.1f;

    float lightRange = 15f;
    float lightMinRange = 0f;
    float lightMaxRange = 80f;
    float lightRangeStep = 0.5f;

    // Light angle (for spots)
    float lightSpotAngle = 60f;
    float lightMinAngle = 30f;
    float lightMaxAngle = 180f;
    float lightAngleStep = 3f;

    // Light rotation (for spots)
    // Reserved for future use
    float lightRotationRa = 0f;
    float lightRotationDec = 0f;

    public TileEntityElectricityLightBlock(Chunk _chunk) :
        base(_chunk)
    {}

    protected override void SetValuesFromBlock(ushort blockID)
    {
        base.SetValuesFromBlock(blockID);
        var props = Block.list[blockID].Properties;
        this.lightMinIntensity = !props.Values.ContainsKey("LightMinIntensity") ?
            0f : StringParsers.ParseFloat(props.Values["LightMinIntensity"]);
        this.lightMaxIntensity = !props.Values.ContainsKey("LightMaxIntensity") ?
            1f : StringParsers.ParseFloat(props.Values["LightMaxIntensity"]);
        this.lightIntensityStep = !props.Values.ContainsKey("LightIntensityStep") ?
            0.1f : StringParsers.ParseFloat(props.Values["LightIntensityStep"]);
        this.lightMinRange = !props.Values.ContainsKey("LightMinRange") ?
            0f : StringParsers.ParseFloat(props.Values["LightMinRange"]);
        this.lightMaxRange = !props.Values.ContainsKey("LightMaxRange") ?
            80f : StringParsers.ParseFloat(props.Values["LightMaxRange"]);
        this.lightRangeStep = !props.Values.ContainsKey("LightRangeStep") ?
            0.5f : StringParsers.ParseFloat(props.Values["LightRangeStep"]);
        this.lightMinAngle = !props.Values.ContainsKey("LightMinAngle") ?
            30f : StringParsers.ParseFloat(props.Values["LightMinAngle"]);
        this.lightMaxAngle = !props.Values.ContainsKey("LightMaxAngle") ?
            180f : StringParsers.ParseFloat(props.Values["LightMaxAngle"]);
        this.lightAngleStep = !props.Values.ContainsKey("LightAngleStep") ?
            3f : StringParsers.ParseFloat(props.Values["LightAngleStep"]);
        if (this.chunk == null) return;
        BlockEntityData blockEntity = chunk.GetBlockEntity(ToWorldPos());
        if (blockEntity != null) this.UpdateLightState(blockEntity);
    }

    public void PresetDefaultValues(int blockID)
    {
        var props = Block.list[blockID].Properties;
        this.lightTemperature = !props.Values.ContainsKey("LightKelvin") ?
            defKelvin : StringParsers.ParseUInt16(props.Values["LightKelvin"]);
        this.lightColor = !props.Values.ContainsKey("LightColor") ?
            defColor : StringParsers.ParseColor(props.Values["LightColor"]);
        this.lightIntensity = !props.Values.ContainsKey("LightIntensity") ?
            1f : StringParsers.ParseFloat(props.Values["LightIntensity"]);
        this.lightRange = !props.Values.ContainsKey("LightRange") ?
            1f : StringParsers.ParseFloat(props.Values["LightRange"]);
        this.lightSpotAngle = !props.Values.ContainsKey("LightSpotAngle") ?
            60f : StringParsers.ParseFloat(props.Values["LightSpotAngle"]);
        this.lightMode = !props.Values.ContainsKey("LightMode") ?
            (byte)0 : StringParsers.ParseUInt8(props.Values["LightMode"]);
        if (this.chunk == null) return;
        BlockEntityData blockEntity = chunk.GetBlockEntity(ToWorldPos());
        if (blockEntity != null) this.UpdateLightState(blockEntity);
    }

    public override TileEntityType GetTileEntityType()
    {
        // really just an arbitrary number
        // I tend to use number above 241
        return (TileEntityType) 244;
    }

    public byte Mode => (byte)this.lightMode;
    public bool IsKelvinScale {
        get => (this.lightMode & 1) == 1;
        set
        {
            if (value) this.lightMode |= 1;
            else this.lightMode &= ~1;
        }
    }
    public bool IsColorScale => (this.lightMode & 1) != 1;
    public bool IsSpotLight => (this.lightMode & 2) == 2;
    public bool IsPointLight => (this.lightMode & 2) != 2;

    public Color LightColor
    {
        get => this.lightColor;
        set
        {
            this.lightColor = value;
            if (this.chunk == null) return;
            BlockEntityData blockEntity = chunk.GetBlockEntity(ToWorldPos());
            if (blockEntity != null) this.UpdateLightState(blockEntity);
        }
    }

    public float LightIntensity
    {
        get => this.lightIntensity;
        set
        {
            this.lightIntensity = value;
            if (this.chunk == null) return;
            BlockEntityData blockEntity = chunk.GetBlockEntity(ToWorldPos());
            if (blockEntity != null) this.UpdateLightState(blockEntity);
        }
    }

    public ushort LightKelvin
    {
        get => this.lightTemperature;
        set
        {
            this.lightTemperature = value;
            if (this.chunk == null) return;
            BlockEntityData blockEntity = chunk.GetBlockEntity(ToWorldPos());
            if (blockEntity != null) this.UpdateLightState(blockEntity);
        }
    }

    public float LightSpotAngle
    {
        get => this.lightSpotAngle;
        set
        {
            this.lightSpotAngle = value;
            if (this.chunk == null) return;
            BlockEntityData blockEntity = chunk.GetBlockEntity(ToWorldPos());
            if (blockEntity != null) this.UpdateLightState(blockEntity);
        }
    }

    public float LightRange
    {
        get => this.lightRange;
        set
        {
            this.lightRange = value;
            if (this.chunk == null) return;
            BlockEntityData blockEntity = chunk.GetBlockEntity(ToWorldPos());
            if (blockEntity != null) this.UpdateLightState(blockEntity);
        }
    }

    public float MinSpotAngle => this.lightMinAngle;
    public float MaxSpotAngle => this.lightMaxAngle;
    public float SpotAngleStep => this.lightAngleStep;
    public float MinLightRange => this.lightMinRange;
    public float MaxLightRange => this.lightMaxRange;
    public float LightRangeStep => this.lightRangeStep;
    public float MinLightIntensity => this.lightMinIntensity;
    public float MaxLightIntensity => this.lightMaxIntensity;
    public float LightIntensityStep => this.lightIntensityStep;

    public void UpdateLightState(BlockEntityData blockEntity)
    {

        if (blockEntity == null) return;
        if (blockEntity.transform == null) return;

        // THis may only work in Single player mode, otherwise use ClientData!?
        TileEntityElectricityLightBlock tileEntity = this;

        // Only available on SinglePlayer instance?
        bool _isOn = IsPowered && IsToggled;

        Color color = tileEntity.LightColor;
        if (tileEntity.IsKelvinScale) color = KelvinToColor(tileEntity.LightKelvin);
        float range = Mathf.Clamp(tileEntity.LightRange, tileEntity.lightMinRange, tileEntity.lightMaxRange);
        float angle = Mathf.Clamp(tileEntity.LightSpotAngle, tileEntity.lightMinAngle, tileEntity.lightMaxAngle);
        float intensity = Mathf.Clamp(tileEntity.LightIntensity, tileEntity.lightMinIntensity, tileEntity.lightMaxIntensity);

        if (blockEntity.transform.Find("MainLight") is Transform transform1)
        {
            if (transform1.GetComponent<LightLOD>() is LightLOD component)
            {
                component.SetEmissiveColor(color * intensity);
                component.MaxIntensity = intensity;
                if (component.GetLight() is Light light) {
                    light.range = range;
                    if (tileEntity.IsSpotLight) {
                        light.spotAngle = angle;
                    }
                    light.color = color;
                }
                component.SwitchOnOff(_isOn);
            }
        }
        if (blockEntity.transform.Find("SeparatedLensFlare") is Transform transform2)
        {
            if (transform2.GetComponent<LightLOD>() is LightLOD component) {
                component.SwitchOnOff(_isOn);
            }
        }
        
        if (blockEntity.transform.Find("BulbGlow") is Transform transform3)
        {
            if (transform3.GetComponent<MeshRenderer>() is MeshRenderer component) {
                if (component.material != null) {
                    component.material.SetColor("_EmissionColor", color * intensity * 1.25f);
                    if (_isOn) component.material.EnableKeyword("_EMISSION");
                    else component.material.DisableKeyword("_EMISSION");
                }
                component.enabled = true;
            }
        }
        if (blockEntity.transform.Find("ExtraPointLight") is Transform transform4)
        {
            if (transform4.GetComponent<LightLOD>() is LightLOD component) {
                component.SwitchOnOff(_isOn);
            }
        }
        if (blockEntity.transform.Find("Point light") is Transform transform5)
        {
            if (transform5.GetComponent<LightLOD>() is LightLOD component) {
                component.SwitchOnOff(_isOn);
            }
        }
    }

    public override void read(PooledBinaryReader _br, TileEntity.StreamModeRead _eStreamMode)
    {
        BlockEntityData blockEntity = null;
        base.read(_br, _eStreamMode);
        this.lightMode = _br.ReadByte();
        this.lightIntensity = _br.ReadSingle();
        this.lightTemperature = _br.ReadUInt16();
        this.lightRange = _br.ReadSingle();
        if (this.IsSpotLight) {
            this.lightSpotAngle = _br.ReadSingle();
            this.lightRotationRa = _br.ReadSingle();
            this.lightRotationDec = _br.ReadSingle();
        }
        this.lightColor = NetworkUtils.ReadColor32((BinaryReader) _br);
        if (chunk != null) blockEntity = chunk.GetBlockEntity(ToWorldPos());
        if (blockEntity != null) this.UpdateLightState(blockEntity);
    }

    public override void write(PooledBinaryWriter _bw, TileEntity.StreamModeWrite _eStreamMode)
    {
        base.write(_bw, _eStreamMode);
        _bw.Write(this.Mode); // byte
        _bw.Write(this.LightIntensity);
        _bw.Write(this.LightKelvin);
        _bw.Write(this.lightRange);
        if (this.IsSpotLight) {
            _bw.Write(this.lightSpotAngle);
            _bw.Write(this.lightRotationRa);
            _bw.Write(this.lightRotationDec);
        }
        NetworkUtils.WriteColor32((BinaryWriter) _bw, this.LightColor);
    }

    protected override PowerItem CreatePowerItem() {
        return base.CreatePowerItem();
    } 

    protected override void setModified()
    {
        base.setModified();
        if (this.chunk == null) return;
        BlockEntityData blockEntity = chunk.GetBlockEntity(ToWorldPos());
        if (blockEntity != null) this.UpdateLightState(blockEntity);
    }

}
