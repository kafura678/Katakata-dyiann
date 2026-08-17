using System.Collections.Generic;
using UnityEngine;

public class bossSpecialAttackSelector
{
    readonly IReadOnlyList<bossSpecialAttackPhase> specialAttackPhases;
    readonly List<bossSpecialAttackPhase> validPhases = new List<bossSpecialAttackPhase>();

    public bossSpecialAttackSelector(IReadOnlyList<bossSpecialAttackPhase> specialAttackPhases)
    {
        this.specialAttackPhases = specialAttackPhases;
    }

    public bossSpecialAttackPhase select()
    {
        validPhases.Clear();

        if (specialAttackPhases == null)
        {
            return null;
        }

        foreach (bossSpecialAttackPhase phase in specialAttackPhases)
        {
            if (phase != null && phase.isAvailable)
            {
                validPhases.Add(phase);
            }
        }

        if (validPhases.Count == 0)
        {
            return null;
        }

        return validPhases[Random.Range(0, validPhases.Count)];
    }
}
