using UnityEngine;

public class EnemyMelee : Enemy
{
    #region Variables
    [Header("Enemy Melee Reference")]
    [SerializeField] private Animator _animator;
    #endregion

    #region Private Functions
    private void Start()
    {
        SetStats();
        GoToTarget();
    }

    private void Update()
    {
        Aniimator();
        GoToTargetWhenNear();
        AttackWhenNearTarget();
    }

    private void Aniimator()
    {
        if (_agent.velocity.magnitude > 0.0f) _animator.SetBool("_isMoving", true);
        else if (_agent.velocity.magnitude <= 0.0f) _animator.SetBool("_isMoving", false);
        _animator.SetBool("_isAttacking", _isAttacking);
    }
    #endregion
}