using System.IO;
using UnityEngine;
using Platform;

public class TileEntityElectricityLightBlock : TileEntityPoweredBlock
{

    // Default kelvin temperature for lights
    private static ushort defKelvin = 3200;
    // Default custom color for lights
    private static Color defColor = KelvinToColor(defKelvin);

    // Null vector (0, 0, 0) to be used when needed
    private static Vector3 nullVector = new Vector3(0, 0, 0);

    public byte LightMode => (byte)this.lightMode;
    public bool IsKelvinScale
    {
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

    public float LightIntensity
    {
        get => this.lightIntensity;
        set
        {
            this.lightIntensity = value;
            this.UpdateLightState();
        }
    }

    public float LightRange
    {
        get => this.lightRange;
        set
        {
            this.lightRange = value;
            this.UpdateLightState();
        }
    }

    public ushort LightKelvin
    {
        get => this.lightKelvin;
        set
        {
            this.lightKelvin = value;
            this.UpdateLightState();
        }
    }

    public Color LightColor
    {
        get => this.lightColor;
        set
        {
            this.lightColor = value;
            this.UpdateLightState();
        }
    }

    public float LightAngle
    {
        get => this.lightAngle;
        set
        {
            this.lightAngle = value;
            this.UpdateLightState();
        }
    }

    // Used for future POI light idea
    // This stores the flicker effects
    public LightStateType LightState;
    public float Rate = 1f;
    public float Delay = 1f;

    // Implementation for POI light idea
    // Flag if this light is claimed by anyone
    public bool IsClaimed = false;

    // Flag if block behaves as a powered POI
    // Get free power while not claimed, otherwise
    // players must power the light by themselves.
    public bool IsPoweredPOI = false;

    // State of last tick check for powered POIs
    // Only do the check once in a while (save CPU)
    private float nextTickCheck = 0;

    // Flag to emit an interesting warning once
    // Debugs some state I haven't seen yet
    private static bool warnOnce = true;

    // Meta byte for bool states
    private int lightMode = 0;

    // Intensity as passed to light LOD
    private float lightIntensity = 1f;
    // Range as passed to light LOD
    private float lightRange = 15f;

    // Only used if mode is color
    private Color lightColor = defColor;
    // Only used if mode is color
    private ushort lightKelvin = defKelvin;

    // Light beam angle (for spots)
    private float lightAngle = 60f;

    // User light rotation (for spots)
    // Reserved for future use only
    float lightRotationRa = 0f;
    float lightRotationDec = 0f;

    // Flag if light should be rotated
    // Also rotates backed light shadows?
    bool isLightReRotated = false;

    // Static light re-orientation as given by block
    Vector3 lightOrientation = new Vector3();

    public override TileEntity Clone()
    {
        TileEntityElectricityLightBlock te = new TileEntityElectricityLightBlock(this.chunk);
        te.lightMode = this.lightMode;
        te.lightIntensity = this.lightIntensity;
        te.lightRange = this.lightRange;
        te.lightColor = this.lightColor;
        te.lightKelvin = this.lightKelvin;
        te.lightAngle = this.lightAngle;
        te.lightRotationRa = this.lightRotationRa;
        te.lightRotationDec = this.lightRotationDec;
        te.isLightReRotated = this.isLightReRotated;
        // te.LightShadows = this.LightShadows;
        te.LightState = this.LightState;
        te.Rate = this.Rate;
        te.Delay = this.Delay;
        return te;
    }

    public override void CopyFrom(TileEntity _other)
    {
        // Prefabs should only have lights
        if (_other is TileEntityLight light)
        {
            this.lightMode = light.LightType == LightType.Spot ? 2 : 0;
            this.lightIntensity = light.LightIntensity;
            this.lightRange = light.LightRange;
            this.lightColor = light.LightColor;
            this.lightKelvin = defKelvin;
            this.lightAngle = light.LightAngle;
            this.lightRotationRa = 0f;
            this.lightRotationDec = 0f;
            this.isLightReRotated = false;
            // this.LightShadows = light.LightShadows;
            this.LightState = light.LightState;
            this.Rate = light.Rate;
            this.Delay = light.Delay;

        }
        // How should they know about me?
        else if (_other is TileEntityElectricityLightBlock te)
        {
            this.lightMode = te.lightMode;
            this.lightIntensity = te.LightIntensity;
            this.lightRange = te.LightRange;
            this.lightColor = te.LightColor;
            this.lightKelvin = te.lightKelvin;
            this.lightAngle = te.lightAngle;
            this.lightRotationRa = te.lightRotationRa;
            this.lightRotationDec = te.lightRotationDec;
            this.isLightReRotated = te.isLightReRotated;
            // this.LightShadows = te.LightShadows;
            this.LightState = te.LightState;
            this.Rate = te.Rate;
            this.Delay = te.Delay;
        }

    }

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


    public TileEntityElectricityLightBlock(Chunk _chunk) :
        base(_chunk)
    {}

    protected override void SetValuesFromBlock(ushort blockID)
    {
        base.SetValuesFromBlock(blockID);
        var props = Block.list[blockID].Properties;
        this.isLightReRotated = props.Values.ContainsKey("LightOrientation");
        this.lightOrientation = !isLightReRotated ? nullVector :
            StringParsers.ParseVector3(props.Values["LightOrientation"]);
        this.UpdateLightState();
    }

    public void PresetDefaultValues(int blockID)
    {
        var props = Block.list[blockID].Properties;
        this.lightKelvin = !props.Values.ContainsKey("LightKelvin") ?
            defKelvin : StringParsers.ParseUInt16(props.Values["LightKelvin"]);
        this.lightColor = !props.Values.ContainsKey("LightColor") ?
            defColor : StringParsers.ParseColor(props.Values["LightColor"]);
        this.lightIntensity = !props.Values.ContainsKey("LightIntensity") ?
            1f : StringParsers.ParseFloat(props.Values["LightIntensity"]);
        this.lightRange = !props.Values.ContainsKey("LightRange") ?
            1f : StringParsers.ParseFloat(props.Values["LightRange"]);
        this.lightAngle = !props.Values.ContainsKey("LightAngle") ?
            60f : StringParsers.ParseFloat(props.Values["LightAngle"]);
        this.lightMode = !props.Values.ContainsKey("LightMode") ?
            (byte)0 : StringParsers.ParseUInt8(props.Values["LightMode"]);
        this.UpdateLightState();
    }

    public override TileEntityType GetTileEntityType()
    {
        // really just an arbitrary number
        // I tend to use number above 241
        return (TileEntityType) 244;
    }

    private void UpdateLightLOD(LightLOD lod)
    {
        Color color = LightColor;
        if (IsKelvinScale) color = KelvinToColor(LightKelvin);
        lod.EmissiveColor = color * LightIntensity;
        lod.MaxIntensity = LightIntensity;
        lod.LightStateType = LightState;
        lod.StateRate = Rate;
        lod.FluxDelay = Delay;
        if (lod.GetLight() is Light light) {
            // Craps without a light
            lod.SetRange(LightRange);
            light.spotAngle = LightAngle;
            light.color = color;
            // Force the specific type
            light.type = IsPointLight
                ? LightType.Point
                : LightType.Spot;
            // Internally fetches it from chunk
            BlockValue block = blockValue;
            if (block.type != BlockValue.Air.type)
            {
                DynamicProperties props = Block.list[block.type].Properties;
                if (props.Values.ContainsKey("LightShadow"))
                {
                    light.shadows = props.Values["LightShadow"].Equals("Soft")
                        ? LightShadows.Soft : LightShadows.Hard;
                }
                if (props.Values.ContainsKey("LightShadowBias"))
                {
                    light.shadowBias = StringParsers.
                        ParseFloat(props.Values["LightShadowBias"]);
                }
                if (props.Values.ContainsKey("LightShadowStrength"))
                {
                    light.shadowStrength = StringParsers.
                        ParseFloat(props.Values["LightShadowStrength"]);
                }
                if (props.Values.ContainsKey("LightPosition"))
                {
                    light.transform.localPosition = StringParsers.
                        ParseVector3(props.Values["LightPosition"]);
                }
                if (props.Values.ContainsKey("LightShadowNearPlane"))
                {
                    light.shadowNearPlane = StringParsers.
                        ParseFloat(props.Values["LightShadowNearPlane"]);
                }
                // Disabled until verified again in A20
                // if (props.Values.ContainsKey("LightOrientation"))
                // {
                //     light.transform.localEulerAngles = StringParsers.
                //         ParseVector3(props.Values["LightOrientation"]);
                // }

            }
        }
        lod.SetEmissiveColor();
    }

    public override void UpdateTick(World world)
    {
        if (this.chunk == null) return;
        if (Time.fixedTime > nextTickCheck) {
            this.UpdateLightState();
            nextTickCheck = Time.fixedTime + 150f +
                world.GetGameRandom().RandomRange(90f);
        }
    }

    public void UpdateLightState()
    {
        if (this.chunk == null) return;
        // Update the light state for the block if one is already given
        BlockEntityData blockEntity = chunk.GetBlockEntity(ToWorldPos());
        if (blockEntity != null) this.UpdateLightState(blockEntity);
    }

    public void UpdateLightState(BlockEntityData blockEntity)
    {

        if (blockEntity == null) return;
        if (blockEntity.transform == null) return;

        // THis may only work in Single player mode, otherwise use ClientData!?
        TileEntityElectricityLightBlock tileEntity = this;

        // Only available on SinglePlayer instance?
        bool _isOn = (IsPoweredPOI || IsPowered) && IsToggled;

        // If light is claimed by user, turn off free energy
        if (!GameManager.IsDedicatedServer && IsPoweredPOI) {
            IsClaimed = GameManager.Instance.World.IsMyLandProtectedBlock(ToWorldPos(),
                GameManager.Instance.World.GetGameManager().GetPersistentLocalPlayer());
            if (IsClaimed && !IsPowered) _isOn = false;
        }

        Color color = tileEntity.LightColor;
        if (tileEntity.IsKelvinScale) color = KelvinToColor(tileEntity.LightKelvin);

        // float range = Mathf.Clamp(tileEntity.LightRange, tileEntity.lightMinRange, tileEntity.lightMaxRange);
        // float angle = Mathf.Clamp(tileEntity.LightBeamAngle, tileEntity.lightMinAngle, tileEntity.lightMaxAngle);
        // float intensity = Mathf.Clamp(tileEntity.LightIntensity, tileEntity.lightMinIntensity, tileEntity.lightMaxIntensity);

        float range = tileEntity.LightRange;
        float angle = tileEntity.LightAngle;
        float intensity = tileEntity.LightIntensity;

        if (blockEntity.transform.Find("MainLight") is Transform transform1)
        {
            if (isLightReRotated) transform1.localEulerAngles  = lightOrientation;
            if (transform1.GetComponent<LightLOD>() is LightLOD component)
            {
                UpdateLightLOD(component);
                component.SwitchOnOff(_isOn, Vector3i.min);
            }
        }
        if (blockEntity.transform.Find("SeparatedLensFlare") is Transform transform2)
        {
            if (transform2.GetComponent<LightLOD>() is LightLOD component) {
                component.SwitchOnOff(_isOn, Vector3i.min);
            }
        }
        if (blockEntity.transform.Find("BulbGlow") is Transform transform3)
        {
            if (transform3.GetComponent<MeshRenderer>() is MeshRenderer component) {
                if (component.material != null) {
                    component.material.SetColor("_EmissionColor", color * intensity * 1.5f);
                    if (_isOn) component.material.EnableKeyword("_EMISSION");
                    else component.material.DisableKeyword("_EMISSION");
                }
                component.enabled = true;
            }
        }
        if (blockEntity.transform.Find("ExtraPointLight") is Transform transform4)
        {
            if (warnOnce) Log.Warning("LightLOD => Light Model has ExtraPointLight");
            if (transform4.GetComponent<LightLOD>() is LightLOD component) {
                if (isLightReRotated) transform4.localEulerAngles = lightOrientation;
                UpdateLightLOD(component);
                component.SwitchOnOff(_isOn, Vector3i.min);
            }
            warnOnce = false;
        }
        if (blockEntity.transform.Find("Point light") is Transform transform5)
        {
            if (warnOnce) Log.Warning("LightLOD => Light Model has Point Light");
            if (transform5.GetComponent<LightLOD>() is LightLOD component) {
                if (isLightReRotated) transform5.localEulerAngles = lightOrientation;
                UpdateLightLOD(component);
                component.SwitchOnOff(_isOn, Vector3i.min);
            }
            warnOnce = false;
        }
    }

    public override void read(PooledBinaryReader _br, StreamModeRead _eStreamMode)
    {
        // Call PoweredBlock base reader
        base.read(_br, _eStreamMode);
        // Allow for future additions
        byte version = 0; // current version
        // Only needed to upgrade persisted data
        if (_eStreamMode == StreamModeRead.Persistency)
        {
            version = _br.ReadByte();
        }
        // Read all light options
        this.lightMode = _br.ReadByte();
        this.lightRange = _br.ReadSingle();
        this.lightIntensity = _br.ReadSingle();
        this.lightKelvin = _br.ReadUInt16();
        this.lightColor = StreamUtils.ReadColor32(_br);
        // Additional spotlight properties
        if (this.IsSpotLight)
        {
            this.lightAngle = _br.ReadSingle();
            this.lightRotationRa = _br.ReadSingle();
            this.lightRotationDec = _br.ReadSingle();
        }
        this.UpdateLightState();
    }

    public override void write(PooledBinaryWriter _bw, StreamModeWrite _eStreamMode)
    {
        // Call PoweredBlock base writer
        base.write(_bw, _eStreamMode);
        // Write protocol version
        if (_eStreamMode == StreamModeWrite.Persistency)
        {
            _bw.Write((byte)0); // byte
        }
        // Write all light options
        _bw.Write(this.LightMode); // byte
        _bw.Write(this.lightRange);
        _bw.Write(this.LightIntensity);
        _bw.Write(this.LightKelvin);
        StreamUtils.WriteColor32(_bw, this.LightColor);
        // Additional spotlight properties
        if (this.IsSpotLight) {
            _bw.Write(this.lightAngle);
            _bw.Write(this.lightRotationRa);
            _bw.Write(this.lightRotationDec);
        }
    }

    protected override PowerItem CreatePowerItem() {
        return base.CreatePowerItem();
    }

    protected override void setModified()
    {
        base.setModified();
        this.UpdateLightState();
    }

}
