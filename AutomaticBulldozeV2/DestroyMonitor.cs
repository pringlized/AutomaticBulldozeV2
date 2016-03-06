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

        private int GetBuildingRefundAmount(ushort id, ref Building building)
        {
            if (_simulationManager.IsRecentBuildIndex(building.m_buildIndex))
                return building.Info.m_buildingAI.GetRefundAmount(id, ref building);
            return 0;
        }

        private void DispatchAutobulldozeEffect(BuildingInfo info, Vector3 pos, float angle, int length)
        {
            var effect = _buildingManager.m_properties.m_bulldozeEffect;
            if (effect == null)
                return;
            var instance = new InstanceID();
            var spawnArea = new EffectInfo.SpawnArea(Matrix4x4.TRS(Building.CalculateMeshPosition(info, pos, angle, length), Building.CalculateMeshRotation(angle), Vector3.one), info.m_lodMeshData);
            _effectManager.DispatchEffect(effect, instance, spawnArea, Vector3.zero, 0.0f, 1f, _nullAudioGroup);
        }

        private void DeleteBuildingImpl(ushort buildingId, ref Building building)
        {
            var info = building.Info;
            if (info.m_buildingAI.CheckBulldozing(buildingId, ref building) == ToolBase.ToolErrors.None)
            {
                var buildingRefundAmount = this.GetBuildingRefundAmount(buildingId, ref building);
                if (buildingRefundAmount != 0)
                    _economyManager.AddResource(EconomyManager.Resource.RefundAmount, buildingRefundAmount, info.m_class);
                var pos = building.m_position;
                var angle = building.m_angle;
                var length = building.Length;
                _buildingManager.ReleaseBuilding(buildingId);
                if (ItemClass.GetPublicServiceIndex(info.m_class.m_service) != -1)
                    _coverageManager.CoverageUpdated(info.m_class.m_service, info.m_class.m_subService, info.m_class.m_level);
                this.DispatchAutobulldozeEffect(info, pos, angle, length);
            }
        }
        
        public override void OnAfterSimulationTick()
        {
            var buffer = _buildingManager.m_buildings.m_buffer;
            for (var i = (ushort)(_simulationManager.m_currentTickIndex % 1000); i < _buildingManager.m_buildings.m_buffer.Length; i += 1000)
            {
                if (buffer[i].m_flags == Building.Flags.None)
                    continue;

                if (UIAutoBulldozerPanel.DemolishAbandoned && (buffer[i].m_flags & buffer[i].m_flags & Building.Flags.Abandoned) != Building.Flags.None
                    || UIAutoBulldozerPanel.DemolishBurned && (buffer[i].m_flags & buffer[i].m_flags & Building.Flags.BurnedDown) != Building.Flags.None)
                {
                    this.DeleteBuildingImpl(i, ref buffer[i]);
                }
            }
        }
    }
}
