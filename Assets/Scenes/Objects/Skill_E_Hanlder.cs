using UnityEngine;
using System.Collections;
using NUnit.Framework;

public class Skill_E_Hanlder : MonoBehaviour
{
    private PlayerMove m_owner;
    public PlayerMove owner { set { m_owner = value; } }

    private float m_dash_dist = 3f;
    private float m_dash_duration = 0.2f;

    [SerializeField] private Skill_E_ColliderHandler m_skill_E_collider_handler;

    private Coroutine m_cur_coroutine;

    public void SetParameters(float  dash_dist, float dash_duration)
    {
        m_dash_dist = dash_dist; 
        m_dash_duration = dash_duration;
    }

    private void Awake()
    {
        Assert.IsNotNull(m_skill_E_collider_handler);
        m_skill_E_collider_handler.owner = this;
        m_skill_E_collider_handler.gameObject.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assert.IsNotNull(m_owner, "owner script가 설정되지 않음.");
    }

    private void OnEnable()
    {
        m_skill_E_collider_handler.gameObject.SetActive(true);
        m_cur_coroutine = StartCoroutine(Dash());
    }

    private void OnDisable()
    {
        m_cur_coroutine = null;
        m_skill_E_collider_handler.gameObject.SetActive(false);
    }

    private IEnumerator Dash()
    {
        Debug.Log("Dash coroutine called");

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

    public void EnemyHit(Collider col)
    {
        Debug.Log("E skill hit enemy!!");
        if(m_cur_coroutine != null)
        {
            StopCoroutine(m_cur_coroutine);

            var enemy_move = col.gameObject.GetComponent<EnemyMove>();
            if(enemy_move)
            {
                enemy_move.TransitionState(eEnemyState.Supperessed);
                m_owner.TransitionState(eCharacterState.Suppressing);

                Transform enemy_root = col.transform.root;
                Transform this_root = transform.root;

                enemy_root.SetParent(this_root);
                enemy_root.transform.localPosition = new Vector3(0, 0, 0.5f);

                //캐릭터가 보고 있는 방향과 일치시킨다
                enemy_root.rotation = Quaternion.LookRotation(this_root.forward);
            }

            this.enabled = false;
        }
    }
}
