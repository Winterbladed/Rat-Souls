using UnityEngine;

public class EnemyRanged : Enemy
{
    #region Variables
    [Header("Enemy Ranged Reference")]
    [SerializeField] private Animator _animator;
    [SerializeField] private GameObject _rangedObject;
    [SerializeField] private Transform _rangedObjectSpawn;
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
        AttackRangeWhenNearTarget(_rangedObject, _rangedObjectSpawn);
    }

    private void Aniimator()
    {
        if (_agent.velocity.magnitude > 0.0f) _animator.SetBool("_isMoving", true);
        else if (_agent.velocity.magnitude <= 0.0f) _animator.SetBool("_isMoving", false);
    }
    #endregion
}