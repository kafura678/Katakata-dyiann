public interface bossAttackPhase
{
    bool isCompleted { get; }
    bool isAvailable { get; }

    void enter();
    void updatePhase(float deltaTime);
    void pause();
    void resume();
    void exit();
}
