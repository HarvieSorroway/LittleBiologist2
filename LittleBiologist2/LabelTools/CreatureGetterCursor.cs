using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;

namespace LittleBiologist
{
    public class CreatureGetterCursor : LBioHUDGraphics
    {
        //参数设置
        public float DistanceThreshold => 55f;
        public float maxRectScale = 250f;

        //精灵贴图
        public FSprite testSquare;

        //基础状态变量
        public Vector2 targetPos;//矩形选取框中心坐标
        public Vector2 smoothPos;
        public Vector2 lastPos;

        public float targetScale;//矩形选取框大小
        public float smoothScale;
        public float lastScale;

        public float targetRotate;//矩形选取框旋转角度
        public float smoothRotate;
        public float lastRotate;

        public float targetAlpha;
        public float smoothAlpha;
        public float lastAlpha;

        public bool foundTarget = false;//选取框是否查找到目标
        public bool hidden = false;//是否隐藏光标

        public Creature targetCreature = null;

        public CreatureGetterCursor(LBioHUD hud,LBioLabelCanvas canvas) : base(hud,canvas,true)
        {
            targetPos = Input.mousePosition.GetVector2();
            smoothPos = targetPos;
            lastPos = targetPos;

            targetScale = 15f;
            smoothScale = targetScale;
            lastScale = targetScale;

            targetRotate = 0f;
            smoothRotate = targetRotate;
            lastRotate = targetRotate;


            //delegate on
            hud.inputModule.onClickDelegate += MouseInputControl;

            isVisible = true;
            alpha = 1f;
        }

        public override void InitSprites()
        {
            testSquare = new FSprite("pixel", true)
            {
                color = Color.white * 0.5f,
                scaleX = 15f,
                scaleY = 15f
            };
            fnodes.Add(testSquare);

            base.InitSprites();
        }

        public override void Update()
        {
            base.Update();
            if (!ShouldDrawOrUpdate) return;

            Vector2 mousePos = Input.mousePosition.GetVector2();

            Creature targetCreature = null;
            float minDist = float.MaxValue;

            //查找房间内最贴近鼠标的生物
            foreach (var updateObj in hud.cam.room.updateList)
            {
                if (!(updateObj is Creature)) continue;

                Creature current = updateObj as Creature;

                foreach (var bodychunk in current.bodyChunks)
                {
                    float currentDist = LBioExpandFunc.ManhattanDist(bodychunk.pos, mousePos + hud.cam.pos);

                    if (currentDist < DistanceThreshold && currentDist < minDist + bodychunk.rad)
                    {
                        targetCreature = current;
                        minDist = currentDist;

                        break;
                    }
                }
            }

            if (targetCreature != null)
            {
                Vector2 arve = Vector2.zero;
                foreach (var bodychunk in targetCreature.bodyChunks)
                {
                    arve += bodychunk.pos;

                }
                arve /= targetCreature.bodyChunks.Length;

                //取第一个身体区块、中间的身体区块、最后的身体区块，并计算间隔最远的两个区块，用以计算出矩形选取框的理想大小
                var fisrtChunk = targetCreature.bodyChunks[0];
                var midChunk = targetCreature.bodyChunks[Mathf.FloorToInt(targetCreature.bodyChunks.Length / 2f)];
                var lastChunk = targetCreature.bodyChunks.Last();

                float maxDist = Mathf.Max(Vector2.Distance(fisrtChunk.pos, midChunk.pos), Vector2.Distance(fisrtChunk.pos, lastChunk.pos), Vector2.Distance(midChunk.pos, lastChunk.pos));


                targetPos = arve - hud.cam.pos;
                targetScale = Mathf.Min(maxRectScale, Mathf.Max(maxDist + Mathf.Max(targetCreature.bodyChunks.Select(x => x.rad).ToArray()), 30f));
                targetRotate = -90f;

                foundTarget = true;
            }
            else
            {
                targetScale = 15f;
                targetRotate = 0f;
                targetPos = mousePos;
            }
            this.targetCreature = targetCreature;
        }

        public override void ClearSprites()
        {
            base.ClearSprites();
        }

        public override void Destroy()
        {
            hud.cursor = null;
            hud.inputModule.onClickDelegate -= MouseInputControl;
            base.Destroy();
        }

        public override void DrawSprites(float timeStacker)
        {
            base.DrawSprites(timeStacker);
            if (!ShouldDrawOrUpdate) return;

            smoothPos = Vector2.Lerp(lastPos, targetPos, foundTarget ? 0.07f : 0.5f);
            lastPos = smoothPos;
            AnchorPos = smoothPos;

            smoothScale = Mathf.Lerp(lastScale, targetScale, 0.2f);
            lastScale = smoothScale;

            smoothRotate = Mathf.Lerp(lastRotate, targetRotate, 0.2f);
            lastRotate = smoothRotate;

            smoothAlpha = Mathf.Lerp(lastAlpha, targetAlpha, 0.2f);
            lastAlpha = smoothAlpha;
            alpha = smoothAlpha;

            testSquare.SetPosition(AnchorPos.x, AnchorPos.y);
            testSquare.scale = smoothScale;
            testSquare.rotation = smoothRotate;

            testSquare.alpha = Alpha;

            foundTarget = false;
        }

        public void MouseInputControl(int button_id)
        {
            if (button_id == 0)
            {
                if (ShouldDrawOrUpdate)
                {
                    if (targetCreature != null)
                    {
                        hud.infoLabel.FocusCreature = targetCreature;
                    }
                }
            }
            if(button_id == 1)
            {
                hud.infoLabel.FocusCreature = null;
            }
        }

        public override bool IsMouseOverMe(Vector2 mousePos, bool higherPiorityAlreadyOver)
        {
            if (!ShouldDrawOrUpdate) return false;
            targetAlpha = higherPiorityAlreadyOver ? 0f : 1f;
            return false;
        }
    }
}
