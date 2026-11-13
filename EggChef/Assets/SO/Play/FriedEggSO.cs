using UnityEngine;

namespace SO.Play
{
    [CreateAssetMenu(fileName = "FriedEggSO", menuName = "PlaySO/FriedEggSO")]
    public class FriedEggSO : ScriptableObject
    {
        /// <summary>
        /// 生煎蛋的颜色
        /// </summary>
        public Color rawFriedEgg_Color = Color.white;
        /// <summary>
        /// 煎的恰到好处的颜色
        /// </summary>
        public Color perfectFriedEgg_Color = Color.sandyBrown;
        /// <summary>
        /// 煎糊的颜色
        /// </summary>
        public Color burnedFriedEgg_Color = Color.black;
    }
}
