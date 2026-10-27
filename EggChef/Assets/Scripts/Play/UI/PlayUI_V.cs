using UnityEngine;
using UnityEngine.UIElements;

namespace Play.UI
{
    public interface IPlayUI_V
    {
        public void Init(IPlayUI_P playUI_P, VisualElement root);
        public void Refresh();
    }

    public class PlayUI_V : IPlayUI_V
    {
        private IPlayUI_P playUI_P;

        //v层只能持有ui相关字段（元素、颜色、字符串、图片）
        #region UI元素字段
        
        private VisualElement root;
        
        

        #endregion

        //V不用构造是为了拓扑序，注入数据时先p再v，不然同时构造注入数据的话，总会有一方拿不到带数据的实例
        public void Init(IPlayUI_P playUI_P, VisualElement root)
        {
            Debug.Log("初始化：PlayUI_V");
            this.playUI_P = playUI_P;
            this.root = root;
            CreateUI();
        }

        //对UI进行代码上的修补
        private void CreateUI()
        {
            
        }

        public void Refresh()
        {
            
        }
    }
}
