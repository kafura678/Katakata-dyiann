using UnityEngine;
using System.Collections.Generic;

public class boss : MonoBehaviour
{
    [SerializeField] bossNormalAttackPhase normalAttackPhase;
    [SerializeField] List<bossSpecialAttackPhase> specialAttackPhases = new List<bossSpecialAttackPhase>();

    bossAttackPhaseController attackPhaseController;
    bool hasStarted;
    bool isDefeated;

    void Awake()
    {
        attackPhaseController = new bossAttackPhaseController(
            normalAttackPhase,
            new bossSpecialAttackSelector(specialAttackPhases));
    }

    void Start()
    {
        hasStarted = true;
        startAttackPhases();
    }

    void OnEnable()
    {
        if (hasStarted)
        {
            startAttackPhases();
        }
    }

    void FixedUpdate()
    {
        if (isDefeated || Time.timeScale <= 0f)
        {
            return;
        }

        attackPhaseController.updatePhase(Time.fixedDeltaTime);
    }

    void OnDisable()
    {
        attackPhaseController?.end();
    }

    void OnDestroy()
    {
        attackPhaseController?.end();
    }

    public void bossDefeated()
    {
        if (isDefeated)
        {
            return;
        }

        isDefeated = true;
        attackPhaseController.end();
    }

    void startAttackPhases()
    {
        if (isDefeated)
        {
            return;
        }

        attackPhaseController.start();
    }
}
