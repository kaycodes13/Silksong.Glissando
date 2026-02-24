using HarmonyLib;
using DownSlashTypes = HeroControllerConfig.DownSlashTypes;

namespace Glissando.Patches;

[HarmonyPatch(typeof(HeroController))]
internal static class AttacksPatch {

	// See HeroFsmsPatch for the FSM-controlled attacks

	[HarmonyPatch(typeof(DamageEnemies), nameof(DamageEnemies.SetDirection))]
	[HarmonyPostfix]
	private static void FlipNonFsmAttackDirections(DamageEnemies __instance, ref float newDirection) {
		if (!GlissandoPlugin.GravityIsFlipped || !DamageEnemies.IsNailAttackObject(__instance.gameObject))
			return;

		int direction = DirectionUtils.GetCardinalDirection(newDirection);

		if (direction == DirectionUtils.Down || direction == DirectionUtils.Up)
			__instance.FlipDirection();
	}

	// The low priority is because I know Needleforge also patches the downspike methods
	// and I want at least SOME custom crests to play nice with this mod.

	[HarmonyPatch(nameof(HeroController.DownAttack))]
	[HarmonyPostfix]
	[HarmonyPriority(Priority.Last)]
	private static void FlipDownspikeStart(HeroController __instance) {
		if (__instance.Config.DownSlashType != DownSlashTypes.DownSpike)
			return;

		if (GlissandoPlugin.GravityIsFlipped && __instance.Config.DownspikeThrusts)
			GlissandoPlugin.FlipHeroVelocity();

		DamageEnemies damager = __instance.currentDownspike.EnemyDamager;
		int cardinalDir = DirectionUtils.GetCardinalDirection(damager.direction);
		if (
			(GlissandoPlugin.GravityIsFlipped && cardinalDir == DirectionUtils.Down)
			|| (!GlissandoPlugin.GravityIsFlipped && cardinalDir == DirectionUtils.Up)
		) {
			damager.FlipDirection();
		}
	}

	[HarmonyPatch(nameof(HeroController.Downspike))]
	[HarmonyPostfix]
	[HarmonyPriority(Priority.Last)]
	private static void FlipDownspikeMiddle(HeroController __instance) {
		if (GlissandoPlugin.GravityIsFlipped && __instance.Config.DownspikeThrusts)
			GlissandoPlugin.FlipHeroVelocity();
	}

	[HarmonyPatch(nameof(HeroController.FinishDownspike), [typeof(bool)])]
	[HarmonyPostfix]
	[HarmonyPriority(Priority.Last)]
	private static void FlipDownspikeEnd(HeroController __instance) {
		if (GlissandoPlugin.GravityIsFlipped && !__instance.cState.floating && !__instance.startWithBalloonBounce)
			GlissandoPlugin.FlipHeroVelocity();
	}

}
