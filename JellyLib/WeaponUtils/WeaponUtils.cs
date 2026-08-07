using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Lua.Proxy;
using HarmonyLib;
using JellyLib.Utilities;
using UnityEngine;

namespace JellyLib.WeaponUtils
{
    public static class WeaponUtils
    {
        public static bool IsThrowableWeapon(Weapon weapon)
        {
            return weapon is ThrowableWeapon;
        }

        public static bool IsMountedWeapon(Weapon weapon)
        {
            return weapon is MountedWeapon;
        }

        //Returns null if weapon passed is not a MountedWeapon
        public static MountedWeaponProxy AsMountedWeapon(Weapon weapon)
        {
            var mountedWeapon = weapon as MountedWeapon;
            return !mountedWeapon ? null : new MountedWeaponProxy(mountedWeapon);
        }

        private static WeaponOverrideManager _overrideManager; 
        public static WeaponOverrideManager OverrideManager => _overrideManager ??= new WeaponOverrideManager();

        private static WeaponCustomDataManager _customDataManager; 
        public static WeaponCustomDataManager CustomDataManager => _customDataManager ??= new WeaponCustomDataManager();

        private static Dictionary<ulong, Dictionary<string, WeaponManager.WeaponEntry>> _weaponsByModId;
        private static Dictionary<ulong, string> _modNamesById = new();
        private static Dictionary<WeaponManager.WeaponEntry, Projectile.Configuration> _defaultProjectileConfigs = new();
        private static Dictionary<WeaponManager.WeaponEntry, ExplodingProjectile.ExplosionConfiguration> _defaultExplodingProjectileConfigs = new();

        private static bool _doneLoading;
        public static bool  DoneLoading => _doneLoading;

        /// <summary>
        /// Runs through all weapon entries to sort them by ID and cache projectile configurations.
        /// </summary>
        public static void ProcessWeaponEntries()
        {
            if (_weaponsByModId == null)
                _weaponsByModId = new();
            else
                return;
            
            Stopwatch stopwatch = Stopwatch.StartNew();
            foreach(var weaponEntry in WeaponManager.instance.allWeapons)
            {
                //Cache weapon entry's projectile configs so we have these as a reference later on.
                CacheProjectileConfig(weaponEntry);
                
                var modId = weaponEntry.sourceMod.workshopItemId.m_PublishedFileId;
                var weaponEntryName = weaponEntry.name.Trim();
                if (_weaponsByModId.TryGetValue(modId, out var weaponSet))
                {
                    if (weaponSet.ContainsKey(weaponEntryName))
                    {
                        var alternateName = $"{weaponEntryName}({weaponEntry.slot})";
                        weaponSet.Add(alternateName, weaponEntry);
                        Plugin.Logger.LogWarning($"[{nameof(WeaponUtils)}.{nameof(ProcessWeaponEntries)}] Weapon Entry with name {weaponEntryName} in group {modId} already found. Registering with alternate name {alternateName} instead.");
                        continue;
                    }
                    weaponSet.Add(weaponEntryName, weaponEntry);
                }
                else
                {
                    var newSet = new Dictionary<string, WeaponManager.WeaponEntry>(StringComparer.InvariantCultureIgnoreCase)
                    {
                        [weaponEntryName] = weaponEntry
                    };
                    _weaponsByModId.Add(modId, newSet);
                    _modNamesById.Add(modId, weaponEntry.sourceMod.title);
                }
                
                //Plugin.Logger.LogInfo($"[{nameof(WeaponUtils)}.{nameof(SortWeaponEntriesByModId)}] Registered: {weaponEntry.name} to group {modId}");
            }
            stopwatch.Stop();
            Plugin.Logger.LogInfo($"[{nameof(WeaponUtils)}.{nameof(ProcessWeaponEntries)}] Processed {WeaponManager.instance.allWeapons.Count} weapons. Operation took {stopwatch.ElapsedMilliseconds}ms.");
            _doneLoading = true;
        }
        
        /// <summary>
        /// Cache the given weapon entry's original projectile configs.
        /// </summary>
        /// <param name="weaponEntry"></param>
        private static void CacheProjectileConfig(WeaponManager.WeaponEntry weaponEntry)
        {
            var weapon = weaponEntry.prefab?.GetComponent<Weapon>();
            if (!weapon)
                return;

            var projectile = weapon.configuration.projectilePrefab?.GetComponent<Projectile>();
            if (projectile == null)
                return;
            
            if (_defaultProjectileConfigs.ContainsKey(weaponEntry))
                return;
            
            _defaultProjectileConfigs[weaponEntry] = projectile.configuration;

            if (projectile is not ExplodingProjectile explodingProjectile)
                return;
            
            if (_defaultExplodingProjectileConfigs.ContainsKey(weaponEntry))
                return;
            
            _defaultExplodingProjectileConfigs[weaponEntry] = explodingProjectile.explosionConfiguration;
        }

        public static WeaponManager.WeaponEntry GetWeaponEntry(string weaponEntryName, ulong modId)
        {
            var key = _weaponsByModId.ContainsKey(modId) ? modId : 0;
            if (!_weaponsByModId.TryGetValue(key, out var weaponSet))
                return null;
            
            var trimmedString = weaponEntryName.Trim();
            return !weaponSet.TryGetValue(trimmedString, out var weaponEntry) ? null : weaponEntry;
        }

        /// <summary>
        /// Returns the weapons projectile damage. If an override exists for the weapon, will use that value instead.
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public static float GetWeaponDamage(Weapon weapon)
        {
            if (weapon == null)
                return 0;

            var projectilePrefab = weapon.configuration.projectilePrefab;

            if (projectilePrefab == null)
                return 0;
            
            var projectile = projectilePrefab.GetComponent<Projectile>();
            if (projectile == null)
                return 0;

            _overrideManager.GetWeaponOverride(weapon.weaponEntry, out var weaponOverride);

            return weaponOverride.damage ?? projectile.configuration.damage;
        }

        /// <summary>
        /// Returns the weapons explosion damage. If an override exists for the weapon, will use that value instead.
        /// </summary>
        /// <param name="weapon"></param>
        /// <returns></returns>
        public static float GetWeaponExplosionDamage(Weapon weapon)
        {
            if (weapon == null)
                return 0;

            var projectilePrefab = weapon.configuration.projectilePrefab;

            if (projectilePrefab == null)
                return 0;
            
            var projectile = projectilePrefab.GetComponent<Projectile>();
            if (projectile == null)
                return 0;

            if (_overrideManager.GetWeaponOverride(weapon.weaponEntry, out var weaponOverride) && weaponOverride.explosionDamage.HasValue)
                return weaponOverride.explosionDamage.Value;

            return projectile switch
            {
                ExplodingProjectile explodingProjectile => explodingProjectile.explosionConfiguration.damage,
                GrenadeProjectile grenadeProjectile => grenadeProjectile.explosionConfiguration.damage,
                _ => 0
            };
        }

        public static bool TryGetCachedProjectileConfig(WeaponManager.WeaponEntry weaponEntry, out Projectile.Configuration configuration)
        {
            if (weaponEntry != null) return _defaultProjectileConfigs.TryGetValue(weaponEntry, out configuration);
            
            configuration = null;
            return false;
        }

        public static bool TryGetCachedExplodingProjectileConfig(WeaponManager.WeaponEntry weaponEntry, out ExplodingProjectile.ExplosionConfiguration configuration)
        {
            if (weaponEntry != null) return _defaultExplodingProjectileConfigs.TryGetValue(weaponEntry, out configuration);
            
            configuration = null;
            return false;
        }

        public static void DumpWeaponNames()
        {
            Directory.CreateDirectory($@"{Plugin.filePath}\dumps\");
            
            var dump = "";
            foreach (var kvp in _weaponsByModId)
            { 
                if (kvp.Key == 0)
                    dump += "Vanilla or RFToolsExport\n";
                else
                    dump += $"Mod {_modNamesById[kvp.Key]} (ID: {kvp.Key}): \n";
                
                foreach (var weaponName in kvp.Value.Keys)
                {
                    dump += "-" + weaponName + "\n";
                    var weaponEntry = kvp.Value[weaponName];
                    dump += "--Tags: \n";
                    foreach (var tag in weaponEntry.tags) 
                        dump += $"----{tag}\n";
                    if (!weaponEntry.prefab)
                        continue;
                    var weapon = weaponEntry.prefab.GetComponent<Weapon>();
                    if (!weapon)
                        continue;
                    dump += "--Stats: \n";
                    dump += $"----Max Ammo: {weapon.configuration.ammo}\n";
                    dump += $"----Max Spare Ammo: {weapon.configuration.spareAmmo}\n";
                    dump += $"----Kickback: {weapon.configuration.kickback}\n";
                    dump += $"----Random Kick: {weapon.configuration.randomKick}\n";
                    dump += $"----Snap Magnitude: {weapon.configuration.snapMagnitude}\n";
                    dump += $"----Snap Duration: {weapon.configuration.snapDuration}\n";
                    dump += $"----Snap Frequency: {weapon.configuration.snapFrequency}\n";
                    dump += $"----Cooldown: {weapon.configuration.cooldown}\n";
                    dump += $"----Spread: {weapon.configuration.spread}\n";
                    dump += $"----Follow Up Spread Gain: {weapon.configuration.followupSpreadGain}\n";
                    dump += $"----Follow Up Max Spread Hip: {weapon.configuration.followupMaxSpreadHip}\n";
                    dump += $"----Follow Up Max Spread Aim: {weapon.configuration.followupMaxSpreadAim}\n";
                    dump += $"----Followup Spread Stay Time: {weapon.configuration.followupSpreadStayTime}\n";
                    dump += $"----Followup Spread Dissipate Time: {weapon.configuration.followupSpreadDissipateTime}\n";
                    dump += $"----Spread Prone Multiplier: {weapon.configuration.spreadProneMultiplier}\n";
                    dump += $"----Follow Up Spread Prone Multiplier: {weapon.configuration.followupSpreadProneMultiplier}\n";
                    var projectilePrefab = weapon.configuration.projectilePrefab;
                    if (!projectilePrefab)
                        continue;
                    var projectile = projectilePrefab.GetComponent<Projectile>();
                    if (!projectile)
                        continue;
                    dump += $"----Health Damage: {projectile.configuration.damage}\n";
                    dump += $"----Balance Damage: {projectile.configuration.balanceDamage}\n";
                    dump += $"----Projectile Speed: {projectile.configuration.speed}\n";
                    dump += $"----Projectile Drop-Off End: {projectile.configuration.dropoffEnd}\n";
                }
                File.WriteAllText($@"{Plugin.filePath}\dumps\{kvp.Key}.txt", dump);
                dump = "";
            }
            
        }
    }
    
    [HarmonyPatch(typeof(Actor), "SpawnWeapon")]
    public class PatchSpawnWeapon
    {
        static void Postfix(Weapon __result)
        {
            if (__result == null) return;
            
            WeaponOverrideManager.ApplyOverrides(__result);
        }
    }
    
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.ReturnToMenu))]
    public class PatchReturnToMenu
    {
        static bool Prefix(GameManager __instance)
        {
            WeaponUtils.OverrideManager.Clear();
            WeaponUtils.CustomDataManager.Clear();
            Plugin.Logger.LogInfo($"{nameof(WeaponUtils)}.{nameof(GameManager.ReturnToMenu)}.Prefix: Cleared weapon overrides.");
            return true;
        }
    }

    [HarmonyPatch(typeof(GameManager), nameof(GameManager.ReturnToCampaignLobby))]
    public class PatchReturnToCampaignLobby
    {
        static bool Prefix(GameManager __instance)
        {
            WeaponUtils.OverrideManager.Clear();
            WeaponUtils.CustomDataManager.Clear();
            Plugin.Logger.LogInfo($"{nameof(WeaponUtils)}.{nameof(GameManager.ReturnToMenu)}.Prefix: Cleared weapon overrides.");
            return true;
        }
    }
    
    [HarmonyPatch(typeof(GameManager), nameof(GameManager.RestartLevel))]
    public class PatchRestartLevel
    {
        static bool Prefix(GameManager __instance)
        {
            WeaponUtils.OverrideManager.Clear();
            WeaponUtils.CustomDataManager.Clear();
            Plugin.Logger.LogInfo($"{nameof(WeaponUtils)}.{nameof(GameManager.RestartLevel)}.Prefix: Cleared weapon overrides.");
            return true;
        }
    }
    
    [HarmonyPatch(typeof(ModManager), "FinalizeLoadedModContent")]
    public class PatchFinalizeLoadedModContent
    {
        static void Postfix()
        {
            WeaponUtils.ProcessWeaponEntries();
        }
    }
    
    [HarmonyPatch(typeof(Weapon), "OnDestroy")]
    public class PatchWeaponOnDestroy
    {
        static void Prefix(Weapon __instance)
        {
            WeaponUtils.OverrideManager.RemoveWeaponInstanceOverride(__instance);
            WeaponUtils.CustomDataManager.ClearWeaponData(__instance);
        }
    }

    [HarmonyPatch(typeof(Weapon), "SpawnProjectile")]
    public class PatchProjectileAwake
    {
        static void Postfix(Weapon __instance, Projectile __result)
        {
            if (__instance == null) return;
            if (__result == null) return;
            if (__result.sourceWeapon == null) return;
            if (__result.sourceWeapon != __instance) return;
            
            var hasGlobalOverride = WeaponUtils.OverrideManager.GetWeaponOverride(__result.sourceWeapon.weaponEntry, out var globalOverride);
            var hasInstanceOverride = WeaponUtils.OverrideManager.GetInstanceOverride(__result.sourceWeapon, out var instanceOverride);
            if(!hasGlobalOverride && !hasInstanceOverride) return;

            var projectileOverride = new ProjectileOverride
            {
                damage = instanceOverride.damage ?? globalOverride.damage,
                balanceDamage = instanceOverride.balanceDamage ?? globalOverride.balanceDamage
            };

            if (__result is ExplodingProjectile or GrenadeProjectile)
            {
                projectileOverride.explosionDamage = instanceOverride.explosionDamage ?? globalOverride.explosionDamage;
                projectileOverride.explosionBalanceDamage = instanceOverride.explosionBalanceDamage ?? globalOverride.explosionBalanceDamage;
            }
            
            WeaponUtils.OverrideManager.SetProjectileOverride(__result, projectileOverride);
        }
    }
    
    /// <summary>
    /// Patch damage calculation instead of editing the actual projectile instance.
    /// </summary>
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.Damage))]
    public class PatchProjectileDamage
    {
        static bool Prefix(Projectile __instance, ref float __result)
        {
            if (__instance == null) return false;

            if (!WeaponUtils.OverrideManager.TryGetProjectileOverride(__instance, out var projectileOverride))
                return true;

            if (!projectileOverride.damage.HasValue)
                return true;
            
            var dropOff = __instance.configuration.damageDropOff.Evaluate(__instance.travelDistance / __instance.configuration.dropoffEnd);
            __result = dropOff * projectileOverride.damage.Value;
            
            return false;
        }
    }

    /// <summary>
    /// Patch balance damage calculation instead of editing the actual projectile instance.
    /// </summary>
    [HarmonyPatch(typeof(Projectile), nameof(Projectile.BalanceDamage))]
    public class PatchProjectileBalanceDamage
    {
        static bool Prefix(Projectile __instance, ref float __result)
        {
            if (__instance == null) return false;
            
            if (!WeaponUtils.OverrideManager.TryGetProjectileOverride(__instance, out var projectileOverride))
                return true;

            if (!projectileOverride.balanceDamage.HasValue)
                return true;
            
            var dropOff = __instance.configuration.damageDropOff.Evaluate(__instance.travelDistance / __instance.configuration.dropoffEnd);
            __result = dropOff * projectileOverride.balanceDamage.Value;
            
            return false;
        }
    }

    [HarmonyPatch(typeof(ExplodingProjectile), "Explode")]
    public class PatchProjectileExplode
    {
        private static readonly AccessTools.FieldRef<ExplodingProjectile, AudioSource> GetAudioSource = AccessTools.FieldRefAccess<ExplodingProjectile, AudioSource>("audioSource");
        
        static bool Prefix(ExplodingProjectile __instance, Vector3 position, Vector3 up, ref bool __result)
        {
            if (!WeaponUtils.OverrideManager.TryGetProjectileOverride(__instance, out var projectileOverride))
                return true;
            
            if (!projectileOverride.explosionDamage.HasValue && !projectileOverride.explosionBalanceDamage.HasValue)
                return true;
            
            var reduceFriendlyDamage = __instance.firedByAI && __instance.travelDistance < 5.0;
            
            //Create a cloned instance with overridden values instead of directly editing the exploding projectile instance.
            var overridenExplosionConfig = new ExplodingProjectile.ExplosionConfiguration()
            {
                damage = projectileOverride.explosionDamage ?? __instance.configuration.damage,
                balanceDamage = projectileOverride.explosionBalanceDamage ?? __instance.configuration.balanceDamage,
                infantryDamageMultiplier = __instance.explosionConfiguration.infantryDamageMultiplier,
                damageRange = __instance.explosionConfiguration.damageRange,
                damageFalloff = __instance.explosionConfiguration.damageFalloff,
                balanceRange = __instance.explosionConfiguration.balanceRange,
                balanceFalloff = __instance.explosionConfiguration.balanceFalloff,
                force = __instance.explosionConfiguration.force
            };
            
            var flag = ActorManager.Explode(__instance.killCredit, __instance.sourceWeapon, position, overridenExplosionConfig, __instance.armorDamage, reduceFriendlyDamage);
            
            __instance.transform.rotation = Quaternion.LookRotation(up);
            __instance.enabled = false;
            
            if (__instance.renderers != null)
            {
                foreach (var renderer in __instance.renderers)
                {
                    if(renderer == null) continue;
                    renderer.enabled = false;
                }
            }
            
            if (__instance.impactParticles != null)
                __instance.impactParticles.Play();
            if (__instance.trailParticles != null)
                __instance.trailParticles.Stop();

            var audioSource = GetAudioSource(__instance);
            
            if (audioSource != null)
            {
                Vector3 vector3 = position + up * 0.5f;
                GameManager.UpdateSoundOutputGroupCombat(audioSource, Vector3.Distance(vector3, GameManager.GetPlayerCameraPosition()), !Physics.Linecast(vector3, GameManager.GetPlayerCameraPosition(), 8392705));
                audioSource.pitch *= UnityEngine.Random.Range(0.9f, 1.1f);
                audioSource.Play();
            }
            if (__instance.activateOnExplosion != null)
            {
                __instance.activateOnExplosion.SetActive(true);
                if (__instance.deactivateAgainTime > 0.0)
                    __instance.Invoke("Deactivate", __instance.deactivateAgainTime);
            }
            __instance.Invoke("StopSmoke", __instance.smokeTime);

            __result = flag;
            return false;
        }
    }

    [HarmonyPatch(typeof(GrenadeProjectile), "Explode")]
    public class PatchGrenadeExplode
    {
        static bool Prefix(GrenadeProjectile __instance)
        {
            if (!WeaponUtils.OverrideManager.TryGetProjectileOverride(__instance, out var projectileOverride))
                return true;
            
            if (!projectileOverride.explosionDamage.HasValue && !projectileOverride.explosionBalanceDamage.HasValue)
                return true;
            
            var overridenExplosionConfig = new ExplodingProjectile.ExplosionConfiguration()
            {
                damage = projectileOverride.explosionDamage ?? __instance.configuration.damage,
                balanceDamage = projectileOverride.explosionBalanceDamage ?? __instance.configuration.balanceDamage,
                infantryDamageMultiplier = __instance.explosionConfiguration.infantryDamageMultiplier,
                damageRange = __instance.explosionConfiguration.damageRange,
                damageFalloff = __instance.explosionConfiguration.damageFalloff,
                balanceRange = __instance.explosionConfiguration.balanceRange,
                balanceFalloff = __instance.explosionConfiguration.balanceFalloff,
                force = __instance.explosionConfiguration.force
            };
            
            Plugin.Logger.LogInfo("Explosion range: " + overridenExplosionConfig.damageRange);
            
            ActorManager.Explode(__instance.killCredit, __instance.sourceWeapon, __instance.transform.position, overridenExplosionConfig, __instance.armorDamage, false);
            __instance.transform.rotation = Quaternion.LookRotation(Vector3.up);
            RaycastHit hitInfo;
            if (Physics.Raycast(new Ray(__instance.transform.position, Vector3.down), out hitInfo, 1f, 1))
                DecalManager.AddDecal(hitInfo.point, hitInfo.normal, UnityEngine.Random.Range(1f, 2f), DecalManager.DecalType.Impact);
            __instance.enabled = false;
            foreach (Renderer renderer in __instance.renderers)
                renderer.enabled = false;
            
            __instance.explosionParticles.Play(true);
            
            AudioSource component = __instance.GetComponent<AudioSource>();
            if (component != null)
            {
                Vector3 position = __instance.transform.position;
                position.y += 0.5f;
                GameManager.UpdateSoundOutputGroupCombat(component, Vector3.Distance(position, GameManager.GetPlayerCameraPosition()), !Physics.Linecast(position, GameManager.GetPlayerCameraPosition(), 8392705));
                component.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
                component.Play();
            }
            if (__instance.activateOnExplosion != null)
            {
                __instance.activateOnExplosion.SetActive(true);
                if (__instance.deactivateAgainTime > 0.0)
                    __instance.Invoke("Deactivate", __instance.deactivateAgainTime);
            }
            
            __instance.Invoke("Cleanup", __instance.cleanupTime);

            return false;
        }
    }

    [HarmonyPatch(typeof(ProjectilePool), nameof(ProjectilePool.ReturnToPool))]
    public class PatchProjectileReturnToPool
    {
        static void Postfix(Projectile projectile)
        {
            if(projectile == null) return;
            
            WeaponUtils.OverrideManager.RemoveProjectileOverride(projectile);
        }
    }
}

