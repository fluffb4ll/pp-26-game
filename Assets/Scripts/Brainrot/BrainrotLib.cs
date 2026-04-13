using System;
using System.Collections.Generic;
using UnityEngine;

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public enum BrainrotType
{
    SkibidiToilet,
    Sigma,
    TTTSahur
}

/// <summary>
/// Конфигурация редкости - соотносим редкость с множителями времени жизни и выработки  
/// </summary>
[Serializable]
public struct RarityConfig
{
    public Rarity rarity;
    public float lifetimeMult;
    public float produceMult;
}

/// <summary>
/// Позволяет добавлять объявления брейнротов внутри инспектора - просто и безболезненно (надеюсь) :)
/// </summary>
[CreateAssetMenu(fileName = "BrainrotLib", menuName = "Scriptable Objects/BrainrotLib")]
public class BrainrotLib : ScriptableObject
{
    public BrainrotType type;
    public float baseProduce;
    public float baseLifetime;
    
    public List<RarityConfig> rarityPool;
}
