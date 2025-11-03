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
    [SerializeField] private Animator m_animator;

    [SerializeField] private GameObject m_range_indicator_radius;
    private eSkill m_cur_indicating = eSkill.NONE;

    private Vector3 m_target_position; // 이동 목표 지점

    private eCharacterState m_state = eCharacterState.Idle;

    [Header("이동 관련 설정")]
    [SerializeField] private float m_move_speed = 3.0f;    // 초당 이동 속도
    [SerializeField] private float m_stop_dist = 0.1f; // 목표 지점과 이 거리만큼 가까워지면 멈춤

    [Header("Q 관련 변수")]
    [SerializeField] private float m_q_radius = 3f;



    private readonly int m_state_hash = Animator.StringToHash("m_state");

    private bool IsBusy()
    {
        return m_state > eCharacterState.Moving;
    }

    void Start()
    {
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
                m_state = eCharacterState.Idle;
                m_target_position = transform.position;
            }
            //이동 중일 경우
            else
            { 
                m_state = eCharacterState.Moving;

                //방향 정규화
                dir.Normalize();
                transform.position += (dir * Time.deltaTime * m_move_speed);

                transform.rotation = Quaternion.LookRotation(dir);
            }
        }

        m_animator.SetInteger(m_state_hash, ((int)m_state));
    }

    public void ToggleRangeQ()
    {
        if(IsBusy())
        {
            return;
        }
        
        EnableCircleIndicator(m_q_radius, m_q_radius, m_q_radius);
    }

    public void MoveTo(Vector3 newPosition)
    {
        m_target_position = newPosition;
        m_target_position.y = 0;

        DisableCircleIndicator();
    }

    private void EnableCircleIndicator(float x, float y, float z)
    {
        if (m_range_indicator_radius)
        {
            m_range_indicator_radius.SetActive(true);
            m_range_indicator_radius.transform.localScale.Set(x, y, z);
        }
    }
    private void DisableCircleIndicator()
    {
        m_range_indicator_radius.SetActive(false);
    }
}
