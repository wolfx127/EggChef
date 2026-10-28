using Framework;
using Play.Hand;
using SO.Play;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Play
{
    public class PlayCamera
    {
        private PlayCameraSO cameraSO;

        private float speed_upNdown => cameraSO.speed_upNdown;
        private float speed_leftNright => cameraSO.speed_leftNright;
        private float leftRange => cameraSO.leftRange;
        private float rightRange => cameraSO.rightRange;
        private float UpRange => cameraSO.UpRange;
        private float DownRange => cameraSO.DownRange;

        private float beginningX = 0;
        private float beginningY = 0;
        private float curX = 0;
        private float curY = 0;

        private Camera cam;
        private GameObject leftHandGO;
        private GameObject rightHandGO;
        private float radius = 0;
        private float horizontalGap_Percent = 0;
        private float verticalGap_Percent = 0;
        private GameObject catchingHandGO = null;
        private Side isCatch = Side.NULL;
        
        public PlayCamera(GameObject cameraGO, Camera cam, PlayCameraSO cameraSO, GameObject leftHandGO, GameObject rightHandGO)
        {
            Debug.Log("初始化：PlayCamera构造");
            cameraGO.transform.rotation = Quaternion.Euler(beginningY, beginningX, 0f);
            this.cameraSO = cameraSO;
            this.cam = cam;
            beginningX = cameraSO.beginningX;
            beginningY = cameraSO.beginningY;
            radius = cameraSO.radius;
            horizontalGap_Percent = cameraSO.horizontalGap_Percent;
            verticalGap_Percent = cameraSO.verticalGap_Percent;
            this.leftHandGO = leftHandGO;
            this.rightHandGO = rightHandGO;
        }
        
        public void MoveCamera(GameObject cameraGO, PlayInputAction inputAction, float dt)
        {
            Vector2 v = inputAction.map.Rotation.ReadValue<Vector2>();

            curX = Mathf.Clamp(curX + v.x * speed_leftNright * dt, leftRange, rightRange);
            curY = Mathf.Clamp(curY - v.y * speed_upNdown * dt, UpRange, DownRange); 

            cameraGO.transform.rotation = Quaternion.Euler(beginningY + curY, beginningX + curX, 0f);

            Vector3 camPos = cameraGO.transform.position;
            Vector3 leftHandPos = camPos +
                                  cam.ViewportPointToRay(
                                      new Vector3(horizontalGap_Percent, verticalGap_Percent, 0f)).direction *
                                  radius;
            Vector3 rightHandPos = camPos +
                                   cam.ViewportPointToRay(new Vector3(1f - horizontalGap_Percent,
                                       verticalGap_Percent, 0f)).direction * radius;

            EventBus.Publish(new HandMoveByCameraEvent { leftV = leftHandPos, rightV = rightHandPos });

        }

        public void CatchingHand(Mouse mouse)
        {
            if (mouse.leftButton.isPressed)
            {
                Ray mouseRay = cam.ScreenPointToRay(mouse.position.ReadValue());
                Vector3 lV = new Vector3();
                Vector3 rV = new Vector3();
                if (isCatch == Side.NULL)
                {
                    catchingHandGO = null;
                    if (Physics.Raycast(mouseRay, out RaycastHit hit, radius * 1.1f))
                    {
                        catchingHandGO = hit.collider.gameObject;
                    }
                }
                if (catchingHandGO)
                {
                    if (catchingHandGO.CompareTag("LeftHand"))
                    {
                        isCatch = Side.left;
                        lV = mouseRay.GetPoint(radius);
                        rV = rightHandGO.transform.position;
                        EventBus.Publish(new HandMoveByMouseEvent{
                            side = Side.left,
                            leftV = lV,
                            rightV = rV
                        });
                    }
                    else if (catchingHandGO.CompareTag("RightHand"))
                    {
                        isCatch = Side.right;
                        lV = leftHandGO.transform.position;
                        rV = mouseRay.GetPoint(radius);
                        EventBus.Publish(new HandMoveByMouseEvent{
                            side = Side.right,
                            leftV = lV,
                            rightV = rV
                        });
                    }
                }
            }
            else
            {
                isCatch = Side.NULL;
                if(catchingHandGO && catchingHandGO.CompareTag("LeftHand"))
                    EventBus.Publish(new HandFreeEvent{side = Side.left});
                else if(catchingHandGO && catchingHandGO.CompareTag("RightHand"))
                    EventBus.Publish(new HandFreeEvent{side = Side.right});
                catchingHandGO = null;
            }
        }
    }
}
