using Overall;
using SO.Play;
using UnityEngine;

namespace Play
{
    public class PlayTimeManager
    {
        private float sumTime = 0;
        public float curTime = 0;
        
        //TODO:暂停游戏

        public PlayTimeManager(PlayTimeManagerSO so)
        {
            sumTime = so.sumTime;
        }

        public void Tick(float dt)
        {
            curTime += dt;
            if(sumTime <= curTime)
                FinishDay();
        }

        private void FinishDay()
        {
            //TODO:暂停游戏 + 结算窗口ui
        }

        /// <summary>
        /// 给ui按钮用
        /// </summary>
        private void IntoSettleScene()
        {
            OverallSceneMgr.Instance.ChangeScene("SettleScene", "PlayScene");
        }
    }
}
