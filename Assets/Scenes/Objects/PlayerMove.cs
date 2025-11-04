using UnityEngine;

public enum eCharacterState
{
    Idle,
    Moving,
    Q,
    W,
    E,
    R
}

public enum eSkill
{ NONE, Q, W, E, R }

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private LayerMask m_ground_mask;   //지정 필요

    private Camera m_camera;
    [SerializeField] private Animator m_animator;

    [SerializeField] private GameObject m_range_indicator_radius;
    [SerializeField] private GameObject m_range_indicator_arrow;
    [SerializeField] private GameObject m_range_indicator_mouse;
    private float m_mouse_indicator_range;

    private eSkill m_cur_indicating = eSkill.NONE;

    private Vector3 m_target_position; // 이동 목표 지점

    private eCharacterState m_state = eCharacterState.Idle;

    [Header("이동 관련 설정")]
    [SerializeField] private float m_move_speed = 3.0f;    // 초당 이동 속도
    [SerializeField] private float m_stop_dist = 0.1f; // 목표 지점과 이 거리만큼 가까워지면 멈춤

    [Header("Q 관련 변수")]
    [SerializeField] private float m_q_radius = 3f;
    [SerializeField] private GameObject[] m_fist_arm_meshes;

    [Header("W 관련 변수")]
    [SerializeField] private float m_w_radius = 3f;
    [SerializeField] private GameObject[] m_portal_arm_meshes;
    [SerializeField] private GameObject m_blue_portal;
    [SerializeField] private GameObject m_orange_portal;
    [SerializeField] private GameObject m_orange_portal_prefab;
    private GameObject m_launched_portal_ref;

    [Header("E 관련 변수")]
    [SerializeField] private float m_E_dist = 3f;
    [SerializeField] private float m_E_duration = 0.2f;
    [SerializeField] private Skill_E_Hanlder m_E_handle_inst;

    [Header("R 관련 변수")]
    [SerializeField] private float m_R_range = 3f;
    [SerializeField] private Skill_R_Handler m_R_handle_inst;

    private readonly int m_state_hash = Animator.StringToHash("m_state");
    private bool IsBusy()
    {
        return m_state > eCharacterState.Moving;
    }

    void Start()
    {
        m_camera = Camera.main;
        m_target_position = transform.position;
        m_target_position.y = 0;
    }

    void Update()
    {

        if(false == IsBusy())
        {
            //목표지점과의 거리 계산
            Vector3 dir = m_target_position - transform.position;
            dir.y = 0;
            float dist = dir.magnitude;

            //목표지점에 도착해 있을 경우
            if (dist <= m_stop_dist)
            {
                SetArrived();
                TransitionState(eCharacterState.Idle);
            }
            //이동 중일 경우
            else
            {
                //방향 정규화
                dir.Normalize();
                transform.position += (dir * Time.deltaTime * m_move_speed);

                transform.rotation = Quaternion.LookRotation(dir);

                TransitionState(eCharacterState.Moving);
            }
        }

        UpdateArrowIndicator();
        UpdateMouseIndicator();
    }

    public void OnSetTarget()
    {
        if (IsBusy()) { return; }

        switch (m_cur_indicating)
        {
            case eSkill.NONE:
                return;
            case eSkill.Q:
                Q_Start();
                return;
            case eSkill.W:
                W_Start();
                return;
            case eSkill.E:
                E_Start();
                return;
            case eSkill.R:
                R_Start();
                return;
            default:
                return;
        }
    }

    public void ToggleRangeQ()
    {
        if (IsBusy())
        {
            return;
        }

        m_cur_indicating = eSkill.Q;
        SetArrived();
        EnableCircleIndicator(m_q_radius, m_q_radius, m_q_radius);
        EnableArrowIndicator(m_q_radius);
    }

    private void Q_Start()
    {
        if (m_camera)
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_ground_mask))
            {
                Vector3 dir = hit.point - transform.position;
                dir.y = 0;

                //회전시킨 뒤 
                transform.rotation = Quaternion.LookRotation(dir);

                //팔길이 늘린다
                for (int i = 0; i < m_fist_arm_meshes.Length; ++i)
                {
                    if (m_fist_arm_meshes[i])
                    {
                        m_fist_arm_meshes[i].transform.localScale = new Vector3(2, 2, 2);
                    }
                }

                //스킬 시전(busy 상태 진입)
                TransitionState(eCharacterState.Q);
            }
        }

        //indicator off
        DisableAllIndicators();
        m_cur_indicating = eSkill.NONE;
    }

    public void Q_End()
    {
        SetArrived();

        //팔길이 원상복구
        for (int i = 0; i < m_fist_arm_meshes.Length; ++i)
        {
            if (m_fist_arm_meshes[i])
            {
                m_fist_arm_meshes[i].transform.localScale = Vector3.one;
            }
        }

        TransitionState(eCharacterState.Idle);
    }

    public void ToggleRangeW()
    {
        if (IsBusy())
        {
            return;
        }

        m_cur_indicating = eSkill.W;
        EnableCircleIndicator(m_w_radius, m_w_radius, m_w_radius);
        EnableMouseIndicator(m_w_radius);
    }

    public void W_Start()
    {
        if (m_camera)
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_ground_mask))
            {
                Vector3 target_pos = hit.point;

                Vector3 dir = target_pos - transform.position;
                dir.y = 0;
                float dist = dir.magnitude;

                //최대 거리를 넘어서지 않을 때만 시전
                if (dist <= m_mouse_indicator_range)
                {
                    dir.Normalize();
                    transform.rotation = Quaternion.LookRotation(dir);

                    if(m_orange_portal_prefab)
                    {
                        Vector3 init_pos = transform.position;
                        
                        //약간 땅에서 위로 띄워줌
                        init_pos.y = 1;
                        target_pos.y = 1;

                        //생성 후 발사
                        m_launched_portal_ref = Instantiate(m_orange_portal_prefab, init_pos, transform.rotation);
                        W_PortalHandler handler = m_launched_portal_ref.GetComponent<W_PortalHandler>();
                        handler.launch(this, m_orange_portal.transform.localScale.x, target_pos);
                        Debug.Log("w_portal launched");

                        //손에 있는 포탈 제거
                        m_orange_portal.SetActive(false);
                    }

                    //스킬 시전(busy 상태 진입)
                    TransitionState(eCharacterState.W);
                }
            }
        }

        //indicator off
        DisableAllIndicators();
        m_cur_indicating = eSkill.NONE;
    }
    public void W_End()
    {
        SetArrived();
        TransitionState(eCharacterState.Idle);
    }
    public void On_W_PortalInstDestroy()
    {
        if(m_orange_portal)
        {
            m_orange_portal.SetActive(true);
        }
        m_launched_portal_ref = null;
    }

    public void ToggleRangeE()
    {
        if (IsBusy())
        {
            return;
        }

        m_cur_indicating = eSkill.E;
        EnableArrowIndicator(m_E_dist);
    }

    public void E_Start()
    {
        if (m_camera)
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_ground_mask))
            {
                Vector3 dir = hit.point - transform.position;
                dir.y = 0;

                //회전시킨 뒤 
                transform.rotation = Quaternion.LookRotation(dir);

                if(m_E_handle_inst)
                {
                    m_E_handle_inst.SetParameters(m_E_dist, m_E_duration);
                    m_E_handle_inst.enabled = true;
                }

                //스킬 시전(busy 상태 진입)
                TransitionState(eCharacterState.E);
            }
        }

        //indicator off
        DisableAllIndicators();
        m_cur_indicating = eSkill.NONE;
    }
    public void E_End()
    {
        SetArrived();
        TransitionState(eCharacterState.Idle);
    }
    public void ToggleRangeR()
    {
        if (IsBusy())
        {
            return;
        }

        m_cur_indicating = eSkill.R;
        EnableArrowIndicator(m_R_range);
    }
    public void R_Start()
    {
        if (m_camera)
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_ground_mask))
            {
                Vector3 dir = hit.point - transform.position;
                dir.y = 0;

                //회전시킨 뒤 
                transform.rotation = Quaternion.LookRotation(dir);

                //궁극기 스크립트 활성화
                if(m_R_handle_inst)
                {
                    m_R_handle_inst.enabled = true;
                }

                //스킬 시전(busy 상태 진입)
                TransitionState(eCharacterState.R);
            }
        }

        //indicator off
        DisableAllIndicators();
        m_cur_indicating = eSkill.NONE;
    }
    public void R_End()
    {
        SetArrived();
        m_R_handle_inst.enabled = false;
        TransitionState(eCharacterState.Idle);
    }

    public void OnMove()
    {
        if (m_camera)
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_ground_mask))
            {
                Debug.Log("우클릭한 Plane의 위치: " + hit.point);
                MoveTo(hit.point);
            }
        }
    }

    private void MoveTo(Vector3 newPosition)
    {
        m_target_position = newPosition;
        m_target_position.y = 0;

        DisableAllIndicators();
        m_cur_indicating = eSkill.NONE;
    }

    //state를 최종적으로 변경하는 함수.
    //실제 state를 변경 가능할 때에만 호출할 것
    public void TransitionState(eCharacterState state)
    {
        m_state = state;
        m_animator.SetInteger(m_state_hash, ((int)m_state));
    }
    private void EnableCircleIndicator(float x, float y, float z)
    {
        if (m_range_indicator_radius)
        {
            m_range_indicator_radius.transform.localScale = new Vector3(x, y, z); 
            m_range_indicator_radius.SetActive(true);
        }
    }
    private void DisableCircleIndicator()
    {
        if(m_range_indicator_radius)
        {
            m_range_indicator_radius.SetActive(false);
        }
    }

    private void EnableArrowIndicator(float range)
    {
        if(m_range_indicator_arrow)
        {
            m_range_indicator_arrow.transform.localScale = new Vector3(m_q_radius, 1, 1);
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

    private void DisableArrowIndicator()
    {
        if (m_range_indicator_arrow)
        {
            m_range_indicator_arrow.SetActive(false);
        }
    }

    private void EnableMouseIndicator(float range)
    {
        if(m_range_indicator_mouse)
        {
            m_mouse_indicator_range = range;

            m_range_indicator_mouse.SetActive(true);
        }
    }

    private void UpdateMouseIndicator()
    {
        if(m_range_indicator_mouse && m_range_indicator_mouse.activeInHierarchy)
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

    private void DisableMouseIndicator()
    {
        if (m_range_indicator_mouse)
        {
            m_range_indicator_mouse.SetActive(false);
        }
    }

    private void DisableAllIndicators()
    {
        DisableCircleIndicator();
        DisableArrowIndicator();
        DisableMouseIndicator();
    }
    
    //도착
    private void SetArrived()
    {
        m_target_position = transform.position;
    }
}
