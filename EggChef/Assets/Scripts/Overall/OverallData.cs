

using UnityEngine;

namespace Overall
{
    //永久数据
    public static class OverallData
    {
        /// <summary>
        /// 资金
        /// </summary>
        public static float money = 0;
        /// <summary>
        /// 鸡蛋数
        /// </summary>
        public static int eggCount = 0;
        /// <summary>
        /// 鸡蛋数上限
        /// </summary>
        public static int eggMaxCount = 0;
        /// <summary>
        /// 油量
        /// </summary>
        public static float oilCount = 0;
        /// <summary>
        /// 油量上限
        /// </summary>
        public static float oilMaxCount = 0;
        /// <summary>
        /// 等级
        /// </summary>
        public static float Grade = 0;
        /// <summary>
        /// 房租
        /// </summary>
        public static float rent = 0;
        /// <summary>
        /// 煎蛋速度
        /// </summary>
        public static float cookSpeed = 0;
        /// <summary>
        /// 煎蛋速度上限
        /// </summary>
        public static float cookMaxSpeed = 0;
        /// <summary>
        /// 生煎蛋的颜色
        /// </summary>
        public static Color rawFriedEgg_Color = Color.white;
        /// <summary>
        /// 煎的恰到好处的颜色
        /// </summary>
        public static Color perfectFriedEgg_Color = Color.sandyBrown;
        /// <summary>
        /// 煎糊的颜色
        /// </summary>
        public static Color burnedFriedEgg_Color = Color.black;

        public static void CopyFieldsBySO(OverallDataSO so)
        {
            money = so.money;   
            eggCount = so.eggCount;
            eggMaxCount = so.eggMaxCount;
            oilCount = so.oilCount;
            oilMaxCount = so.oilMaxCount;
            Grade = so.Grade;
            rent = so.rent;
            cookSpeed = so.cookSpeed;
            cookMaxSpeed = so.cookMaxSpeed;
            rawFriedEgg_Color = so.rawFriedEgg_Color;
            perfectFriedEgg_Color = so.perfectFriedEgg_Color;
            burnedFriedEgg_Color = so.burnedFriedEgg_Color;
        }
    }
}
