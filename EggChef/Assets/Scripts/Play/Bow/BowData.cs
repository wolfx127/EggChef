using System.Collections.Generic;
using Framework;
using Play.Egg;
using Play.Hand;
using SO.Play;
using UnityEngine.Rendering;
using UnityEngine;

namespace Play.Bow
{
    public class BowData : MonoBehaviour
    {
        private float drawTrackBallGapDis;
        private GameObject drawTrackBallGO;
        private float minAngle_horizontal;
        private float maxAngle_horizontal;
        private float minAngle_vertical;
        private float maxAngle_vertical;
        private Vector3[] trackBallPosList = new Vector3[64];
        private int trackBallPosListBackIndex = 0;
        private float trackGroundY => 0f;
        private UniTaskLoopHandle drawTrackHandle;
        
        //渲染轨迹依赖
        private Matrix4x4[] trackBallMats = new Matrix4x4[64];
        private Mesh trackBallMesh;
        private Material trackBallMat;
        private Vector3 trackBallScale;
        private RenderParams trackBallParams;
        
        public Vector3 forward = new Vector3();
        private float speed = 12;
        private float launchHeight = 1;

        public FriedEggData friedEgg;
        public Side handSide = Side.NULL;
        public float g => -Physics.gravity.y;
        
        public void Init(BowSO so)
        {
            drawTrackBallGapDis = so.drawTrackBallGapDis;
            minAngle_horizontal = so.minDis_horizontal;
            maxAngle_horizontal = so.maxDis_horizontal;
            minAngle_vertical = so.minDis_vertical;
            maxAngle_vertical = so.maxDis_vertical;


            trackBallMesh = Resources.GetBuiltinResource<Mesh>("Sphere.fbx");
            trackBallMat = so.trackBallMat;
            if (trackBallMat) trackBallMat.enableInstancing = true;
            trackBallScale = Vector3.one * 0.1f;
            trackBallParams = new RenderParams(trackBallMat)
            {
                worldBounds = new Bounds(Vector3.zero, Vector3.one * 200f),
            };

            
            drawTrackHandle?.Stop();
        }

        public void StartDrawTrack()
        {
            drawTrackHandle?.Stop();
            drawTrackHandle = UniTaskMgr.RepeatEveryFrame(DrawTrack);
        }

        public void WriteDrawTrackField(Vector3 handPos)
        {
            forward = transform.position - handPos;
            forward = forward.normalized;
        }

        private void DrawTrack()
        {
            if (!trackBallMat || speed < 1e-4f || drawTrackBallGapDis < 1e-3f) return;

            trackBallPosListBackIndex = 0;

            for (float x = drawTrackBallGapDis; 
                 trackBallPosListBackIndex < trackBallPosList.Length; 
                 x += drawTrackBallGapDis)
            {
                Vector3 p = transform.position + Vector3.up * launchHeight + DrawDeltaPos(x);
                if (p.y <= trackGroundY) break;
                trackBallPosList[trackBallPosListBackIndex++] = p;
            }

            if (trackBallPosListBackIndex == 0) return;

            for (int i = 0; i < trackBallPosListBackIndex; i++)
                trackBallMats[i] = Matrix4x4.TRS(trackBallPosList[i], 
                    Quaternion.identity, 
                    trackBallScale);

            for (int i = 0; i < trackBallPosListBackIndex; i++)
                Graphics.DrawMesh(trackBallMesh, trackBallMats[i], trackBallMat, 0);
        }

        public void StopDrawTrack()
        {
            drawTrackHandle?.Stop();
            drawTrackHandle = null;
        }

        //抛物线方程，输入相对于朝向的横坐标，返回对应的世界坐标
        private Vector3 DrawDeltaPos(float X)
        {
            float g = 9.81f;

            Vector3 dir = forward.normalized;
            Vector3 horizontal = new Vector3(dir.x, 0f, dir.z);
            float cosA = horizontal.magnitude;

            if (cosA < 1e-4f || speed < 1e-4f) return Vector3.zero;

            float tanA = dir.y / cosA;
            float y = X * tanA - g * X * X * (1f + tanA * tanA) / (2f * speed * speed);

            return horizontal / cosA * X + Vector3.up * y;
        }

        public void LaunchFriedEgg()
        {
            if (!friedEgg)
                return;
            if (friedEgg.handSide != Side.NULL)
            {
                friedEgg = null;
                return;
            }

            friedEgg.transform.parent = null;
            friedEgg.Launch(forward, speed);
            friedEgg = null;
        }

        public void DeleteFriedEgg(FriedEggData data)
        {
            if (friedEgg && data.Equals(friedEgg))
            {
                friedEgg.transform.parent = null;
                friedEgg = null;
            }
        }

        public void PutEgg(FriedEggData friedEgg)
        {
            this.friedEgg = friedEgg;
            friedEgg.transform.parent = transform;
            friedEgg.transform.localPosition = new Vector3(0, launchHeight, 0);
        }
    }
}
