using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraMove : MonoBehaviour
{
    public float m_move_speed = 10f;
    private Vector3 m_move_dir;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(m_move_dir != Vector3.zero)
        {
            transform.position += m_move_dir * m_move_speed * Time.deltaTime;
        }

    }
    public void OnCamMove(InputValue value)
    {
        Vector2 val = value.Get<Vector2>();

        m_move_dir.x = -val.x;
        m_move_dir.z = -val.y;
    }
}
