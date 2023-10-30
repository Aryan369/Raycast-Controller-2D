public enum ThrowableStates
{
    Idle,
    Picked, // Picked by player
    Thrown, // Currently in thrown state (in air)
    Discarded, // Destroyed after being thrown and hitting or discarding (picking another item)
    TeleportPosSwitched  // Player switched Pos using Aminotejikara 
}