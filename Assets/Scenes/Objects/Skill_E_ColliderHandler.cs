using NUnit.Framework;
using UnityEngine;

public class Skill_E_ColliderHandler : MonoBehaviour
{
    private Skill_E_Hanlder m_owner;
    public Skill_E_Hanlder owner { set { m_owner = value; } }

    private LayerMask m_enemy_layer;

    private void Awake()
    {
        m_enemy_layer = LayerMask.NameToLayer("Enemy");
    }

    private void Start()
    {
        Assert.IsNotNull(m_owner);
    }

    private void OnTriggerEnter(Collider other)
    {
        //첫 enemy 충돌체 확인 시 정보 전달 후 충돌체 끈다
        if (other.gameObject.layer == m_enemy_layer)
        {
            m_owner.EnemyHit(other);
            gameObject.SetActive(false);
        }
    }
}
