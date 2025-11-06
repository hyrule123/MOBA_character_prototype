using System.Collections;
using UnityEngine;

public enum eCharacterState
{
    Idle,
    Moving,
    Q,
    W,
    E,
    R,
    Suppressing
}

public enum eSkill
{ NONE, Q, W, E, R, EE }

public class PlayerMove : MonoBehaviour
{
    [SerializeField] private LayerMask m_ground_mask;

    private Camera m_camera;
    [SerializeField] private Animator m_animator;
    
    private eCharacterState m_state = eCharacterState.Idle;
    public eCharacterState state { get { return m_state; } }

    //Indicator
    private eSkill m_cur_indicating = eSkill.NONE;
    [SerializeField] private RangeIndicatorHandler m_range_indicator_handler;

    [Header("이동 관련 설정")]
    [SerializeField] private float m_move_speed = 3.0f;    // 초당 이동 속도
    [SerializeField] private float m_stop_dist = 0.1f; // 목표 지점과 이 거리만큼 가까워지면 멈춤
    private Vector3 m_target_position; // 이동 목표 지점

    [Header("Q 관련 변수")]
    [SerializeField] private float m_q_radius = 3f;
    [SerializeField] private Skill_Q_Handler m_Q_handle_inst;

    [Header("W 관련 변수")]
    [SerializeField] private float m_w_radius = 3f;
    [SerializeField] private Skill_W_Handler m_W_handle_inst;

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

    private void Awake()
    {
        m_Q_handle_inst.owner = this;
        m_E_handle_inst.owner = this;
    }

    void Start()
    {
        m_camera = Camera.main;
        m_target_position = transform.position;
        m_target_position.y = 0;

        m_Q_handle_inst.enabled = false;
        m_W_handle_inst.enabled = false;
        m_E_handle_inst.enabled = false;
        m_R_handle_inst.enabled = false;
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
    }

    public void OnSetTarget()
    {
        if(m_cur_indicating == eSkill.EE)
        {
            EE_Start();
            return;
        }

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

        SetArrived();

        ResetIndicators();

        m_cur_indicating = eSkill.Q;
        m_range_indicator_handler.EnableCircleIndicator(m_q_radius, m_q_radius, m_q_radius);
        m_range_indicator_handler.EnableArrowIndicator(m_q_radius);

        var portal_handler = m_W_handle_inst.launched_portal_handler;
        if (portal_handler)
        {
            portal_handler.range_indicator.EnableCircleIndicator(m_q_radius, m_q_radius, m_q_radius);
            portal_handler.range_indicator.EnableArrowIndicator(m_q_radius);

            //화살표 방향 업데이트를 중지한다.
            m_range_indicator_handler.stop_arrow_direction_update = true;

            //현재 플레이어가 향하고 있는 방향을 바라보도록 지정
            Vector3 fwd = transform.forward;
            fwd.y = 0;
            m_range_indicator_handler.SetArrowDirection(fwd);
        }

        StartCoroutine(Q_RangeUpdate());
    }

    private IEnumerator Q_RangeUpdate()
    {
        //Q 범위 표시 중에 본체의 화살표 방향을 업데이트해야 하는지 말아야 하는지를 지속 검사한다
        while(m_cur_indicating == eSkill.Q)
        {
            var portal_handler = m_W_handle_inst.launched_portal_handler;

            //포탈이 있으면 본체의 스킬 시전 방향은 업데이트하지 않는다
            m_range_indicator_handler.stop_arrow_direction_update = (null != portal_handler);

            yield return null;
        }
    }

    private void Q_Start()
    {
        if (m_camera)
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_ground_mask))
            {
                //기본 방향: 마우스 지정 방향
                Vector3 dir = hit.point - transform.position;
                dir.y = 0;

                //만약 포탈이 있다면: 플레이어의 정면 방향으로 시전
                if(m_W_handle_inst.launched_portal_handler)
                {
                    dir = transform.forward;
                    dir.y = 0;
                }

                //회전시킨 뒤 
                transform.rotation = Quaternion.LookRotation(dir);

                //Q스킬 핸들 스크립트 활성화
                if (m_Q_handle_inst)
                {
                    m_Q_handle_inst.SetParameters(m_W_handle_inst.launched_portal_handler, hit.point);
                    m_Q_handle_inst.enabled = true;
                }

                //스킬 시전(busy 상태 진입)
                TransitionState(eCharacterState.Q);
            }
        }


        //indicator off
        m_range_indicator_handler.DisableAllIndicators();
        m_W_handle_inst.StopIndicator();
        m_cur_indicating = eSkill.NONE;
    }

    public void Q_End()
    {
        if(m_Q_handle_inst)
        {
            m_Q_handle_inst.enabled = false;
        }

        SetArrived();
        TransitionState(eCharacterState.Idle);
    }

    public void ToggleRangeW()
    {
        if (IsBusy())
        {
            return;
        }

        ResetIndicators();
        m_cur_indicating = eSkill.W;
        m_range_indicator_handler.EnableCircleIndicator(m_w_radius, m_w_radius, m_w_radius);
        m_range_indicator_handler.EnableMouseIndicator(m_w_radius);
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
                if (dist <= m_range_indicator_handler.mouse_indicator_range)
                {
                    dir.Normalize();
                    transform.rotation = Quaternion.LookRotation(dir);

                    if(m_W_handle_inst)
                    {
                        m_W_handle_inst.SetParameters(target_pos);
                        m_W_handle_inst.enabled = true;
                    }

                    //스킬 시전(busy 상태 진입)
                    TransitionState(eCharacterState.W);
                }
            }
        }

        //indicator off
        m_range_indicator_handler.DisableAllIndicators();
        m_cur_indicating = eSkill.NONE;
    }
    public void W_End()
    {
        if(m_W_handle_inst)
        {
            m_W_handle_inst.enabled = false;
        }

        SetArrived();
        TransitionState(eCharacterState.Idle);
    }


    public void ToggleRangeE()
    {
        Debug.Log("ToggleRange Called!");
        //E 대시 중 적 캐릭터와 충돌해 제압중이면 다른 동작을 해야 함
        if(m_state == eCharacterState.Suppressing && m_cur_indicating != eSkill.EE)
        {
            ResetIndicators();
            m_cur_indicating = eSkill.EE;

            m_range_indicator_handler.EnableCircleIndicator(m_q_radius, m_q_radius, m_q_radius);
            m_range_indicator_handler.EnableArrowIndicator(m_q_radius);

            //포탈이 있을 경우 동작이 달라진다.
            //제압 상태에서는 포탈이 사라지지 않는다.(설정한 제압 시간만큼만 증가)
            var portal_handler = m_W_handle_inst.launched_portal_handler;
            if (portal_handler)
            {
                portal_handler.range_indicator.EnableCircleIndicator(m_q_radius, m_q_radius, m_q_radius);
                portal_handler.range_indicator.EnableArrowIndicator(m_q_radius);

                //화살표 방향 업데이트를 중지한다.
                m_range_indicator_handler.stop_arrow_direction_update = true;

                //현재 플레이어가 향하고 있는 방향을 바라보도록 지정
                Vector3 fwd = transform.forward;
                fwd.y = 0;
                m_range_indicator_handler.SetArrowDirection(fwd);
            }

            return;
        }

        if (IsBusy())
        {
            return;
        }

        ResetIndicators();
        m_cur_indicating = eSkill.E;
        m_range_indicator_handler.EnableArrowIndicator(m_E_dist);
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
        m_range_indicator_handler.DisableAllIndicators();
        m_cur_indicating = eSkill.NONE;
    }
    public void E_End()
    {
        SetArrived();
        //Suppression 상태에서도 이 함수가 호출됨 - 예외 처리 필요
        if(m_state == eCharacterState.E)
        {
            TransitionState(eCharacterState.Idle);
            ResetIndicators();
        }
    }

    public void EE_End()
    {
        SetArrived();
        ResetIndicators();
        if(Get_W_PortalHandler())
        {
            Get_W_PortalHandler().DelayDestroy(false);
        }
    }

    public void ToggleRangeR()
    {
        if (IsBusy())
        {
            return;
        }

        ResetIndicators();
        m_cur_indicating = eSkill.R;
        m_range_indicator_handler.EnableArrowIndicator(m_R_range);
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
                    m_R_handle_inst.SetParameters(m_W_handle_inst.launched_portal_handler);
                    m_R_handle_inst.enabled = true;
                }

                //스킬 시전(busy 상태 진입)
                TransitionState(eCharacterState.R);
            }
        }

        //indicator off
        m_range_indicator_handler.DisableAllIndicators();
        m_cur_indicating = eSkill.NONE;
    }
    public void R_End()
    {
        SetArrived();
        m_R_handle_inst.enabled = false;
        TransitionState(eCharacterState.Idle);
    }

    public void EE_Start()
    {
        if (m_camera)
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_ground_mask))
            {
                //기본 방향: 마우스 지정 방향
                Vector3 dir = hit.point - transform.position;
                dir.y = 0;

                //만약 포탈이 있다면: 플레이어의 정면 방향으로 시전
                if (m_W_handle_inst.launched_portal_handler)
                {
                    dir = transform.forward;
                    dir.y = 0;
                }

                //회전시킨 뒤 
                transform.rotation = Quaternion.LookRotation(dir);

                Vector3 target_pos = hit.point;
                target_pos.y = 0;
                m_E_handle_inst.EE_Start(Get_W_PortalHandler(), target_pos);
            }
        }
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
        if(IsBusy()) { return; }

        m_target_position = newPosition;
        m_target_position.y = 0;

        m_range_indicator_handler.DisableAllIndicators();
        m_W_handle_inst.StopIndicator();
        
        m_cur_indicating = eSkill.NONE;
    }

    //state를 최종적으로 변경하는 함수.
    //실제 state를 변경 가능할 때에만 호출할 것
    public void TransitionState(eCharacterState state)
    {
        m_state = state;
        m_animator.SetInteger(m_state_hash, ((int)m_state));
    }
    
    //도착
    private void SetArrived()
    {
        m_target_position = transform.position;
    }

    private void ResetIndicators()
    {
        m_range_indicator_handler.DisableAllIndicators();
        var portal_handler = m_W_handle_inst.launched_portal_handler;
        if (portal_handler && portal_handler.range_indicator)
        {
            portal_handler.range_indicator.DisableAllIndicators();
        }
    }

    public W_PortalHandler Get_W_PortalHandler()
    {
        return m_W_handle_inst.launched_portal_handler;
    }
}
