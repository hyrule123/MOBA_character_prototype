using NUnit.Framework;
using UnityEngine;

public class Skill_Q_Handler : MonoBehaviour
{
    [SerializeField] private GameObject m_upper_arm;
    [SerializeField] private GameObject m_lower_arm;

    private W_PortalHandler m_portal_inst;
    public void SetParameters()
    {

    }

    private void OnEnable()
    {
        if(m_upper_arm)
        {
            m_upper_arm.transform.localScale = Vector3.one * 2;
        }
        if(m_lower_arm)
        {
            m_lower_arm.transform.localScale = Vector3.one * 2;
        }
    }

    private void OnDisable()
    {
        if (m_upper_arm)
        {
            m_upper_arm.transform.localScale = Vector3.one;
        }
        if (m_lower_arm)
        {
            m_lower_arm.transform.localScale = Vector3.one;
        }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
