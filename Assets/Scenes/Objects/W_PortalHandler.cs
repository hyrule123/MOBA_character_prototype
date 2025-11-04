using NUnit.Framework;
using UnityEngine;

public class W_PortalHandler : MonoBehaviour
{
    private Skill_W_Handler m_owner_script;

    private RangeIndicatorHandler m_range_indicator_inst;
    public RangeIndicatorHandler range_indicator { get { return m_range_indicator_inst; } }

    [SerializeField] private float m_speed = 3f;

    private float m_initial_scale = 0.3f;
    [SerializeField] private float m_final_scale = 1f;
    [SerializeField] private float m_duration = 5f;

    private bool m_is_ready = false;

    private float m_total_dist;
    private Vector3 m_destination_pos;
    private bool m_is_arrived = false;
    private const float m_arrive_threshold = 0.1f;

    private bool m_b_delay_destroy = false;

    public void DelayDestroy(bool need_delay)
    {
        m_b_delay_destroy = need_delay;
    }

    public void launch(Skill_W_Handler owner, float initial_scale, Vector3 dest_pos)
    {
        m_owner_script = owner;
        m_is_ready = true;
        m_initial_scale = initial_scale;
        m_destination_pos = dest_pos;
    }

    private void Awake()
    {
        m_range_indicator_inst = GetComponentInChildren<RangeIndicatorHandler>();
        Assert.IsNotNull(m_range_indicator_inst);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_total_dist = (m_destination_pos - transform.position).magnitude;
        Assert.IsTrue(m_is_ready);

        Debug.Log("w total dist = " + m_total_dist.ToString());
        Debug.Log("w dest = " + m_destination_pos.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        //아직 도착 안했으면 목적지까지 이동
        if(false == m_is_arrived)
        {
            Vector3 dir = m_destination_pos - transform.position;
            dir.y = 0f;
            float dist = dir.magnitude;
            dir.Normalize();

            if(dist > m_arrive_threshold)
            {
                transform.position += (dir * m_speed * Time.deltaTime);

                float progress = 1f - (dist / m_total_dist);
                float cur_scale = Mathf.Lerp(m_initial_scale, m_final_scale, progress);
                transform.localScale = new Vector3(cur_scale, cur_scale, cur_scale);
            }
            else
            {
                m_is_arrived = true;
                transform.position = m_destination_pos;
                transform.localScale = new Vector3(m_final_scale, m_final_scale, m_final_scale);
                Debug.Log("W arrived!!");
            }
        }

        //도착했으면 타이머 작동
        else
        {
            m_duration -= Time.deltaTime;
            if (false == m_b_delay_destroy && m_duration < 0)
            {
                Debug.Log("w Destroyed!!");
                Destroy(this.gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        if(m_owner_script)
        {
            m_owner_script.On_W_PortalInstDestroy();
        }
    }


}
