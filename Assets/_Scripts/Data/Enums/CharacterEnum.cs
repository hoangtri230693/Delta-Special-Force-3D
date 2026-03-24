

namespace DeltaSpecialForce3D.Enums
{
    public enum MovementState { Idle, Walk, Run, JumpOI, JumpOM, Fall }
    public enum StanceState { Stand, Crouch }
    public enum CombatState { None, Aim }
    public enum ActionState { None, ManualShoot, AutomaticShoot, Melee, Throw, Reload, Drop }
    public enum LifeState { None, Alive, Hurt, DeathShoot, DeathMelee, DeathThrow }
    public enum TeamName { None, Counter, Terrorist, Zombie }
    public enum CharacterName 
    { 
        None, Alpha, Bravo, Delta,
        BlackViper, IronBlood, RedFang, ShadowDawn,
        ZombieWalker, ZombieRunner, ZombieShambler 
    }
}
