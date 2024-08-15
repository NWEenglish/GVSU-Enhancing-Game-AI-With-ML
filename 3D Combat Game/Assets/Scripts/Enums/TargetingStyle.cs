namespace Assets.Scripts.Enums
{
    public enum TargetingStyle
    {
        ClosestFirst,   // Targets the closest enemy command post
        FurthestFirst,  // Targets the furthest enemy command post
        Defensive,      // Defends the furthest captured command post in a chain from spawn, else targets closest command post to spawn
        Random          // Picks a random enemy command post
    }
}
