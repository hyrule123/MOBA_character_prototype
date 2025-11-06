using NUnit.Framework;
using UnityEngine;

public enum eEnemyState
{ 
    Idle = 0, Supperessed
}

public class EnemyMove : MonoBehaviour
{
    private eEnemyState m_enemystate = eEnemyState.Idle;

    private bool m_b_thrown;
    public bool b_thrown { set { m_b_thrown = value; } }

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


    private void OnCollisionEnter(Collision collision)
    {
        if(false == m_b_thrown) { return; }

        m_b_thrown = false;
        var rb = GetComponent<Rigidbody>();
        if(rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }
        Vector3 pos = this.transform.position;
        pos.y = 0;
        this.transform.position = pos;
    }
}
