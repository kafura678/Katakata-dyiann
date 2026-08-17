public class bossAttackPhaseController
{
    readonly bossNormalAttackPhase normalAttackPhase;
    readonly bossSpecialAttackSelector specialAttackSelector;

    bossAttackPhase currentPhase;
    bool isRunning;

    public bossAttackPhaseController(
        bossNormalAttackPhase normalAttackPhase,
        bossSpecialAttackSelector specialAttackSelector)
    {
        this.normalAttackPhase = normalAttackPhase;
        this.specialAttackSelector = specialAttackSelector;
    }

    public void start()
    {
        if (isRunning || normalAttackPhase == null || !normalAttackPhase.isAvailable)
        {
            return;
        }

        isRunning = true;
        changePhase(normalAttackPhase);
    }

    public void updatePhase(float deltaTime)
    {
        if (!isRunning || currentPhase == null || deltaTime <= 0f)
        {
            return;
        }

        currentPhase.updatePhase(deltaTime);

        if (currentPhase.isCompleted)
        {
            moveToNextPhase();
        }
    }

    public void end()
    {
        if (!isRunning)
        {
            return;
        }

        currentPhase?.exit();
        currentPhase = null;
        isRunning = false;
    }

    void moveToNextPhase()
    {
        if (currentPhase is bossSpecialAttackPhase)
        {
            changePhase(normalAttackPhase);
            return;
        }

        bossSpecialAttackPhase selectedSpecialAttack = specialAttackSelector.select();
        changePhase(selectedSpecialAttack != null ? selectedSpecialAttack : normalAttackPhase);
    }

    void changePhase(bossAttackPhase nextPhase)
    {
        currentPhase?.exit();
        currentPhase = nextPhase;
        currentPhase?.enter();
    }
}
