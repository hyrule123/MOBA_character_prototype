using NUnit.Framework;
using UnityEngine;

public class Skill_W_Handler : MonoBehaviour
{
    [SerializeField] private GameObject[] m_portal_arm_meshes;
    [SerializeField] private GameObject m_orange_portal;
    [SerializeField] private GameObject m_orange_portal_prefab;
    private W_PortalHandler m_launched_portal_handler;
    public W_PortalHandler launched_portal_handler { get { return m_launched_portal_handler; } }

    private bool m_b_ready;
    private Vector3 m_target_pos;

    public void SetParameters(Vector3 target_pos)
    {
        m_b_ready = true;
        m_target_pos = target_pos;
    }

    private void OnEnable()
    {
        Assert.IsTrue(m_b_ready);

        if (m_orange_portal_prefab)
        {
            Vector3 init_pos = transform.position;

            //약간 땅에서 위로 띄워줌
            init_pos.y = 1;
            m_target_pos.y = 1;

            //생성 후 발사
            GameObject inst = Instantiate(m_orange_portal_prefab, init_pos, transform.rotation);
            m_launched_portal_handler = inst.GetComponent<W_PortalHandler>();
            m_launched_portal_handler.launch(this, m_orange_portal.transform.localScale.x, m_target_pos);
            Debug.Log("w_portal launched");

            //손에 있는 포탈 제거
            m_orange_portal.SetActive(false);
        }
    }

    public void On_W_PortalInstDestroy()
    {
        if (m_orange_portal)
        {
            m_orange_portal.SetActive(true);
        }
        m_launched_portal_handler = null;
    }

    private void OnDisable()
    {
        m_b_ready = false;
    }

    //Stop indicators if the portal exists
    public void StopIndicator()
    {
        if(m_launched_portal_handler)
        {
            m_launched_portal_handler.range_indicator.DisableAllIndicators();
        }
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
