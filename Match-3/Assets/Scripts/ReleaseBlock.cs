using System;
using UnityEngine;
public class ReleaseBlock : StateMachineBehaviour
{
    private static bool called;
    public static Action OnDespawnAnimationDone;
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        called = false;
        animator.gameObject.GetComponent<Collider2D>().enabled = false;
    }
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        GameObject block = animator.gameObject;
        BlockObjectPool.Get().Pool.Release(block);
        if (called) return;
        called = true;
        OnDespawnAnimationDone?.Invoke();
    }
}