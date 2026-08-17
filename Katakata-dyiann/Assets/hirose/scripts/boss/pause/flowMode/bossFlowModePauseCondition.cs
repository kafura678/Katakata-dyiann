using UnityEngine;

public class bossFlowModePauseCondition : MonoBehaviour
{
    [SerializeField] FlowManager flowManager;

    public bool isPauseRequested => flowManager != null && flowManager.isFlowMode;
}
