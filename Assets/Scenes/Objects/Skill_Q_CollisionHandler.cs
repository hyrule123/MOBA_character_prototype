using NUnit.Framework;
using UnityEngine;


public class Skill_Q_CollisionHandler : MonoBehaviour
{
    private Skill_Q_Handler m_owner_script;
    public Skill_Q_Handler owner_script { set { m_owner_script = value; } }

    private LayerMask m_enemy_mask;
    private void Awake()
    {
        m_enemy_mask= LayerMask.NameToLayer("Enemy");
    }

    //적과 충돌 감지시 주인에게 그대로 전달
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == m_enemy_mask)
        {
            if(m_owner_script)
            {
                m_owner_script.EnemyHit(other);
            }
            else
            {
                Debug.Log("Skill_Q_CollisionHandler: owner object가 없습니다.");
            }

            //최초 감지한 적을 전달하고 작동 종료
            gameObject.SetActive(false);
        }
    }
}
