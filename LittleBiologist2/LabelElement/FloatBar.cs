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
    public class FloatBar : LBioHUDGraphics
    {
        public string name;

        float focusValue = 0f;
        public float FocusValue
        {
            get => focusValue;
            set
            {
                if (value != focusValue)
                {
                    focusValue = value;
                    if (valueLabel == null) return;
                    valueLabel.text = String.Format("{0:f3}", value);
                }
            }
        }
        float nameLabelWidth = -1;
        public float NameLabelWidth
        {
            get
            {
                if (nameLabelWidth == -1 && nameLabel != null)
                {
                    nameLabelWidth = nameLabel.textRect.width + 10f;//10f的间隙，防止两个label靠的太近
                }
                return nameLabelWidth;
            }
        }

        public float width;
        public Color color;

        public float rangeMin;
        public float rangeMax;

        public float Height => 15f;

        public FSprite background;
        public FSprite bar;

        public FLabel nameLabel;
        public FLabel valueLabel;

        float targetValueAlpha = 1f;
        float smoothValueAlpha = 0f;
        float lastValueAlpha = 0f;

        float forceRevalProgress;

        public FloatBar(LBioHUD part, LBioHUDGraphics forwardModule, Vector2 range, Color color, float width = 50f, string name = "defaultName") : base(part, forwardModule)
        {
            this.name = name;
            this.width = width;
            this.color = color;

            rangeMin = range.x;
            rangeMax = range.y;
            alpha = 1f;
        }

        public override void InitSprites()
        {
            Plugin.Log("Init FloarBar : " + name + " " + FocusValue.ToString());
            background = new FSprite("pixel", true)//锚点定位到左上角
            {
                scaleX = width,
                scaleY = Height,

                anchorX = 0f,
                anchorY = 1f,

                color = Color.black * 0.5f
            };
            bar = new FSprite("pixel", true)
            {
                scaleX = width,
                scaleY = Height,

                anchorX = 0f,
                anchorY = 1f,

                color = color
            };

            nameLabel = new FLabel(Custom.GetFont(), name)
            {
                anchorX = 0f,
                anchorY = 1f,
                scale = 1.02f
            };
            valueLabel = new FLabel(Custom.GetFont(), String.Format("{0:f3}", FocusValue))
            {
                anchorX = 0f,
                anchorY = 1f,
            };

            fnodes.Add(background);
            fnodes.Add(bar);

            fnodes.Add(nameLabel);
            fnodes.Add(valueLabel);

            base.InitSprites();
        }

        public override void DrawSprites(float timeStacker)
        {
            base.DrawSprites(timeStacker);
            if (!ShouldDrawOrUpdate) return;
           
            background.x = AnchorPos.x;
            background.y = AnchorPos.y;
            background.alpha = Alpha * 0.5f;

            bar.x = AnchorPos.x;
            bar.y = AnchorPos.y;
            bar.scaleX = width * Mathf.InverseLerp(rangeMin, rangeMax, FocusValue);
            bar.alpha = Alpha;

            nameLabel.x = AnchorPos.x;
            nameLabel.y = AnchorPos.y;
            nameLabel.alpha = (1f - smoothValueAlpha) * Alpha;

            valueLabel.x = AnchorPos.x + forceRevalProgress * NameLabelWidth;
            valueLabel.y = AnchorPos.y;
            valueLabel.alpha = (smoothValueAlpha + forceRevalProgress) * Alpha;
        }

        public override void Update()
        {
            base.Update();
            if (!ShouldDrawOrUpdate) return;

            Vector2 mouseLocalPos = background.GetLocalMousePosition();
            bool revalValueLabel = mouseLocalPos.x < 1 && mouseLocalPos.x > 0 && mouseLocalPos.y < 0 && mouseLocalPos.y > -1;//y 在-1到0之间是实测结果，具体为什么我也不知道.jpg

            targetValueAlpha = revalValueLabel ? 1f : 0f;
            smoothValueAlpha = Mathf.Lerp(lastValueAlpha, targetValueAlpha, 0.15f);
            lastValueAlpha = smoothValueAlpha;

            forceRevalProgress = Mathf.Lerp(forceRevalProgress, Input.GetMouseButton(1) ? 1f : 0f, 0.15f);
        }
    }
}
