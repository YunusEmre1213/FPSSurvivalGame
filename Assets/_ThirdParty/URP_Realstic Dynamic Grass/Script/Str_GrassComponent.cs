using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Str_GrassComponent
{
    public GameObject m_GrassChild;
    public Material m_LitMat, m_UnlitMat;
    //Only fill this in Grass LOD0 which can interact
    [Header("Only fill this in Grass LOD0 which can interact")]
    public Material m_LitMatInteract;
    public Material m_UnlitMatInteract;
}
