using NUnit.Framework;
using UnityEngine;

public class Skill_Q_Handler : MonoBehaviour
{
    [SerializeField] private GameObject m_upper_arm;
    [SerializeField] private GameObject m_lower_arm;

    [SerializeField] private GameObject m_blue_portal_inst;
    [SerializeField] private GameObject m_blue_portal_prefab;
    private GameObject m_casted_blue_portal_inst;

    private bool m_b_ready;
    private W_PortalHandler m_portal_inst;
    private Vector3 m_hit_point;    //W 포탈이 있을 경우 위치 계산에 필요
    public void SetParameters(W_PortalHandler portal_inst, Vector3 hit_point)
    {
        m_b_ready = true;
        m_portal_inst = portal_inst;
        m_hit_point = hit_point;
    }

    //Q 스킬 시전 시 호출 됨
    private void OnEnable()
    {
        Assert.IsTrue(m_b_ready);

        if (m_portal_inst)
        {
            m_portal_inst.DelayDestroy(true);
            m_blue_portal_inst.SetActive(false);
            m_casted_blue_portal_inst = Instantiate(m_blue_portal_prefab, this.transform);
            m_casted_blue_portal_inst.transform.localPosition = new Vector3(0, 1, 0.5f);
            m_casted_blue_portal_inst.transform.localScale = new Vector3(2, 2, 2);
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

    //Q 스킬 종료 시 호출됨
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

        if(m_casted_blue_portal_inst)
        {
            Destroy(m_casted_blue_portal_inst);
            m_casted_blue_portal_inst = null;
            m_blue_portal_inst.SetActive(true);
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

    private void LateUpdate()
    {
        //포탈 방향에서 팔이 발사되는 느낌
        if (m_portal_inst)
        {
            m_upper_arm.transform.position = m_portal_inst.transform.position;
            m_upper_arm.transform.rotation = Quaternion.LookRotation(m_portal_inst.transform.forward);
        }
    }
}
