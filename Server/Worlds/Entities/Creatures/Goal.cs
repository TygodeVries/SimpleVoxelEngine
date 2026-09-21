namespace Server.Worlds;

public abstract class Goal
{
    public Creature? Creature { get; internal set; }

    public abstract int GetPriority();
    public virtual bool CanStart() => true;
    public virtual void Start() { }
    public virtual void Stop() { }
    public virtual void Update() { }
}
