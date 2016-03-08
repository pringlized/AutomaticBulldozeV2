using AutomaticBulldozeV2.UI;
using ColossalFramework;
using ICities;
using UnityEngine;

namespace AutomaticBulldozeV2
{
    public class DestroyMonitor : ThreadingExtensionBase
    {
        private readonly BuildingManager _buildingManager;
        private readonly SimulationManager _simulationManager;
        private readonly EffectManager _effectManager;
        private readonly EconomyManager _economyManager;
        private readonly CoverageManager _coverageManager;
        private readonly AudioGroup _nullAudioGroup;

        public DestroyMonitor()
        {
            _buildingManager = Singleton<BuildingManager>.instance;
            _simulationManager = Singleton<SimulationManager>.instance;
            _effectManager = Singleton<EffectManager>.instance;
            _economyManager = Singleton<EconomyManager>.instance;
            _coverageManager = Singleton<CoverageManager>.instance;
            _nullAudioGroup = new AudioGroup(0, new SavedFloat("NOTEXISTINGELEMENT", Settings.gameSettingsFile, 0, false));
        }

        private int GetBuildingRefundAmount(ref ushort id, ref Building building)
        {
            return _simulationManager.IsRecentBuildIndex(building.m_buildIndex) 
                ? building.Info.m_buildingAI.GetRefundAmount(id, ref building) 
                : 0;
        }

        private void DispatchAutobulldozeEffect(BuildingInfo info, ref Vector3 pos, ref float angle, int length)
        {
            var effect = _buildingManager.m_properties.m_bulldozeEffect;
            if (effect == null)
                return;
            var instance = new InstanceID();
            var spawnArea = new EffectInfo.SpawnArea(Matrix4x4.TRS(Building.CalculateMeshPosition(info, pos, angle, length), Building.CalculateMeshRotation(angle), Vector3.one), info.m_lodMeshData);
            _effectManager.DispatchEffect(effect, instance, spawnArea, Vector3.zero, 0.0f, 1f, _nullAudioGroup);
        }

        private void DeleteBuildingImpl(ref ushort buildingId, ref Building building)
        {
            var info = building.Info;
            if (info.m_buildingAI.CheckBulldozing(buildingId, ref building) != ToolBase.ToolErrors.None)
                return;
            var buildingRefundAmount = this.GetBuildingRefundAmount(ref buildingId, ref building);
            if (buildingRefundAmount != 0)
                _economyManager.AddResource(EconomyManager.Resource.RefundAmount, buildingRefundAmount, info.m_class);
            this.DispatchAutobulldozeEffect(info, ref building.m_position, ref building.m_angle, building.Length);
            _buildingManager.ReleaseBuilding(buildingId);
            if (ItemClass.GetPublicServiceIndex(info.m_class.m_service) != -1)
                _coverageManager.CoverageUpdated(info.m_class.m_service, info.m_class.m_subService, info.m_class.m_level);
        }
        
        public override void OnAfterSimulationTick()
        {
            for (var i = (ushort)(_simulationManager.m_currentTickIndex % 1000); i < _buildingManager.m_buildings.m_buffer.Length; i += 1000)
            {
                if (_buildingManager.m_buildings.m_buffer[i].m_flags == Building.Flags.None)
                    continue;

                if (UIAutoBulldozerPanel.DemolishAbandoned && (_buildingManager.m_buildings.m_buffer[i].m_flags & Building.Flags.Abandoned) != Building.Flags.None
                    || UIAutoBulldozerPanel.DemolishBurned && (_buildingManager.m_buildings.m_buffer[i].m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
                {
                    this.DeleteBuildingImpl(ref i, ref _buildingManager.m_buildings.m_buffer[i]);
                }
            }
        }
    }
}
