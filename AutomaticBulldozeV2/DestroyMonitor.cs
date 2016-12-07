using AutomaticBulldozeV2.Extensions;
using AutomaticBulldozeV2.UI;
using ColossalFramework;
using ICities;

namespace AutomaticBulldozeV2
{
    public class DestroyMonitor : ThreadingExtensionBase
    {
        private readonly BuildingManager _buildingManager;
        private readonly SimulationManager _simulationManager;
        private readonly EconomyManager _economyManager;
        private readonly CoverageManager _coverageManager;
        private readonly AudioGroup _nullAudioGroup;

        public DestroyMonitor()
        {
            this._buildingManager = Singleton<BuildingManager>.instance;
            this._simulationManager = Singleton<SimulationManager>.instance;
            this._economyManager = Singleton<EconomyManager>.instance;
            this._coverageManager = Singleton<CoverageManager>.instance;
            this._nullAudioGroup = new AudioGroup(0, new SavedFloat("NOTEXISTINGELEMENT", Settings.gameSettingsFile, 0, false));
        }

        private void DeleteBuildingImpl(ref ushort buildingId, ref Building building)
        {
            if (building.Info.m_buildingAI.CheckBulldozing(buildingId, ref building) != ToolBase.ToolErrors.None)
                return;
            var buildingRefundAmount = building.GetRefundAmount(ref buildingId);
            if (buildingRefundAmount != 0)
                _economyManager.AddResource(EconomyManager.Resource.RefundAmount, buildingRefundAmount, building.Info.m_class);
            building.DispatchAutobulldozeEffect(ref buildingId, _nullAudioGroup);
            _buildingManager.ReleaseBuilding(buildingId);
            if (ItemClass.GetPublicServiceIndex(building.Info.m_class.m_service) != -1)
                _coverageManager.CoverageUpdated(building.Info.m_class.m_service, building.Info.m_class.m_subService, building.Info.m_class.m_level);
        }
        
        public override void OnAfterSimulationTick()
        {
            for (var i = (ushort)(_simulationManager.m_currentTickIndex % 1000); i < _buildingManager.m_buildings.m_buffer.Length; i+=1000)
            {
                if (_buildingManager.m_buildings.m_buffer[i].m_flags == Building.Flags.None)
                    continue;

                if ((UIAutoBulldozerPanel.DemolishAbandoned && (_buildingManager.m_buildings.m_buffer[i].m_flags & Building.Flags.Abandoned) != Building.Flags.None)
                    || (UIAutoBulldozerPanel.DemolishBurned && !_buildingManager.DisasterResponseBuildingExist() && (_buildingManager.m_buildings.m_buffer[i].m_flags & Building.Flags.BurnedDown) != Building.Flags.None))
                {
                    this.DeleteBuildingImpl(ref i, ref _buildingManager.m_buildings.m_buffer[i]);
                }
            }
        }
    }
}
