using UnityEngine;
using System.Collections;
using NUnit.Framework;

public class Skill_E_Hanlder : MonoBehaviour
{
    private PlayerMove m_owner;
    public PlayerMove owner { set { m_owner = value; } }

    [SerializeField] private float m_dash_dist = 3f;
    [SerializeField] private float m_dash_duration = 0.2f;

    [SerializeField] private Skill_E_ColliderHandler m_skill_E_collider_handler;

    private Coroutine m_cur_coroutine;

    [SerializeField] private float m_suppression_time = 5f;
    [SerializeField] private float m_throw_force = 5f;
    [SerializeField] private float m_upward_force = 2f;

    private bool m_b_EE_start = false;
    private W_PortalHandler m_orange_portal_handler;
    private Vector3 m_EE_target_pos;
    
    public void EE_Start(W_PortalHandler portal_inst, Vector3 target_pos)
    {
        m_b_EE_start = true;
        m_orange_portal_handler = portal_inst;
        m_EE_target_pos = target_pos;
    }

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
        m_b_EE_start = false;
    }

    private void OnDisable()
    {
        m_cur_coroutine = null;
        m_skill_E_collider_handler.gameObject.SetActive(false);
        m_b_EE_start = false;
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
            //Dash 코루틴을 중지한다.
            StopCoroutine(m_cur_coroutine);

            //주인 스크립트에서 자신의 코루틴을 실행
            m_owner.StartCoroutine(OnSuppression(col.gameObject));

            this.enabled = false;
        }
    }

    public IEnumerator OnSuppression(GameObject other)
    {
        //혹시 모르니 false로 초기화
        m_b_EE_start = false;
        m_orange_portal_handler = null;

        var enemy_move = other.GetComponent<EnemyMove>();
        if(enemy_move == null) { yield break; }

        //자신과 적 모두 상태 변경
        m_owner.TransitionState(eCharacterState.Suppressing);
        enemy_move.TransitionState(eEnemyState.Supperessed);

        Transform enemy_root = other.transform.root;
        Transform this_root = transform.root;

        enemy_root.SetParent(this_root);
        enemy_root.transform.localPosition = new Vector3(0, 0, 0.5f);

        //캐릭터가 보고 있는 방향과 일치시킨다
        enemy_root.rotation = Quaternion.LookRotation(this_root.forward);

        //캐릭터 제압 시 포탈이 닫히지 않도록 만듦.
        var portal = m_owner.Get_W_PortalHandler();
        if (portal)
        {
            portal.DelayDestroy(true);
        }

        
        float remain_time = m_suppression_time;
        //시간이 남아있으면 입력이 들어올 때까지 대기
        while (0 < remain_time)
        {
            //입력이 들어왔다면 대기 해제하고
            if(m_b_EE_start) {  break; }

            remain_time -= Time.deltaTime;

            yield return null;
        }

        //EE 시전
        if (m_b_EE_start)
        {
            //포탈이 있으면
            if (m_orange_portal_handler)
            {
                //파란색 포탈 처리
                m_owner.StartCoroutine(BluePortalCoroutine());

                //enemy의 위치를 주황색 포탈 위치로 이동
                enemy_root.position = m_orange_portal_handler.transform.position;

                Vector3 dir = m_EE_target_pos - m_orange_portal_handler.transform.position;
                dir.y = 0;


                //던진다
                ThrowEnemy(enemy_root);
                enemy_move.b_thrown = true;
            }
            //포탈이 없으면 바로 적을 던져버린다
            else
            {
                ThrowEnemy(enemy_root);
                enemy_move.b_thrown = true;
            }
        }

        IEnumerator BluePortalCoroutine()
        {
            m_owner.blue_portal_on_arm.SetActive(false);

            //파란색 포탈 소환
            var casted_portal = Instantiate(m_owner.blue_portal_prefab, this.transform);
            casted_portal.transform.localPosition = new Vector3(0, 1, 0.5f);
            casted_portal.transform.localScale = new Vector3(2, 2, 2);

            yield return new WaitForSeconds(2f);

            m_owner.blue_portal_on_arm.SetActive(true);
            Destroy(casted_portal);
        }


        Debug.Log("Suppression END");
        //아무 동작 없이 종료 시 둘다 Idle 상태로 복귀
        m_owner.EE_End();
        m_owner.TransitionState(eCharacterState.Idle);
        enemy_move.TransitionState(eEnemyState.Idle);
        enemy_root.SetParent(null);
    }


    void ThrowEnemy(Transform enemy_transform)
    {
        enemy_transform.SetParent(null);
        //땅에서 살~짝 띄워준다(Ground Check 스크립트가 바로 작동하지 않도록)
        enemy_transform.position = enemy_transform.position + new Vector3(0, 0.01f, 0);

        //rigidbody ON
        var root_rb = enemy_transform.GetComponent<Rigidbody>();
        root_rb.isKinematic = false;
        root_rb.useGravity = true;

        //이 방향에서
        Vector3 dir = m_EE_target_pos - enemy_transform.position;
        dir.y = 0;
        dir.Normalize();

        //벡터를 약간 위쪽으로 올려준다.
        dir += (Vector3.up * m_upward_force);

        //발사
        root_rb.AddForce(dir * m_throw_force, ForceMode.Impulse);
    }
}
