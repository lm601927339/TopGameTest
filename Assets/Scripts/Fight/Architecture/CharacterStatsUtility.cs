using UnityEngine;

public static class CharacterStatsUtility
{
    public static ChaProperty BuildFinalProperty(ChaProperty baseProp, ChaProperty equipmentProp, ChaProperty[] buffProp)
    {
        ChaProperty plus = buffProp != null && buffProp.Length > 0 ? buffProp[0] : ChaProperty.zero;
        ChaProperty times = buffProp != null && buffProp.Length > 1 ? buffProp[1] : ChaProperty.zero;
        return (baseProp + equipmentProp + plus) * times;
    }

    public static ChaResource ClampResource(ChaResource resource, ChaProperty property)
    {
        resource.hp = Mathf.Clamp(resource.hp, 0, property.hp);
        resource.ammo = Mathf.Clamp(resource.ammo, 0, property.ammo);
        resource.stamina = Mathf.Clamp(resource.stamina, 0, 100);
        return resource;
    }

    public static bool CanBeKilledByDamageInfo(ChaResource resource, float immuneTime, DamageInfo damageInfo)
    {
        if (immuneTime > 0 || damageInfo.isHeal())
        {
            return false;
        }

        int dValue = damageInfo.DamageValue(false);
        return dValue >= resource.hp;
    }
}
