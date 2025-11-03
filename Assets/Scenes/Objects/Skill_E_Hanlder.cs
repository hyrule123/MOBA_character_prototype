using UnityEngine;
using System.Collections;

public class Skill_E_Hanlder : MonoBehaviour
{
    private float m_dash_dist = 3f;
    private float m_dash_duration = 0.2f;
    [SerializeField] private Animator m_animator;
    
    private readonly int m_E_duration_hash = Animator.StringToHash("m_E_duration_hash");

    public void SetParameters(float  dash_dist, float dash_duration)
    {
        m_dash_dist = dash_dist; 
        m_dash_duration = dash_duration;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    private void OnEnable()
    {
        if(m_animator)
        {
            m_animator.SetFloat(m_E_duration_hash, m_dash_duration);
        }
        StartCoroutine(Dash());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator Dash()
    {
        float start_time = Time.time;
        Vector3 start_pos = transform.position;
        //캐릭터가 바라보는 방향
        Vector3 target_pos = transform.position + transform.forward * m_dash_dist;

        //대시 지속 시간 동안 루프
        while (Time.time < start_time + m_dash_duration)
        {
            float t = (Time.time - start_time) / m_dash_duration;
            transform.position = Vector3.Lerp(start_pos, target_pos, t);

            //다음 프레임까지 대기
            yield return null; 
        }

        // 목표 지점에 정확히 위치시키고 종료
        transform.position = target_pos;
        this.enabled = false;
    }
}
