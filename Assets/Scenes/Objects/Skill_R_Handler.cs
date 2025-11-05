using UnityEngine;
using System.Collections;
using NUnit.Framework;

//카메라가 내려와서 플래시(Spotlight) 팡 터뜨리고 감
//만약 포탈이 배치되어 있을 경우 포탈 쪽에도 플래시(Pointlight) 터뜨리고 감
public class Skill_R_Handler : MonoBehaviour
{
    [SerializeField] private GameObject m_camera_inst;
    [SerializeField] private GameObject m_camera_portal_inst;
    [SerializeField] private Light m_flash_inst;
    [SerializeField] private float m_prepare_time = 0.5f;
    [SerializeField] private float m_flash_fade_time = 0.5f;

    [SerializeField] private Vector3 m_initial_cam_localpos = new Vector3(0f, 2f, 0.5f);
    [SerializeField] private Vector3 m_final_cam_localpos = new Vector3(0f, 1f, 0.5f);

    [SerializeField] private GameObject m_blue_portal_on_arm;
    [SerializeField] private GameObject m_blue_portal_prefab;
    private GameObject m_casted_blue_portal_inst;

    private W_PortalHandler m_orange_portal_inst;
    [SerializeField] private float m_portal_flash_range = 3f;

    bool m_b_ready = false;
    public void SetParameters(W_PortalHandler orange_portal_inst)
    {
        m_b_ready = true;
        m_orange_portal_inst = orange_portal_inst;
    }

    private void OnEnable()
    {
        Assert.IsTrue(m_b_ready);

        if (m_camera_inst)
        {
            m_camera_inst.transform.localPosition = m_initial_cam_localpos;
            m_camera_inst.SetActive(true);

            //m_camera_portal_inst.SetActive((m_orange_portal_inst != null));
            m_camera_portal_inst.SetActive(false);

            //배치된 포탈이 있으면: 파란색 포탈을 카메라 앞에 장착
            //배치된 포탈의 파괴를 스킬 시전이 종료될때까지 유예
            if (m_orange_portal_inst)
            {
                m_blue_portal_on_arm.SetActive(false);
                m_camera_portal_inst.SetActive(true);

                m_orange_portal_inst.DelayDestroy(true);                
            }
        }
        if(m_flash_inst)
        {
            m_flash_inst.intensity = 0;
        }

        StartCoroutine(cast_R());
    }

    private void OnDisable()
    {
        m_b_ready = false;

        Debug.Log("R Handler Disabled");
        if (m_camera_inst)
        {
            m_camera_inst.SetActive(false);
        }

        if(m_orange_portal_inst)
        {
            m_orange_portal_inst.DelayDestroy(false);
            m_orange_portal_inst = null;
        }
    }

    private IEnumerator cast_R()
    {
        float prepare_time = 0f;

        //1. 준비동작
        while(prepare_time < m_prepare_time)
        {
            prepare_time += Time.deltaTime;

            float progress = prepare_time/ m_prepare_time;

            Vector3 cur_local_pos = Vector3.Lerp(m_initial_cam_localpos, m_final_cam_localpos, progress);

            m_camera_inst.transform.localPosition = cur_local_pos;

            yield return null;
        }

        //2. 스킬 시전(플래시 발동)
        //The value can be between 0 and 8. This allows you to create over bright lights.
        if(m_flash_inst)
        {
            m_flash_inst.intensity = 8f;
            float flash_fade_time = 0f;

            //주황색 포탈이 배치되어 있을 경우 포탈 쪽의 섬광도 활성화 한다.
            Light portal_light_inst = null;
            if (m_orange_portal_inst)
            {
                portal_light_inst = m_orange_portal_inst.GetComponent<Light>();
                Assert.IsNotNull(portal_light_inst);
                portal_light_inst.intensity = 0f;
                portal_light_inst.range = m_portal_flash_range;
                portal_light_inst.enabled = true;
            }

            while (flash_fade_time < m_flash_fade_time)
            {
                flash_fade_time += Time.deltaTime;

                float progress = flash_fade_time / m_flash_fade_time;

                float cur_intensity = Mathf.Lerp(8f, 0f, progress);

                m_flash_inst.intensity = cur_intensity;
                if(portal_light_inst)
                {
                    portal_light_inst.intensity = cur_intensity;
                }

                yield return null;
            }
        }
        else
        {
            Debug.Log("Flash instance가 없습니다");
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
