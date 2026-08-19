using UnityEngine;

namespace DesignerScripts
{
    public static class TimelineEventIds
    {
        public const string CasterPlayAnim = "CasterPlayAnim";
        public const string CasterForceMove = "CasterForceMove";
        public const string CasterForceBeatBack = "CasterForceBeatBack";
        public const string SetCasterControlState = "SetCasterControlState";
        public const string PlaySightEffectOnCaster = "PlaySightEffectOnCaster";
        public const string StopSightEffectOnCaster = "StopSightEffectOnCaster";
        public const string FireBullet = "FireBullet";
        public const string FireMultipleBullet = "FireMultipleBullet";
        public const string CasterImmune = "CasterImmune";
        public const string CreateAoE = "CreateAoE";
        public const string AddBuffToCaster = "AddBuffToCaster";
        public const string CasterAddAmmo = "CasterAddAmmo";
        public const string SummonCharacter = "SummonCharacter";
        public const string ApplyAbilityState = "ApplyAbilityState";
        public const string RemoveAbilityState = "RemoveAbilityState";
        public const string ControlNearestMonster = "ControlNearestMonster";
        public const string SpawnAbilityClone = "SpawnAbilityClone";
        public const string SummonAbilityLightning = "SummonAbilityLightning";
        public const string SetWeather = "SetWeather";
    }

    public static class TimelineEventRegistry
    {
        public static bool TryGet(string eventId, out TimelineEvent timelineEvent)
        {
            EnsureInitialized();
            if (string.IsNullOrEmpty(eventId))
            {
                timelineEvent = null;
                return false;
            }

            return TimelineScript.functions.TryGetValue(eventId, out timelineEvent);
        }

        public static TimelineEvent Require(string eventId)
        {
            if (TryGet(eventId, out TimelineEvent timelineEvent))
            {
                return timelineEvent;
            }

            Debug.LogError("[TimelineEventRegistry] Unknown timeline event: " + eventId);
            return Noop;
        }

        public static void EnsureInitialized()
        {
            TimelineScript.EnsureInitialized();
        }

        private static void Noop(TimelineObj timeline, params object[] args)
        {
        }
    }
}
