using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandleScript : MonoBehaviour
{
    [SerializeField] private LayerMask  m_ground_mask;   //지정 필요
    [SerializeField] private Camera     m_camera;   //지정 필요
    private CameraMove m_camera_move_script;

    [SerializeField] private GameObject m_player;   //지정 필요
    private PlayerMove m_player_move_script;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(m_camera)
        {
            m_camera_move_script = m_camera.GetComponent<CameraMove>();
            if(null == m_camera_move_script)
            {
                Debug.Log("Camera Move Script 찾지 못함.");
            }
        }

        if(m_player)
        {
            m_player_move_script = m_player.GetComponent<PlayerMove>();
            if (null == m_player_move_script)
            {
                Debug.Log("Player Move Script 찾지 못함.");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnCamMove(InputValue value)
    {
        if(m_camera_move_script)
        {
            m_camera_move_script.OnCamMove(value);
        }
    }

    void OnMove()
    {
        if (m_camera)
        {
            Ray ray = m_camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, m_ground_mask))
            {
                Debug.Log("우클릭한 Plane의 위치: " + hit.point);
                m_player_move_script.MoveTo(hit.point);
            }
        }
    }

    void OnSetTarget()
    {

    }

    void OnSkill_Q()
    {
        m_player_move_script.ToggleRangeQ();
    }

    void OnSkill_W()
    {

    }

    void OnSkill_E()
    {

    }

    void OnSkill_R()
    {

    }
}
