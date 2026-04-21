using System;
using UnityEngine;

/// <summary>
/// Runtime state of the player: health, max health, and equipped weapon.
/// Pure C# — no MonoBehaviour. The presentation layer listens to events here.
/// </summary>
public class PlayerState
{
    // ── Config ───────────────────────────────────────────────────────

    public int MaxHealth { get; }

    // ── State ────────────────────────────────────────────────────────

    private int currentHealth;
    private CardSO equippedWeapon;
    private int weaponDurability; 

    // ── Events ───────────────────────────────────────────────────────

    public event Action<int, int> OnHealthChanged;  
    public event Action<CardSO> OnWeaponChanged;
    /// <summary>
    /// Fired when a monster is defeated using the equipped weapon.
    /// Carries the slain monster's CardSO so the view can display it next to the weapon.
    /// </summary>
    public event Action<CardSO> OnMonsterSlainWithWeapon;
    public event Action OnDeath;

    // ── Public API ───────────────────────────────────────────────────

    public int CurrentHealth => currentHealth;
    public CardSO EquippedWeapon => equippedWeapon;
    public int WeaponDurability => weaponDurability;
    public bool IsDead => currentHealth <= 0;
    public bool HasWeapon => equippedWeapon != null;

    public PlayerState(int maxHealth = 20)
    {
        MaxHealth     = maxHealth;
        currentHealth = maxHealth;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Math.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, -amount);
        if (currentHealth == 0) OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (amount <= 0) return;
        currentHealth = Math.Min(MaxHealth, currentHealth + amount);
        AudioManager.Instance.PlaySFX("Heal");
        OnHealthChanged?.Invoke(currentHealth, amount);
    }

    /// <summary>
    /// Equip a weapon card. Replaces any previously equipped weapon.
    /// Durability resets to 0 (no monster defeated yet with this weapon).
    /// </summary>
    public void EquipWeapon(CardSO weaponCard)
    {
        equippedWeapon = weaponCard;
        weaponDurability = 15;
        AudioManager.Instance.PlaySFX("Equip");
        OnWeaponChanged?.Invoke(weaponCard);
    }

    /// <summary>
    /// Called by CombatResolver after using a weapon to defeat a monster.
    /// Records the defeated monster's value as durability — the weapon can
    /// only fight monsters weaker than this from now on.
    /// Also fires OnMonsterSlainWithWeapon so the view can display the slain card next to the weapon.
    /// </summary>
    public void SetWeaponDurability(int defeatedMonsterValue, CardSO slainMonster = null)
    {
        weaponDurability = defeatedMonsterValue;
        if (slainMonster != null)
            OnMonsterSlainWithWeapon?.Invoke(slainMonster);
    }

    public void UnequipWeapon()
    {
        equippedWeapon   = null;
        weaponDurability = 0;
        OnWeaponChanged?.Invoke(null);
    }
}

