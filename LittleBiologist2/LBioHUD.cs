using System;
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
        public CreaturePointer pointer;
        public InfoLabel infoLabel;
        public InputModule inputModule;

        public List<LBioHUDGraphics> updateGraphics = new List<LBioHUDGraphics>();


        public LBioHUD(HUD.HUD hud, RoomCamera cam) : base(hud)
        {
            hud_instance = this;
            inputModule = new InputModule();
            this.cam = cam;

            canvas = new LBioLabelCanvas(this);           
            cursor = new CreatureGetterCursor(this,canvas);
            infoLabel = new InfoLabel(this,canvas);
            pointer = new CreaturePointer(this, canvas);

            Plugin.Log("Inited");
            //InitSprites();
        }

        public override void Update()
        {
            try
            {
                inputModule.Update();
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

    /// <summary>
    /// HUD渲染的基础单位
    /// </summary>
    public class LBioHUDGraphics
    {
        public LBioHUD hud;

        public LBioHUDGraphics parentGraphics;
        public List<LBioHUDGraphics> subGraphics = new List<LBioHUDGraphics>();

        public List<FNode> fnodes = new List<FNode>();

        public bool isVisible;
        public bool isDestroy = false;

        public readonly bool acceptInputControl = true;
        public readonly int inputControlPiority = -1;

        /// <summary>
        /// 本地透明度
        /// </summary>
        public float alpha = 1f;

        /// <summary>
        ///本地坐标
        /// </summary>
        public Vector2 localPos;

        public virtual float EffectiveWidth => 50f;
        public virtual float EffectiveHeight => 20f;

        public bool initiated
        {
            get; protected set;
        }

        /// <summary>
        /// 基于父子关系的基坐标
        /// </summary>
        public Vector2 AnchorPos
        {
            get => parentGraphics == null ? localPos : localPos + parentGraphics.AnchorPos;
            set => localPos = value - (parentGraphics == null ? Vector2.zero : parentGraphics.AnchorPos);
        }

        /// <summary>
        /// 基于父子关系的透明度
        /// </summary>
        public float Alpha
        {
            get => parentGraphics == null ? alpha : alpha * parentGraphics.Alpha;
            set => alpha = value;
        }

        /// <summary>
        /// 该HUDGraphics是否可见，基于父子关系
        /// </summary>
        public bool IsVisible
        {
            get => parentGraphics == null ? isVisible : isVisible & parentGraphics.IsVisible;
        }

        /// <summary>
        /// 该HUDGraphics是否应当进行更新和渲染
        /// </summary>
        public bool ShouldDrawOrUpdate
        {
            get => initiated && IsVisible && !isDestroy;
        }

        public bool IsSubGraphic => parentGraphics != null;

        /// <summary>
        /// 注意，如果parent不为null，则InitSprites方法会跟随parent调用。如果初始化在parent之后，请手动调用InitSprites
        /// isVisble默认为false
        /// </summary>
        /// <param name="hud"></param>
        /// <param name="parent"></param>
        public LBioHUDGraphics(LBioHUD hud, LBioHUDGraphics parent = null, bool acceptInputControl = true)
        {
            this.hud = hud;
            this.acceptInputControl = acceptInputControl;
            parentGraphics = parent;

            if (parent != null && !parent.subGraphics.Contains(this)) parent.subGraphics.Add(this);
            this.hud.updateGraphics.Add(this);

            if (acceptInputControl) inputControlPiority = (parent == null ? 0 :(parent.acceptInputControl ? parent.inputControlPiority + 1 : 0));

            hud.inputModule.AddToLayer(this);
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

        /// <summary>
        /// 更新所有节点的可见信息，基于该HUDGraphics的IsVisible信息，如果你有特殊的需求，请重写该方法。
        /// </summary>
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
                for(int i = subGraphics.Count - 1;i >= 0; i--)
                {
                    subGraphics[i].Destroy();
                }
            }
            subGraphics.Clear();
            if(parentGraphics != null)
            {
                parentGraphics.subGraphics.Remove(this);
            }
            if(hud.inputModule != null && acceptInputControl)
            {
                hud.inputModule.layersAndGraphics[inputControlPiority].Remove(this);
            }
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

        #region InputControls
        public virtual bool IsMouseOverMe(Vector2 mousePos,bool higherPiorityAlreadyOver)
        {
            return false;
        }

        public virtual void ClickOnMe(int mouseButton)
        {
        }

        public virtual void DragUpdate(Vector2 startDragPos,Vector2 currentDragPos)
        {
        }

        public virtual void SideFeatureUpdate(bool isMouseButtonRightHolding)
        {

        }
        #endregion
    }

    //便于统一调控透明度
    public class LBioLabelCanvas : LBioHUDGraphics
    {
        public float targetAlpha;
        public float smoothAlpha;
        public float lastAlpha;

        public bool hidden = false;//是否隐藏光标

        public LBioLabelCanvas(LBioHUD hud) : base(hud,null,false)
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
