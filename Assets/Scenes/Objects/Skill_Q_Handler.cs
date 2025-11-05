using NUnit.Framework;
using UnityEngine;

public class Skill_Q_Handler : MonoBehaviour
{
    [SerializeField] private GameObject m_upper_arm;
    [SerializeField] private GameObject m_lower_arm;

    [SerializeField] private GameObject m_blue_portal_on_arm;
    [SerializeField] private GameObject m_blue_portal_prefab;
    private GameObject m_casted_blue_portal_inst;

    private bool m_b_ready;
    private W_PortalHandler m_portal_inst;
    private Vector3 m_target_direction;    //W 포탈이 있을 경우 위치 계산에 필요
    public void SetParameters(W_PortalHandler portal_inst, Vector3 target_direction)
    {
        m_b_ready = true;
        m_portal_inst = portal_inst;
        m_target_direction = target_direction;
    }

    //Q 스킬 시전 시 호출 됨
    private void OnEnable()
    {
        Assert.IsTrue(m_b_ready);

        if (m_portal_inst)
        {
            m_portal_inst.DelayDestroy(true);
            m_blue_portal_on_arm.SetActive(false);
            m_casted_blue_portal_inst = Instantiate(m_blue_portal_prefab, this.transform);
            m_casted_blue_portal_inst.transform.localPosition = new Vector3(0, 1, 0.5f);
            m_casted_blue_portal_inst.transform.localScale = new Vector3(2, 2, 2);

            //포탈의 방향을 지정 방향으로 회전시킨다.
            Vector3 pos = m_portal_inst.transform.position;
            pos.y = 0;
            m_target_direction.y = 0;
            Vector3 dir = m_target_direction - pos;
            m_portal_inst.transform.rotation = Quaternion.LookRotation(dir);
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
            m_blue_portal_on_arm.SetActive(true);
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
