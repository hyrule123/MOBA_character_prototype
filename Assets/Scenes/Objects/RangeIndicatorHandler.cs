using UnityEngine;

public class RangeIndicatorHandler : MonoBehaviour
{
    private Camera m_camera;

    [SerializeField] private LayerMask m_ground_mask;

    [SerializeField] private GameObject m_range_indicator_radius;
    [SerializeField] private GameObject m_range_indicator_arrow;
    [SerializeField] private GameObject m_range_indicator_mouse;
    private float m_mouse_indicator_range;

    public float mouse_indicator_range { get{ return m_mouse_indicator_range; } }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_camera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateArrowIndicator();
        UpdateMouseIndicator();
    }


    public void EnableCircleIndicator(float x, float y, float z)
    {
        if (m_range_indicator_radius)
        {
            m_range_indicator_radius.transform.localScale = new Vector3(x, y, z);
            m_range_indicator_radius.SetActive(true);
        }
    }
    public void DisableCircleIndicator()
    {
        if (m_range_indicator_radius)
        {
            m_range_indicator_radius.SetActive(false);
        }
    }

    public void EnableArrowIndicator(float range)
    {
        if (m_range_indicator_arrow)
        {
            m_range_indicator_arrow.transform.localScale = new Vector3(range, 1, 1);
            m_range_indicator_arrow.SetActive(true);
        }
    }
    private void UpdateArrowIndicator()
    {
        //범위 화살표 회전
        if (m_range_indicator_arrow.activeInHierarchy && m_camera)
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_ground_mask))
            {
                Vector3 dir = hit.point - transform.position;
                dir.y = 0;

                //Euler Z 90도 회전: 화살표 방향이 플레이어 방향 기준 -90도 회전해 있는 상태임
                m_range_indicator_arrow.transform.rotation = Quaternion.LookRotation(dir) * Quaternion.Euler(90, 0, 90);
            }
        }
    }

    public void DisableArrowIndicator()
    {
        if (m_range_indicator_arrow)
        {
            m_range_indicator_arrow.SetActive(false);
        }
    }

    public void EnableMouseIndicator(float range)
    {
        if (m_range_indicator_mouse)
        {
            m_mouse_indicator_range = range;

            m_range_indicator_mouse.SetActive(true);
        }
    }

    private void UpdateMouseIndicator()
    {
        if (m_range_indicator_mouse && m_range_indicator_mouse.activeInHierarchy)
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_ground_mask))
            {
                Vector3 target_pos = hit.point;

                Vector3 dir = target_pos - transform.position;
                dir.y = 0;
                float dist = dir.magnitude;

                //만약 최대 거리를 넘어선다면 최대 거리 안으로 좁힌다
                if (dist > m_mouse_indicator_range)
                {
                    dir.Normalize();
                    target_pos = transform.position + dir * m_mouse_indicator_range;
                }

                target_pos.y = m_range_indicator_mouse.transform.position.y;
                m_range_indicator_mouse.transform.position = target_pos;
            }
        }
    }

    public void DisableMouseIndicator()
    {
        if (m_range_indicator_mouse)
        {
            m_range_indicator_mouse.SetActive(false);
        }
    }

    public void DisableAllIndicators()
    {
        DisableCircleIndicator();
        DisableArrowIndicator();
        DisableMouseIndicator();
    }
}
