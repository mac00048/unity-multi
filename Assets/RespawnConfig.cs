using UnityEngine;

// Конфигурация возрождения. Настраивается из Editor,
// без необходимости лезть в код.
[CreateAssetMenu(fileName = "RespawnConfig", menuName = "Config/RespawnConfig")]
public class RespawnConfig : ScriptableObject
{
    [Tooltip("Время до возрождения после смерти, в секундах.")]
    public float respawnDelay = 5f;
}