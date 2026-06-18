using UnityEngine;
using UnityEngine.AI;
using KnightAdventure.Utils;
using System;


public class EnemyAI : MonoBehaviour
{

    [SerializeField] private State startingState;
    [SerializeField] private float roamingDistanceMax = 7f;
    [SerializeField] private float roamingDistanceMin = 3f;
    [SerializeField] private float roamingTimerMax = 2f;
    [SerializeField] private bool isChasingEnemy = false;
    [SerializeField] private float chasingDistance = 4f;
    [SerializeField] private float chasingSpeedMultiplier = 2f;
    [SerializeField] private bool isAttackingEnemy = false;
    [SerializeField] private float attackingDistance = 2f;
    [SerializeField] private float attackRate = 2f;
    [SerializeField] private float nextAttackTime = 0;

    private NavMeshAgent navMeshAgent;
    private State currentState;
    private float roamingTime;
    private Vector3 roamingPosition;
    private Vector3 startingPosition;

    private float roamingSpeed;
    private float chasingSpeed;

    private float nextCheckDirectionTime = 0f;
    private float checkDirectionDuration = 0.1f;
    private Vector3 lastPosition;

    public event EventHandler OnEnemyAttack;

    public bool IsRunning {
        get {
            if (navMeshAgent.velocity == Vector3.zero) {
                return false;
            } else {
                return true;
            }
        }
    }

    private enum State {
        Idle,
        Roaming,
        Chasing, 
        Attacking,
        Death
    }

    private void Awake() {
        navMeshAgent = GetComponent<NavMeshAgent>();
        navMeshAgent.updateRotation = false;
        navMeshAgent.updateUpAxis = false;
        currentState = startingState;

        roamingSpeed = navMeshAgent.speed;
        chasingSpeed = navMeshAgent.speed * chasingSpeedMultiplier;
    }

    private void Update() {
        StateHandler();
        MovmentDirectionHandler();
    }

    public void SetDeathState() {
        navMeshAgent.ResetPath();
        currentState = State.Death;
    }

    private void StateHandler() {
        switch (currentState) {
            case State.Roaming:
                roamingTime -= Time.deltaTime;
                if (roamingTime < 0) {
                    Roaming();
                    roamingTime = roamingTimerMax;
                }
                CheckCurrentState();
                break;
            case State.Chasing:
                ChaisingTarget();
                CheckCurrentState();
                break;
            case State.Attacking:
                AttackingTarget();
                CheckCurrentState();
                break;
            case State.Death:
                break;
            default:
            case State.Idle:
                break;
        }
    }

    private void AttackingTarget() {
        if (Time.time > nextAttackTime) {
            OnEnemyAttack?.Invoke(this, EventArgs.Empty);

            nextAttackTime = Time.time + attackRate;
        }
    }

    private void ChaisingTarget() {
        navMeshAgent.SetDestination(Player.Instance.transform.position);
    }

    public float GetRoamingAnimationSpeed() {
        return navMeshAgent.speed / roamingSpeed;
    }

    private void CheckCurrentState() {
        float distanceToPlayer = Vector3.Distance(transform.position, Player.Instance.transform.position);
        State newState = State.Roaming;

        if (isChasingEnemy) {
            if (distanceToPlayer <= chasingDistance) {
                newState = State.Chasing;
            }
        }

        if (isAttackingEnemy) {
            if (distanceToPlayer <= attackingDistance) {
                if (Player.Instance.isAlive) {
                    newState = State.Attacking;
                } else {
                    newState = State.Roaming;
                }
            }
        }

        if (newState != currentState) {
            if (newState == State.Chasing) {
                navMeshAgent.ResetPath();
                navMeshAgent.speed = chasingSpeed;
            } else if (newState == State.Roaming) {
                roamingTime = 0;
                navMeshAgent.speed = roamingSpeed;
            } else if (newState == State.Attacking) {
                navMeshAgent.ResetPath();
            }

            currentState = newState;
        }
    }

    private void MovmentDirectionHandler() {
        if (Time.time > nextCheckDirectionTime) {
            if (IsRunning) {
                ChangeFacingDirection(lastPosition, transform.position);
            } else if (currentState == State.Attacking) {
                ChangeFacingDirection(transform.position, Player.Instance.transform.position);
            }

            lastPosition = transform.position;
            nextCheckDirectionTime = Time.time + checkDirectionDuration;
        }
    }

    private void Roaming() {
        startingPosition = transform.position;
        roamingPosition = GetRoamingPosition();
        navMeshAgent.SetDestination(roamingPosition);
    }

    private Vector3 GetRoamingPosition() {
        return startingPosition + Utils.GetRandomDir() * UnityEngine.Random.Range(roamingDistanceMin, roamingDistanceMax);
    }

    private void ChangeFacingDirection(Vector3 sourcePosition, Vector3 targetPosition) {
        if (sourcePosition.x > targetPosition.x) {
            transform.rotation = Quaternion.Euler(0, -180, 0);
        } else {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }
    }
}
