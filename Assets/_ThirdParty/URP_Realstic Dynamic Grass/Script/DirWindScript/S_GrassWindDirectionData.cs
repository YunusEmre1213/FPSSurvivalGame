using UnityEngine;


[CreateAssetMenu(fileName = "new GrassWindDirection", menuName = "Assets/GrassWindDirection")]
public class S_GrassWindDirectionData : ScriptableObject
{
    // Từng cặp 1, (X,Y) = North min, (Z,W) = NorthMax
    public CustomFloat2 m_North;
    public CustomFloat2 m_South;
    public CustomFloat2 m_East;
    public CustomFloat2 m_West;
    public CustomFloat2 m_NorthEast;
    public CustomFloat2 m_NorthWest;
    public CustomFloat2 m_SouthEast;
    public CustomFloat2 m_SouthWest;

}

[System.Serializable]   
public struct CustomFloat2
{
    public float _Y;
    public float _X;
}