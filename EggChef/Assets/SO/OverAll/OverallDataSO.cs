using UnityEngine;

[CreateAssetMenu(fileName = "OverallDataSO", menuName = "OverallSO/OverallDataSO")]
public class OverallDataSO : ScriptableObject
{
    /// <summary>
    /// 总资金
    /// </summary>
    public float money = 0;
    /// <summary>
    /// 鸡蛋数
    /// </summary>
    public int eggCount = 0;
    /// <summary>
    /// 鸡蛋数上限
    /// </summary>
    public int eggMaxCount = 0;
    /// <summary>
    /// 油量
    /// </summary>
    public float oilCount = 0;
    /// <summary>
    /// 油量上限
    /// </summary>
    public float oilMaxCount = 0;
    /// <summary>
    /// 等级
    /// </summary>
    public float Grade = 0;
    /// <summary>
    /// 房租
    /// </summary>
    public float rent = 0;
    /// <summary>
    /// 煎蛋速度
    /// </summary>
    public float cookSpeed = 0;
    /// <summary>
    /// 煎蛋速度上限
    /// </summary>
    public float cookMaxSpeed = 0;
}
