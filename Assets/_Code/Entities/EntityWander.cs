using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Entity))]
[RequireComponent(typeof(Animator))]
public class EntityWander : MonoBehaviour
{
    #region member variables

    public float _maxWanderDistance = 5f;
    public float _speed = 1f;
    public System.Action OnWanderCompleted;

    private Entity _entity;
    private NavMeshAgent _agent;
    private Vector3 _target, _previousTarget, _targetDirection;
    private Animator _animator;

    #endregion

    void Awake()
    {
        _entity = GetComponent<Entity>();
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _agent.speed = _speed;
    }

    void Update()
    {
        if (_entity._alive)
        {
            if (_agent.remainingDistance <= _agent.stoppingDistance)
            {
                OnWanderCompleted?.Invoke();
            }
            if (_targetDirection != Vector3.zero && GameController.Instance.CanKill)
            {
                FaceDirection(_targetDirection);
            }
        }
    }

    public void StartWandering()
    {
        if (!_entity._alive || !_agent) return;
        _previousTarget = _target;
        _target = GetRandomPoint();
        _agent.SetDestination(_target);
        float distance = Vector3.Distance(transform.position, _target);
        _animator.SetBool("IsWalking", true);
    }

    public void StopWandering()
    {
        _agent.velocity = Vector3.zero;
        _animator.SetBool("IsWalking", false);
    }

    public void FaceDirection(Vector3 target)
    {
        _targetDirection = target;
        Vector3 direction = (target - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private Vector3 GetRandomPoint()
    {
        Vector3 randomPoint = Random.insideUnitSphere * _maxWanderDistance;
        randomPoint += transform.position;
        randomPoint.y = transform.position.y;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomPoint, out hit, _maxWanderDistance, 1);
        return hit.position;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, _maxWanderDistance);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(_target, 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(_previousTarget, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(_previousTarget, _target);
    }

    public void Die()
    {
        StopWandering();
        _animator.enabled = false;
        _agent.enabled = false;
    }
}
