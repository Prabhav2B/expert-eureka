public class ProjectEnums 
{
    public enum JumpParameters
    {
        Height,
        Gravity,
        Time
    }
    
    public enum CharacterAction
    {
        JumpStationary,
        MoveBetweenPoints
    }
    
    public enum MovementState
    {
        Stopped = -1,
        Accelerating,
        Decelerating
    }
}
