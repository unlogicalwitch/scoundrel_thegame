using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private ObjectPool objectPool;
    private readonly List<SoundEmitter> activeEmitters = new();
}
