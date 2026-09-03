using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomLODGroup : MonoBehaviour
{
    //public string Name;
    public GameObject[] m_NeedDisableObj;
    private void OnBecameVisible()
    {
        for (int i = 0; i < m_NeedDisableObj.Length; i++)
        {
            
            m_NeedDisableObj[i].SetActive(true);
        }
    }

    private void OnBecameInvisible()
    {
        for (int i = 0; i < m_NeedDisableObj.Length; i++)
        {
            m_NeedDisableObj[i].SetActive(false);
        }
    }


}
