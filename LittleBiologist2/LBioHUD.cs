﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RWCustom;
using HUD;


namespace LittleBiologist
{
    public class LBioHUD : HudPart
    {
        public static LBioHUD hud_instance;

        public RoomCamera cam;

        public LBioLabelCanvas canvas;
        public CreatureGetterCursor cursor;
        public InfoLabel infoLabel;

        public List<LBioHUDGraphics> updateGraphics = new List<LBioHUDGraphics>();


        public LBioHUD(HUD.HUD hud, RoomCamera cam) : base(hud)
        {
            hud_instance = this;
            this.cam = cam;
            canvas = new LBioLabelCanvas(this);

            cursor = new CreatureGetterCursor(this,canvas);
            infoLabel = new InfoLabel(this,canvas);

            Plugin.Log("Inited");
            //InitSprites();
        }

        public override void Update()
        {
            try
            {
                for (int i = updateGraphics.Count - 1; i >= 0; i--)//对所有模块进行逻辑更新
                {
                    updateGraphics[i].Update();
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                Plugin.Log(e.Message);
            }
        }

        public override void Draw(float timeStacker)
        {
            base.Draw(timeStacker);

            for (int i = updateGraphics.Count - 1; i >= 0; i--)//对所有模块进行绘制
            {
                updateGraphics[i].DrawSprites(timeStacker);
            }
        }

        public override void ClearSprites()
        {
            base.ClearSprites();

            for (int i = updateGraphics.Count - 1; i >= 0; i--)
            {
                updateGraphics[i].Destroy();
            }
        }
    }

    public class LBioHUDGraphics
    {
        public LBioHUD hud;

        public LBioHUDGraphics parentGraphics;
        public List<LBioHUDGraphics> subGraphics = new List<LBioHUDGraphics>();

        public List<FNode> fnodes = new List<FNode>();

        public bool isVisible;
        public bool isDestroy = false;

        public float alpha = 1f;

        public Vector2 localPos;


        public virtual float EffectiveWidth => 50f;
        public virtual float EffectiveHeight => 20f;

        public bool initiated
        {
            get; protected set;
        }

        public Vector2 AnchorPos
        {
            get => parentGraphics == null ? localPos : localPos + parentGraphics.AnchorPos;
            set => localPos = value - (parentGraphics == null ? Vector2.zero : parentGraphics.AnchorPos);
        }

        public float Alpha
        {
            get => parentGraphics == null ? alpha : alpha * parentGraphics.Alpha;
            set => alpha = value;
        }

        public bool IsVisible
        {
            get => parentGraphics == null ? isVisible : isVisible & parentGraphics.IsVisible;
        }

        public bool ShouldDrawOrUpdate
        {
            get => initiated && IsVisible && !isDestroy;
        }

        public bool IsSubGraphic => parentGraphics != null;

        public LBioHUDGraphics(LBioHUD hud, LBioHUDGraphics parent = null)
        {
            this.hud = hud;
            parentGraphics = parent;

            if (parent != null && !parent.subGraphics.Contains(this)) parent.subGraphics.Add(this);
            this.hud.updateGraphics.Add(this);
        }

        public virtual void InitSprites()
        {
            if (initiated) return;
            if (subGraphics.Count > 0)
            {
                foreach (var graphics in subGraphics) graphics.InitSprites();
            }

            Plugin.Log(ToString() + " InitSprites");
            AddToContainer();

            initiated = true;
        }

        public virtual void AddToContainer()
        {
            if (fnodes.Count > 0)
            {
                foreach (var node in fnodes)
                {
                    node.isVisible = IsVisible;
                    hud.hud.fContainers[0].AddChild(node);
                }
            }

            Plugin.Log(ToString() + "Add to container");
        }

        public virtual void DrawSprites(float timeStacker)
        {
            if (!IsSubGraphic && !initiated)
            {
                InitSprites();
            }
            UpdateVisibility();
            if (!ShouldDrawOrUpdate) return;
        }

        public virtual void UpdateVisibility()
        {
            if (fnodes.Count > 0 && fnodes[0].isVisible == !IsVisible)
            {
                foreach (var node in fnodes)
                {
                    node.isVisible = IsVisible;
                }
            }
        }

        public virtual void Update()
        {

        }

        public virtual void Destroy()
        {
            isDestroy = true;
            if (hud.updateGraphics.Contains(this))
            {
                hud.updateGraphics.Remove(this);
            }
            ClearSprites();
            if (subGraphics.Count > 0)
            {
                foreach (var graphic in subGraphics) graphic.Destroy();
            }
            subGraphics.Clear();
        }

        public virtual void ClearSprites()
        {
            if (fnodes.Count > 0)
            {
                foreach (var node in fnodes)
                {
                    node.isVisible = false;
                    node.RemoveFromContainer();
                }
            }
            fnodes.Clear();
        }
    }

    //便于统一调控透明度
    public class LBioLabelCanvas : LBioHUDGraphics
    {
        public float targetAlpha;
        public float smoothAlpha;
        public float lastAlpha;

        public bool hidden = false;//是否隐藏光标

        public LBioLabelCanvas(LBioHUD hud) : base(hud)
        {
            Plugin.instance.mouseEventTrigger += MouseInputControl;
            isVisible = true;
        }

        public override void DrawSprites(float timeStacker)
        {
            base.DrawSprites(timeStacker);

            isVisible = alpha > 0.001f;

            targetAlpha = hidden ? 0f : 1f;
            smoothAlpha = Mathf.Lerp(lastAlpha, targetAlpha, 0.2f);
            lastAlpha = smoothAlpha;
            alpha = smoothAlpha;


            if (!ShouldDrawOrUpdate) return;
        }

        public override void Destroy()
        {
            Plugin.instance.mouseEventTrigger -= MouseInputControl;
            base.Destroy();
        }

        public void MouseInputControl(int button_id)
        {
            if (button_id == 0)
            {
                if (!hidden) hidden = false;
            }
            else if (button_id == 2)
            {
                if (!hidden)
                {
                    hidden = true;
                }
                else
                {
                    hidden = false;
                }
            }
        }
    }
}
