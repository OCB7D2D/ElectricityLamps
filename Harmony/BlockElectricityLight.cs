using UnityEngine;

public class BlockElectricityLight : BlockPowered
{

    private BlockActivationCommand[] cmds = new BlockActivationCommand[3]
    {
        new BlockActivationCommand("light", "electric_switch", true),
        new BlockActivationCommand("options", "tool", true),
        new BlockActivationCommand("take", "hand", false)
    };

    public override byte GetLightValue(BlockValue _blockValue) => ((int) _blockValue.meta & 2) == 0 ? (byte) 0 : base.GetLightValue(_blockValue);


    public override string GetActivationText(
        WorldBase _world,
        BlockValue _blockValue,
        int _clrIdx,
        Vector3i _blockPos,
        EntityAlive _entityFocusing)
    {
        TileEntityElectricityLightBlock tileEntity = (TileEntityElectricityLightBlock) _world.GetTileEntity(_clrIdx, _blockPos);
        if (tileEntity != null)
        {
            PlayerActionsLocal playerInput = ((EntityPlayerLocal) _entityFocusing).playerInput;
            string str = playerInput.Activate.GetBindingXuiMarkupString() + playerInput.PermanentActions.Activate.GetBindingXuiMarkupString();
            return tileEntity.IsToggled ? string.Format(Localization.Get("useSwitchLightOff"), (object) str) : string.Format(Localization.Get("useSwitchLightOn"), (object) str);
        }
        return (string) null;
    }

    public override void OnBlockAdded(
        WorldBase _world,
        Chunk _chunk,
        Vector3i _blockPos,
        BlockValue _blockValue)
    {
        base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
        if (_world.GetTileEntity(_chunk.ClrIdx, _blockPos) is TileEntityElectricityLightBlock) return;
        TileEntityPowered tileEntity = this.CreateTileEntity(_chunk);
        tileEntity.localChunkPos = World.toBlock(_blockPos);
        tileEntity.InitializePowerData();
        if (tileEntity is TileEntityElectricityLightBlock toggle) {
            toggle.IsToggled = ((uint)_blockValue.meta & 2U) > 0U;
            if (toggle.GetPowerItem() is PowerConsumerToggle consumer) {
                consumer.IsToggled = toggle.IsToggled;
            }
        }
        _chunk.AddTileEntity((TileEntity) tileEntity);
    }

    public override bool OnBlockActivated(
        int _indexInBlockActivationCommands,
        WorldBase _world,
        int _cIdx,
        Vector3i _blockPos,
        BlockValue _blockValue,
        EntityAlive _player)
    {
        TileEntityElectricityLightBlock tileEntity = (TileEntityElectricityLightBlock) _world.GetTileEntity(_cIdx, _blockPos);
        switch (_indexInBlockActivationCommands)
        {
            case 0:
                if (!_world.IsEditor() && tileEntity != null)
                {
                    tileEntity.IsToggled = !tileEntity.IsToggled;
                    _blockValue.meta = (byte) ((int) _blockValue.meta & -3 | (tileEntity.IsToggled ? 2 : 0));
                    _world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
                    break;
                }
                break;
            case 1:
                _player.AimingGun = false;
                if (tileEntity == null) return false;
                Vector3i worldPos = tileEntity.ToWorldPos();
                _world.GetGameManager().TELockServer(_cIdx, worldPos, tileEntity.entityId, _player.entityId);
                return true;
            case 2:
                this.TakeItemWithTimer(_cIdx, _blockPos, _blockValue, _player);
                return true;
        }
        return false;	
    }

    public override BlockActivationCommand[] GetBlockActivationCommands(
        WorldBase _world,
        BlockValue _blockValue,
        int _clrIdx,
        Vector3i _blockPos,
        EntityAlive _entityFocusing)
    {
        TileEntityElectricityLightBlock tileEntity = (TileEntityElectricityLightBlock) _world.GetTileEntity(_clrIdx, _blockPos);
        bool flag = _world.IsMyLandProtectedBlock(_blockPos, _world.GetGameManager().GetPersistentLocalPlayer());
        var props = Block.list[_blockValue.type].Properties;
        bool isPoweredPOI = !props.Values.ContainsKey("PoweredPOI") ?
            false : StringParsers.ParseBool(props.Values["PoweredPOI"]);
        this.cmds[0].enabled = true;
        this.cmds[1].enabled = _world.IsEditor() || flag && tileEntity != null;
        this.cmds[2].enabled = !isPoweredPOI && flag && (double) this.TakeDelay > 0.0;
        return this.cmds;
    }

    public override void Init()
    {
        base.Init();
    }

    private bool updateLightState(
        WorldBase _world,
        int _cIdx,
        Vector3i _blockPos,
        BlockValue _blockValue,
        bool _bSwitchLight = false)
    {

        // THis may only work in Single player mode, otherwise use ClientData!?
        TileEntityElectricityLightBlock tileEntity = (TileEntityElectricityLightBlock) _world.GetTileEntity(_cIdx, _blockPos);
        if (tileEntity == null) return true;
        BlockEntityData blockEntity = tileEntity.GetChunk().GetBlockEntity(_blockPos);
        if (blockEntity == null) return true;
        var props = Block.list[_blockValue.type].Properties;
        tileEntity.IsPoweredPOI = !props.Values.ContainsKey("PoweredPOI") ?
            false : StringParsers.ParseBool(props.Values["PoweredPOI"]);
        tileEntity.UpdateLightState(blockEntity);
        return true;
    }

    public override void OnBlockValueChanged(
        WorldBase _world,
        Chunk _chunk,
        int _clrIdx,
        Vector3i _blockPos,
        BlockValue _oldBlockValue,
        BlockValue _newBlockValue)
    {
        base.OnBlockValueChanged(_world, _chunk, _clrIdx, _blockPos, _oldBlockValue, _newBlockValue);
        this.updateLightState(_world, _clrIdx, _blockPos, _newBlockValue);
    }

    public override void OnBlockEntityTransformAfterActivated(
        WorldBase _world,
        Vector3i _blockPos,
        int _cIdx,
        BlockValue _blockValue,
        BlockEntityData _ebcd)
    {
        base.OnBlockEntityTransformAfterActivated(_world, _blockPos, _cIdx, _blockValue, _ebcd);
        this.updateLightState(_world, _cIdx, _blockPos, _blockValue);
    }

    public override bool ActivateBlock(
        WorldBase _world,
        int _cIdx,
        Vector3i _blockPos,
        BlockValue _blockValue,
        bool isOn,
        bool isPowered)
    {
        var props = Block.list[_blockValue.type].Properties;
        isOn = !props.Values.ContainsKey("PoweredPOI") ? isOn:
            StringParsers.ParseBool(props.Values["PoweredPOI"]);
        _blockValue.meta = (byte) ((int) _blockValue.meta & -3 | (isOn ? 2 : 0));
        _world.SetBlockRPC(_cIdx, _blockPos, _blockValue);
        this.updateLightState(_world, _cIdx, _blockPos, _blockValue);
        return true;
    }

    public override TileEntityPowered CreateTileEntity(Chunk chunk)
    {
        TileEntityElectricityLightBlock entityPoweredBlock = new TileEntityElectricityLightBlock(chunk);
        entityPoweredBlock.PowerItemType = PowerItem.PowerItemTypes.ConsumerToggle;
        // Seems to be called when copying chunks into the world from prefabs
        // Which makes this ideal to add some randomness to POI stuff
        entityPoweredBlock.PresetDefaultValues(blockID);
        return (TileEntityPowered) entityPoweredBlock;
    }

}