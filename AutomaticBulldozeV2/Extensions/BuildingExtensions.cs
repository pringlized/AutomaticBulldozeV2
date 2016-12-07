using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace AutomaticBulldozeV2.Extensions
{
    internal static class BuildingExtensions
    {
        internal static void DispatchAutobulldozeEffect(this Building building, ref ushort buildingId, AudioGroup nullAudioGroup)
        {
            var effect = Singleton<BuildingManager>.instance.m_properties.m_bulldozeEffect;
            if (effect == null)
                return;
            var instance = new InstanceID();
            var matrix = Matrix4x4.TRS(
                    Building.CalculateMeshPosition(building.Info, building.m_position, building.m_angle, building.Length),
                    Building.CalculateMeshRotation(building.m_angle), Vector3.one);
            EffectInfo.SpawnArea spawnArea;
            var collapsed = (building.m_flags & Building.Flags.Collapsed) != Building.Flags.None;
            if (collapsed)
            {
                var collapsedInfo = (BuildingInfoBase)building.Info.m_collapsedInfo;
                if (!(collapsedInfo != null))
                    return;
                var num = new Randomizer(buildingId).Int32(4U);
                if ((1 << num & building.Info.m_collapsedRotations) == 0)
                    num = num + 1 & 3;
                var x1 = building.Width * 4f;
                var z1 = building.Length * 4f;
                var localMeshOffset = Building.CalculateLocalMeshOffset(building.Info, building.Length);
                var vector31 = Vector3.Max(building.Info.m_generatedInfo.m_min - new Vector3(4f, 0.0f, 4f + localMeshOffset), new Vector3(-x1, 0.0f, -z1));
                var vector32 = Vector3.Min(building.Info.m_generatedInfo.m_max + new Vector3(4f, 0.0f, 4f - localMeshOffset), new Vector3(x1, 0.0f, z1));
                var vector33 = (vector31 + vector32) * 0.5f;
                var vector34 = vector32 - vector31;
                var x2 = ((num & 1) != 0 ? vector34.z : vector34.x) / Mathf.Max(1f, collapsedInfo.m_generatedInfo.m_size.x);
                var z2 = ((num & 1) != 0 ? vector34.x : vector34.z) / Mathf.Max(1f, collapsedInfo.m_generatedInfo.m_size.z);
                var q = Quaternion.AngleAxis(num * 90f, Vector3.down);
                var matrix4X4 = Matrix4x4.TRS(new Vector3(vector33.x, 0.0f, vector33.z + localMeshOffset), q, new Vector3(x2, 1f, z2));

                spawnArea = new EffectInfo.SpawnArea(matrix * matrix4X4, collapsedInfo.m_lodMeshData);
            }
            else
            {
                spawnArea = new EffectInfo.SpawnArea(matrix, building.Info.m_lodMeshData);
            }
            Singleton<EffectManager>.instance.DispatchEffect(effect, instance, spawnArea, Vector3.zero, 0.0f, 1f, nullAudioGroup);
        }

        internal static int GetRefundAmount(this Building building, ref ushort id)
        {
            return Singleton<SimulationManager>.instance.IsRecentBuildIndex(building.m_buildIndex)
                ? building.Info.m_buildingAI.GetRefundAmount(id, ref building)
                : 0;
        }
    }
}
