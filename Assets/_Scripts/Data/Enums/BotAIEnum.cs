using Unity.Behavior;


namespace DeltaSpecialForce3D.Enums
{
    [BlackboardEnum]
    public enum CounterState { Idle, Assault, Chase, Attack, Defend }

    [BlackboardEnum]
    public enum TerroristState { Idle, Patrol, Chase, Attack, Defend }

    [BlackboardEnum]
    public enum ZombieState { Idle, Chase, Attack }
}
