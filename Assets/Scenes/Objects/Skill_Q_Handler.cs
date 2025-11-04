using NUnit.Framework;
using UnityEngine;

public class Skill_Q_Handler : MonoBehaviour
{
    [SerializeField] private GameObject m_upper_arm;
    [SerializeField] private GameObject m_lower_arm;

    private bool m_b_ready;
    private W_PortalHandler m_portal_inst;
    private Vector3 m_hit_point;    //W 포탈이 있을 경우 위치 계산에 필요
    public void SetParameters(W_PortalHandler portal_inst, Vector3 hit_point)
    {
        m_b_ready = true;
        m_portal_inst = portal_inst;
        m_hit_point = hit_point;
    }

    private void OnEnable()
    {
        Assert.IsTrue(m_b_ready);

        if (m_portal_inst)
        {
            m_portal_inst.DelayDestroy(true);
        }

        if (m_upper_arm)
        {
            m_upper_arm.transform.localScale = Vector3.one * 2;
        }
        if(m_lower_arm)
        {
            m_lower_arm.transform.localScale = Vector3.one * 2;
        }
    }

    private void OnDisable()
    {
        if (m_upper_arm)
        {
            m_upper_arm.transform.localScale = Vector3.one;
        }
        if (m_lower_arm)
        {
            m_lower_arm.transform.localScale = Vector3.one;
        }
        
        if (m_portal_inst)
        {
            m_portal_inst.DelayDestroy(false);
        }

        m_b_ready = false;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
