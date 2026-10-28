using UnityEngine;

namespace Play.UI
{
    public interface IPlayUI_P
    {
        public void Subscribe();
        public void Unsubscribe();
    }

    //负责m和v之间的交互，m和v之间不能直接调用，必须先调用p
    public class PlayUI_P : IPlayUI_P
    {
        private IPlayUI_V playUI_V;
        private PlayHandler playHandler;
        //p层不持有数据，只持有v和m层

        public PlayUI_P(IPlayUI_V playUI_V, PlayHandler playHandler)
        {
            Debug.Log("初始化：PlayUI_P构造");
            this.playUI_V = playUI_V;
            this.playHandler = playHandler;
        }
        
        public void Subscribe()
        {
            if (!playHandler)
            {
                Debug.Log("playHandler获取失败：PlayUI_P订阅");
                return;
            }
            playHandler.RefreshUIEvent += RefreshUI;
        }

        public void Unsubscribe()
        {
            if (!playHandler)
            {
                Debug.Log("playHandler获取失败：PlayUI_P注销");
                return;
            }
        }

        #region 来自m层输入
        
        private void RefreshUI()
        {
            playUI_V.Refresh();
        }

        #endregion
    }
}
