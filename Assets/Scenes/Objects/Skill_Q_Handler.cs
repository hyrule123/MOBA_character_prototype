using NUnit.Framework;
using System.Collections;
using UnityEngine;

public class Skill_Q_Handler : MonoBehaviour
{
    private PlayerMove m_owner;
    public PlayerMove owner { set { m_owner = value; } }

    [SerializeField] private GameObject m_upper_arm;
    [SerializeField] private GameObject m_lower_arm;
    [SerializeField] private GameObject m_fist_collider_obj;

    private GameObject m_casted_blue_portal_inst;


    //그랩
    [SerializeField] private float m_grab_pull_speed = 5f;
    private float m_max_grab_time = 5f;

    //기본 Parameter
    private bool m_b_ready;
    private W_PortalHandler m_portal_inst;
    private Vector3 m_target_direction;    //W 포탈이 있을 경우 위치 계산에 필요
    public void SetParameters(W_PortalHandler portal_inst, Vector3 target_direction)
    {
        m_b_ready = true;
        m_portal_inst = portal_inst;
        m_target_direction = target_direction;
    }

    private void Awake()
    {
        Assert.IsNotNull(m_fist_collider_obj);
        Skill_Q_CollisionHandler handler = m_fist_collider_obj.GetComponent<Skill_Q_CollisionHandler>();
        Assert.IsNotNull(handler);
        handler.owner_script = this;
        m_fist_collider_obj.SetActive(false);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Assert.IsNotNull(m_owner);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void LateUpdate()
    {
        //포탈 방향에서 팔이 발사되는 느낌
        if (m_portal_inst)
        {
            m_upper_arm.transform.position = m_portal_inst.transform.position;
            m_upper_arm.transform.rotation = Quaternion.LookRotation(m_portal_inst.transform.forward);
        }
    }

    //Q 스킬 시전 시 호출 됨
    private void OnEnable()
    {
        Assert.IsTrue(m_b_ready);

        if (m_portal_inst)
        {
            m_portal_inst.DelayDestroy(true);

            m_owner.blue_portal_on_arm.SetActive(false);
            m_casted_blue_portal_inst = Instantiate(m_owner.blue_portal_prefab, this.transform);
            m_casted_blue_portal_inst.transform.localPosition = new Vector3(0, 1, 0.5f);
            m_casted_blue_portal_inst.transform.localScale = new Vector3(2, 2, 2);

            //포탈의 방향을 지정 방향으로 회전시킨다.
            Vector3 pos = m_portal_inst.transform.position;
            pos.y = 0;
            m_target_direction.y = 0;
            Vector3 dir = m_target_direction - pos;
            m_portal_inst.transform.rotation = Quaternion.LookRotation(dir);
        }

        if (m_upper_arm)
        {
            m_upper_arm.transform.localScale = Vector3.one * 2;
        }
        if(m_lower_arm)
        {
            m_lower_arm.transform.localScale = Vector3.one * 2;
        }

        m_fist_collider_obj.SetActive(true);
    }

    //Q 스킬 종료 시 호출됨
    private void OnDisable()
    {
        m_fist_collider_obj.SetActive(false);

        if (m_upper_arm)
        {
            m_upper_arm.transform.localScale = Vector3.one;
        }
        if (m_lower_arm)
        {
            m_lower_arm.transform.localScale = Vector3.one;
        }
        
        if (m_portal_inst)
        {
            m_portal_inst.DelayDestroy(false);
        }

        if(m_casted_blue_portal_inst)
        {
            Destroy(m_casted_blue_portal_inst);
            m_casted_blue_portal_inst = null;
            m_owner.blue_portal_on_arm.SetActive(true);
        }

        m_b_ready = false;
    }
       
    //주먹 쪽에서 적 감지시 이 함수가 호출
    public void EnemyHit(Collider collider)
    {
        Debug.Log("Enemy grabbed!!");

        EnemyMove enemy_move = collider.gameObject.GetComponent<EnemyMove>();
        if (enemy_move)
        {
            enemy_move.TransitionState(eEnemyState.Supperessed);

            //이 스크립트는 Disable 될 예정이므로,
            //Disable 되지 않는 주인 Script에 코루틴 함수를 보낸다.
            m_owner.StartCoroutine(PullEnemyTowardPlayer(collider.gameObject));
        }
    }

    public IEnumerator PullEnemyTowardPlayer(GameObject obj)
    {
        //도착점: 플레이어 앞
        Transform root_transform = transform.root.transform;
        Vector3 dest = root_transform.position;
        dest += (root_transform.forward * 0.5f);

        float acc_time = 0f;

        while (acc_time < m_max_grab_time)
        {
            acc_time += Time.deltaTime;

            //출발점: obj 위치
            Vector3 src = obj.transform.position;

            Vector3 dir = dest - src;
            dir.y = 0;
            float dist = dir.magnitude;

            if (dist <= 0.01f) { break; }

            dir.Normalize();
            obj.transform.position += (dir * m_grab_pull_speed * Time.deltaTime);

            yield return null;
        }
        obj.transform.position = dest;

        //그랩 끝나면 제압상태 해제
        EnemyMove enemy_script = obj.GetComponent<EnemyMove>();
        if (enemy_script)
        {
            enemy_script.TransitionState(eEnemyState.Idle);
        }
    }
}
