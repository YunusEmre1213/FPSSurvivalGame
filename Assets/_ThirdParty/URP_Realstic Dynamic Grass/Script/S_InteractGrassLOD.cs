using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_InteractGrassLOD : MonoBehaviour
{
    public Material m_MatWindInteract;


    private void Awake()
    {
        if(m_MatWindInteract == null)
        {
            m_MatWindInteract = GetComponentInParent<MeshRenderer>().sharedMaterial;
        }
    }
    public void Func_TrigerEnterWithPlayer(Vector3 _Playerpos)
    {
        m_MatWindInteract.SetFloat("_IsPlayerInteract", 1);
        m_MatWindInteract.SetVector("_pos", _Playerpos);
    }

    public void Func_TriggerStayWithPlayer(Vector3 _Playerpos)
    {
        m_MatWindInteract.SetVector("_pos", _Playerpos);
    }

    public void Func_OnTriggerExitWithPlayer()
    {
        m_MatWindInteract.SetFloat("_IsPlayerInteract", 0);
    }


}
