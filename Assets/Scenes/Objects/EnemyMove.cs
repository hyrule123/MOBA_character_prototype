using NUnit.Framework;
using UnityEngine;

public enum eEnemyState
{ 
    Idle = 0, Supperessed
}

public class EnemyMove : MonoBehaviour
{
    private eEnemyState m_enemystate = eEnemyState.Idle;

    private Animator m_animator;

    private readonly int m_state_hash = Animator.StringToHash("m_state");

    private void Awake()
    {
        m_animator = GetComponentInChildren<Animator>();
        Assert.NotNull(m_animator);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TransitionState(eEnemyState state)
    {
        m_enemystate = state;
        m_animator.SetInteger(m_state_hash, ((int)state));
    }
}
