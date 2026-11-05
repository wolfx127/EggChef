using UnityEngine;
using UnityEngine.UIElements;

namespace Play.UI
{
    public interface IPlayUI_V
    {
        public void Init(IPlayUI_P playUI_P, VisualElement root);
        public void UpdateMoney(float newMoney);
    }

    public class PlayUI_V : IPlayUI_V
    {
        private IPlayUI_P playUI_P;

        //v层只能持有ui相关字段（元素、颜色、字符串、图片）
        #region UI元素字段
        
        private VisualElement root;
        private Label moneyText;
        

        #endregion

        //V不用构造是为了拓扑序，注入数据时先p再v，不然同时构造注入数据的话，总会有一方拿不到带数据的实例
        public void Init(IPlayUI_P playUI_P, VisualElement root)
        {
            Debug.Log("初始化：PlayUI_V");
            this.playUI_P = playUI_P;
            this.root = root;
            
            moneyText = root.Q<Label>("MoneyText");
            
            CreateUI();
        }

        //对UI进行代码上的修补
        private void CreateUI()
        {
            
        }

        private float prevMoney;
        public void UpdateMoney(float newMoney)
        {
            float addMoney = newMoney - prevMoney;
            //TODO:播放一段ui动画，在新钱数右侧弄个一秒的加/减号
            moneyText.text = newMoney.ToString();
        }
    }
}
