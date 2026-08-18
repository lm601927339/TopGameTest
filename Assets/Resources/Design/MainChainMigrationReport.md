# Main Chain Migration Schema
originSource: legacy static table
targetSource: GameDesignCatalog asset
runtimeFallback: legacy mirror

| name | type | required | source | defaultValue | validation |
| --- | --- | --- | --- | --- | --- |
| skillId | string | yes | legacy static table |  | non-empty, unique, resolves to SkillData/SkillDefinition |
| timelineId | string | yes | legacy static table |  | non-empty, resolves to TimelineData/TimelineDefinition |
| bulletId | string[] | no | legacy static table | [] | optional; each non-empty id resolves to BulletData/BulletDefinition |
| aoeId | string[] | no | legacy static table | [] | optional; each non-empty id resolves to AoEData/AoeDefinition |
| buffId | string[] | no | legacy static table | [] | optional; each non-empty id resolves to BuffData/BuffDefinition |
| targetMode | SkillTargetMode | yes | legacy static table | Auto | must be a defined SkillTargetMode value |
| cost | DesignChaResource | yes | legacy static table | hp=0, ammo=0, stamina=0 | non-null; each resource component should be non-negative |
| cooldown | float | no | migration metadata; runtime cooldown stays on SkillObj | 0 | must be >= 0; default 0 |
| tags | string[] | no | migration metadata | [main-chain, legacy-static, catalog-ready] | unique, non-empty tokens recommended |
| notes | string | no | migration metadata |  | freeform summary of origin and dependency entrances |

# Main Chain Migration Checklist

## fire
- skillId: fire
- migrationPriority: P1
- source: legacy static table -> GameDesignCatalog asset
- timelineId: skill_fire
- bulletIds: bullet2d
- aoeIds: <none>
- buffIds: AutoCheckReload
- targetMode: Auto
- cost: hp=0, ammo=1, stamina=0
- cooldown: 0
- tags: main-chain, legacy-static, catalog-ready, projectile, buff, core-fire
- notes: legacy static table -> GameDesignCatalog asset; runtime fallback remains legacy mirror. cooldown stays runtime-only on SkillObj. SkillData.fire maps to TimelineData.skill_fire and BulletData.bullet2d; AutoCheckReload routes to skill_reload. bullets=bullet2d buffs=AutoCheckReload
- dependencyEntrances:
  - legacy static table/SkillData.fire
  - legacy static table/TimelineData.skill_fire
  - legacy static table/TimelineData.skill_fire -> BulletData.bullet2d
  - legacy static table/BuffData.AutoCheckReload
  - origin/runtime: BuffData.AutoCheckReload -> BuffScript.ReloadAmmo -> TimelineData.skill_reload

## fireMultiple
- skillId: fireMultiple
- migrationPriority: P1
- source: legacy static table -> GameDesignCatalog asset
- timelineId: skill_fireMultiple
- bulletIds: bullet2d
- aoeIds: <none>
- buffIds: AutoCheckReload
- targetMode: Auto
- cost: hp=0, ammo=1, stamina=0
- cooldown: 0
- tags: main-chain, legacy-static, catalog-ready, projectile, buff, core-fire
- notes: legacy static table -> GameDesignCatalog asset; runtime fallback remains legacy mirror. cooldown stays runtime-only on SkillObj. SkillData.fireMultiple maps to TimelineData.skill_fireMultiple and BulletData.bullet2d; AutoCheckReload routes to skill_reload. bullets=bullet2d buffs=AutoCheckReload
- dependencyEntrances:
  - legacy static table/SkillData.fireMultiple
  - legacy static table/TimelineData.skill_fireMultiple
  - legacy static table/TimelineData.skill_fireMultiple -> BulletData.bullet2d
  - legacy static table/BuffData.AutoCheckReload
  - origin/runtime: BuffData.AutoCheckReload -> BuffScript.ReloadAmmo -> TimelineData.skill_reload

## spaceMonkeyBall
- skillId: spaceMonkeyBall
- migrationPriority: P2
- source: legacy static table -> GameDesignCatalog asset
- timelineId: skill_spaceMonkeyBall
- bulletIds: <none>
- aoeIds: SpaceMonkeyBall
- buffIds: <none>
- targetMode: Auto
- cost: hp=0, ammo=3, stamina=0
- cooldown: 0
- tags: main-chain, legacy-static, catalog-ready, aoe, area-control
- notes: legacy static table -> GameDesignCatalog asset; runtime fallback remains legacy mirror. cooldown stays runtime-only on SkillObj. SkillData.spaceMonkeyBall maps to TimelineData.skill_spaceMonkeyBall and AoEData.SpaceMonkeyBall. aoes=SpaceMonkeyBall
- dependencyEntrances:
  - legacy static table/SkillData.spaceMonkeyBall
  - legacy static table/TimelineData.skill_spaceMonkeyBall
  - legacy static table/TimelineData.skill_spaceMonkeyBall -> AoEData.SpaceMonkeyBall

## grenade
- skillId: grenade
- migrationPriority: P2
- source: legacy static table -> GameDesignCatalog asset
- timelineId: skill_grenade
- bulletIds: boomball
- aoeIds: BoomExplosive, StayingBoom
- buffIds: <none>
- targetMode: Auto
- cost: hp=0, ammo=0, stamina=0
- cooldown: 0
- tags: main-chain, legacy-static, catalog-ready, projectile, aoe, area-control
- notes: legacy static table -> GameDesignCatalog asset; runtime fallback remains legacy mirror. cooldown stays runtime-only on SkillObj. SkillData.grenade maps to TimelineData.skill_grenade and BulletData.boomball, with nested AoEData.BoomExplosive and AoEData.StayingBoom from bullet params. bullets=boomball aoes=BoomExplosive, StayingBoom
- dependencyEntrances:
  - legacy static table/SkillData.grenade
  - legacy static table/TimelineData.skill_grenade
  - legacy static table/TimelineData.skill_grenade -> BulletData.boomball
  - legacy static table/BulletData.boomball -> AoEData.BoomExplosive
  - legacy static table/BulletData.boomball -> AoEData.StayingBoom

## explosiveBarrel
- skillId: explosiveBarrel
- migrationPriority: P3
- source: legacy static table -> GameDesignCatalog asset
- timelineId: skill_exploseBarrel
- bulletIds: <none>
- aoeIds: <none>
- buffIds: ExplosionBarrel
- targetMode: Auto
- cost: hp=0, ammo=0, stamina=0
- cooldown: 0
- tags: main-chain, legacy-static, catalog-ready, buff, summon
- notes: legacy static table -> GameDesignCatalog asset; runtime fallback remains legacy mirror. cooldown stays runtime-only on SkillObj. SkillData.explosiveBarrel maps to TimelineData.skill_exploseBarrel and BuffData.ExplosionBarrel. buffs=ExplosionBarrel
- dependencyEntrances:
  - legacy static table/SkillData.explosiveBarrel
  - legacy static table/TimelineData.skill_exploseBarrel
  - legacy static table/TimelineData.skill_exploseBarrel -> BuffData.ExplosionBarrel

## cloakBoomerang
- skillId: cloakBoomerang
- migrationPriority: P3
- source: legacy static table -> GameDesignCatalog asset
- timelineId: skill_cloakBoomerang
- bulletIds: cloakBoomerang
- aoeIds: <none>
- buffIds: <none>
- targetMode: Auto
- cost: hp=0, ammo=0, stamina=0
- cooldown: 0
- tags: main-chain, legacy-static, catalog-ready, projectile, boomerang
- notes: legacy static table -> GameDesignCatalog asset; runtime fallback remains legacy mirror. cooldown stays runtime-only on SkillObj. SkillData.cloakBoomerang maps to TimelineData.skill_cloakBoomerang and BulletData.cloakBoomerang. bullets=cloakBoomerang
- dependencyEntrances:
  - legacy static table/SkillData.cloakBoomerang
  - legacy static table/TimelineData.skill_cloakBoomerang
  - legacy static table/TimelineData.skill_cloakBoomerang -> BulletData.cloakBoomerang

## teleportBullet
- skillId: teleportBullet
- migrationPriority: P1
- source: legacy static table -> GameDesignCatalog asset
- timelineId: skill_teleportBullet_fire
- bulletIds: teleportBullet
- aoeIds: <none>
- buffIds: TeleportBulletPassive, TeleportTo
- targetMode: Auto
- cost: hp=0, ammo=0, stamina=0
- cooldown: 0
- tags: main-chain, legacy-static, catalog-ready, projectile, buff, core-fire, teleport
- notes: legacy static table -> GameDesignCatalog asset; runtime fallback remains legacy mirror. cooldown stays runtime-only on SkillObj. SkillData.teleportBullet maps to TimelineData.skill_teleportBullet_fire and BulletData.teleportBullet; TeleportBulletPassive triggers skill_teleportBullet_tele and TeleportTo in the runtime buff hook. bullets=teleportBullet buffs=TeleportBulletPassive, TeleportTo
- dependencyEntrances:
  - legacy static table/SkillData.teleportBullet
  - legacy static table/TimelineData.skill_teleportBullet_fire
  - legacy static table/TimelineData.skill_teleportBullet_fire -> BulletData.teleportBullet
  - legacy static table/BuffData.TeleportBulletPassive
  - origin/runtime: BuffData.TeleportBulletPassive -> BuffScript.FireTeleportBullet -> TimelineData.skill_teleportBullet_tele
  - origin/runtime: TimelineData.skill_teleportBullet_tele -> BuffData.TeleportTo
