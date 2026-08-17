using System.Collections.Generic;
using UnityEngine;

public class bossRandomMoveTargetSelector : MonoBehaviour
{
    [SerializeField] List<Transform> targetTfs = new List<Transform>();

    readonly List<Transform> validTargetTfs = new List<Transform>();

    public Transform select()
    {
        collectValidTargets();

        if (validTargetTfs.Count == 0)
        {
            return null;
        }

        return validTargetTfs[Random.Range(0, validTargetTfs.Count)];
    }

    void collectValidTargets()
    {
        validTargetTfs.Clear();

        foreach (Transform targetTf in targetTfs)
        {
            if (targetTf == null)
            {
                continue;
            }

            validTargetTfs.Add(targetTf);
        }
    }
}
