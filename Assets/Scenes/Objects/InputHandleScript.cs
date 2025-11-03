using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandleScript : MonoBehaviour
{

    [SerializeField] private GameObject m_player;   //지정 필요
    private PlayerMove m_player_move_script;

    private Camera m_camera;
    private CameraMove m_camera_move_script;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_camera = Camera.main;
        m_camera_move_script = m_camera.GetComponent<CameraMove>();
        if (null == m_camera_move_script)
        {
            Debug.Log("Camera Move Script 찾지 못함.");
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
        if(m_player_move_script)
        {
            m_player_move_script.OnMove();
        }
    }

    void OnSetTarget()
    {
        if(m_player_move_script)
        {
            m_player_move_script.OnSetTarget();
        }
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
