using UnityEngine;

public static class CombatResolver
{
    /// <summary>
    /// Resolve a monster card against the player.
    /// </summary>
    /// <param name="player">The player's runtime state.</param>
    /// <param name="monster">The monster card being fought.</param>
    /// <param name="choice">Whether the player chose to fight with their weapon or barehanded.</param>
    public static void Resolve(PlayerState player, CardSO monster, FightChoice choice)
    {
        if (choice == FightChoice.WithWeapon)
        {
            // Weapon fights the monster. Player absorbs any overflow damage.
            int overflow = monster.Value - player.EquippedWeapon.Value;
            if (overflow > 0)
                player.TakeDamage(overflow);

            // Durability tightens to this monster's value regardless of outcome.
            // Pass the monster card so PlayerState can fire OnMonsterSlainWithWeapon.
            player.SetWeaponDurability(monster.Value, monster);
            Debug.Log("[CombatResolver] Weapon durability: " + monster.Value);
            
            AudioManager.Instance.PlaySFX("WeaponAttack");
        }
        else
        {
            // Barehanded (or durability gate blocked weapon use): full damage, weapon unequipped.
            player.TakeDamage(monster.Value);
            
            AudioManager.Instance.PlaySFX("BarehandAttack");
        }
    }

    /// <summary>
    /// Returns true if the player's equipped weapon can fight this monster.
    /// The only hard restriction is the durability gate: the monster must be strictly
    /// weaker than the last monster defeated with this weapon.
    /// Safe to call from the view layer (e.g. DragResolver) to decide whether to
    /// show the weapon drop zone.
    /// </summary>
    public static bool CanUseWeapon(PlayerState player, CardSO monster)
    {
        if (!player.HasWeapon) return false;

        // if the weapon has been used before, the monster must be
        // weaker than the last monster it defeated
        // durability == 0 means the weapon is fresh — any monster is valid.
        if (player.WeaponDurability > 0)
            return false;
        
        return true;
    }
}
