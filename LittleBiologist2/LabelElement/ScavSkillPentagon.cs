using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static LittleBiologist.LBioHUD;


namespace LittleBiologist
{
    public class ScavSkillPentagon : LBioHUDGraphics
    {
        string[] abilityName;
        float[] abilityValue;

        Color color;
        float rad;

        public TriangleMesh backgroundMesh;
        public TriangleMesh valueMesh;

        public FLabel[] nameLabels;
        public FLabel[] valueLabels;

        public Vector2[] backgroundGridPos;
        public Vector2[] abilityGridPos;
        public Vector2 lastAnchorPos = Vector2.zero;

        public float targetValueAlpha;
        public float smoothValueAlpha;
        public float lastValueAlpha;
        public float PentagonAngle => Mathf.PI * 2f / 5f;

        public bool sideFeatureUpdating = false;

        public ScavSkillPentagon(LBioHUD part, LBioHUDGraphics forwardModule, string[] abilityName, float[] abilityValue, Color color, float rad = 50f) : base(part, forwardModule)
        {
            this.abilityName = abilityName;
            this.abilityValue = abilityValue;
            this.color = color;
            this.rad = rad;

            backgroundGridPos = new Vector2[5];
            abilityGridPos = new Vector2[5];
            for (int i = 0; i < 5; i++)
            {
                backgroundGridPos[i] = new Vector2(rad * Mathf.Cos(Mathf.PI / 2 - PentagonAngle * i), rad * Mathf.Sin(Mathf.PI / 2 - PentagonAngle * i));
                abilityGridPos[i] = Vector2.Lerp(Vector2.zero, backgroundGridPos[i], Mathf.Max(abilityValue[i], 0.05f));
            }
        }

        public override void InitSprites()
        {
            TriangleMesh.Triangle[] mesh0 = new TriangleMesh.Triangle[5]
            {
                new TriangleMesh.Triangle(0,1,2),
                new TriangleMesh.Triangle(0,2,3),
                new TriangleMesh.Triangle(0,3,4),
                new TriangleMesh.Triangle(0,4,5),
                new TriangleMesh.Triangle(0,5,1)
            };

            backgroundMesh = new TriangleMesh("Futile_White", mesh0, true, false)
            {
                color = Color.white
            };
            valueMesh = new TriangleMesh("Futile_White", mesh0, true, false)
            {
                color = this.color
            };

            fnodes.Add(backgroundMesh);
            fnodes.Add(valueMesh);


            nameLabels = new FLabel[5];
            valueLabels = new FLabel[5];

            for (int i = 0; i < 5; i++)
            {
                nameLabels[i] = new FLabel(Custom.GetFont(), abilityName[i]) { scale = 1.05f };
                valueLabels[i] = new FLabel(Custom.GetFont(), String.Format("{0:f3}", abilityValue[i]));

                fnodes.Add(nameLabels[i]);
                fnodes.Add(valueLabels[i]);
            }

            targetValueAlpha = 0f;
            smoothValueAlpha = 0f;
            lastValueAlpha = 0f;

            base.InitSprites();
        }

        public override void DrawSprites(float timeStacker)
        {
            base.DrawSprites(timeStacker);
            if (!ShouldDrawOrUpdate) return;

            for (int i = 0; i < 6; i++)
            {
                backgroundMesh.verticeColors[i].a = Alpha * 0.3f;
                valueMesh.verticeColors[i].a = Alpha * 0.6f;
            }
            for (int i = 0; i < 5; i++)
            {
                valueLabels[i].alpha = smoothValueAlpha;
            }

            if (smoothValueAlpha != 0f || lastAnchorPos != AnchorPos)
            {
                nameLabels[0].SetPosition(backgroundMesh.vertices[1] + (nameLabels[0].textRect.height * (0.5f + smoothValueAlpha) + 5f) * Vector2.up);//为了做出上升效果，这里需要单独移动它
                for (int i = 1; i < 5; i++)
                {
                    valueLabels[i].SetPosition(nameLabels[i].x, nameLabels[i].y + smoothValueAlpha * -(valueLabels[i].textRect.height + 5f));
                }
            }

            if (lastAnchorPos == AnchorPos) return;

            backgroundMesh.MoveVertice(0, AnchorPos);
            valueMesh.MoveVertice(0, AnchorPos);
            for (int i = 1; i < 6; i++)
            {
                backgroundMesh.MoveVertice(i, AnchorPos + backgroundGridPos[i - 1]);
                valueMesh.MoveVertice(i, AnchorPos + abilityGridPos[i - 1]);
            }

            nameLabels[0].SetPosition(backgroundMesh.vertices[1] + (nameLabels[0].textRect.height * (0.5f + smoothValueAlpha) + 5f) * Vector2.up);

            nameLabels[1].SetPosition(backgroundMesh.vertices[2] + (nameLabels[1].textRect.width / 2f + 8f) * Vector2.right);
            nameLabels[2].SetPosition(backgroundMesh.vertices[3] + (nameLabels[2].textRect.width / 2f + 8f) * Vector2.right);

            nameLabels[3].SetPosition(backgroundMesh.vertices[4] + (nameLabels[3].textRect.width / 2f + 8f) * Vector2.left);
            nameLabels[4].SetPosition(backgroundMesh.vertices[5] + (nameLabels[4].textRect.width / 2f + 8f) * Vector2.left);

            valueLabels[0].SetPosition(backgroundMesh.vertices[1] + (nameLabels[0].textRect.height / 2f + 5f) * Vector2.up);//这个值标签不移动，所以只需要在改变锚点的时候移动它就好了

            lastAnchorPos = AnchorPos;
        }

        public override void Update()
        {
            base.Update();
            if (!ShouldDrawOrUpdate) return;
            if (!IsVisible) return;

            targetValueAlpha = sideFeatureUpdating ? 1f : 0f;

            if (smoothValueAlpha == targetValueAlpha) return;

            if (Mathf.Abs(smoothValueAlpha - targetValueAlpha) < 0.001f)
            {
                smoothValueAlpha = targetValueAlpha;
                lastValueAlpha = targetValueAlpha;
                return;
            }

            smoothValueAlpha = Mathf.Lerp(lastValueAlpha, targetValueAlpha, 0.15f);
            lastValueAlpha = smoothValueAlpha;
        }

        public override void SideFeatureUpdate(bool isMouseButtonRightHolding)
        {
            sideFeatureUpdating = isMouseButtonRightHolding;
        }
    }

}
