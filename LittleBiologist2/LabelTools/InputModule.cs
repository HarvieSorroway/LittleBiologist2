using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LittleBiologist
{
    public class InputModule
    {
        public readonly float clickToDragTimeSpan = 0.2f;

        public List<List<LBioHUDGraphics>> layersAndGraphics = new List<List<LBioHUDGraphics>>();

        public OnClick onClickDelegate;

        //标志变量
        public bool lastMouseButtonDown = false;
        public bool currentMouseButtonDown = true;
        public bool lastDragUpdate = false;

        public int currentMouseButton = 0;

        public float mouseDownTime = 0f;

        public Vector2 mouseDownPos;
        public Vector2 currentMousePos;

        public LBioHUDGraphics dragFocusGraphics = null;

        public void AddToLayer(LBioHUDGraphics graphics)
        {
            if (!graphics.acceptInputControl || graphics.inputControlPiority == -1) return;
            for(int i = 0;i < graphics.inputControlPiority + 1 - layersAndGraphics.Count; i++)//添加层级
            {
                layersAndGraphics.Add(new List<LBioHUDGraphics>());
            }
            layersAndGraphics[graphics.inputControlPiority].Add(graphics);
        }

        public void Update()
        {
            bool dragUpdate = false;
            bool clickUpdate = false;
            bool sideFeatureUpdate = false;

            currentMousePos = Input.mousePosition.GetVector2();
            lastMouseButtonDown = currentMouseButtonDown;
            for(int i = 0;i <= 2; i++)
            {
                if (lastMouseButtonDown)
                {
                    if (currentMouseButton == i)
                    {
                        currentMouseButtonDown = Input.GetMouseButton(i);
                        if (currentMouseButtonDown)
                        {
                            if (Time.time - mouseDownTime > clickToDragTimeSpan) 
                            {
                                if (i == 0) dragUpdate = true;
                                if (i == 1) sideFeatureUpdate = true;
                            }
                        }
                        else
                        {
                            if (Time.time - mouseDownTime <= clickToDragTimeSpan) clickUpdate = true;
                        }
                    }
                }
                else//上一帧没有按下
                {
                    currentMouseButtonDown = Input.GetMouseButton(i);
                    if (currentMouseButtonDown)
                    {
                        currentMouseButton = i;
                        mouseDownTime = Time.time;
                        break;
                    }
                }
            }
            if (dragUpdate)
            {
                if (lastDragUpdate != dragUpdate) mouseDownPos = Input.mousePosition.GetVector2();
                DragUpdate(mouseDownPos, currentMousePos);
            }
            else
            {
                int tempPiority = -1;
                for(int i = layersAndGraphics.Count - 1;i >= 0; i--)
                {
                    var layer = layersAndGraphics[i];
                    foreach (var graphics in layer)
                    {
                        if (!graphics.ShouldDrawOrUpdate) continue;
                        if (graphics.IsMouseOverMe(currentMousePos, tempPiority != -1) && graphics.inputControlPiority > tempPiority)
                        {
                            tempPiority = graphics.inputControlPiority;
                            dragFocusGraphics = graphics;
                        }
                    }
                }
                if (tempPiority == -1) dragFocusGraphics = null;
            }
            lastDragUpdate = dragUpdate;

            if (clickUpdate)
            {
                ClickUpdate(currentMouseButton);
            }

            SideFeatureUpdate(sideFeatureUpdate);
            if(sideFeatureUpdate) Plugin.Log("SideFeatureUpdate");
        }

        public void DragUpdate(Vector2 startDragPos, Vector2 currentDragPos)
        {
            if (dragFocusGraphics == null) return;
            dragFocusGraphics.DragUpdate(startDragPos, currentDragPos);
        }

        public void ClickUpdate(int mouseButton)
        {
            for (int i = layersAndGraphics.Count - 1; i >= 0; i--)
            {
                var layer = layersAndGraphics[i];
                foreach (var graphics in layer)
                {
                    if (!graphics.ShouldDrawOrUpdate) continue;
                    if (graphics.IsMouseOverMe(currentMousePos, false))
                    {
                        graphics.ClickOnMe(0);
                        break;
                    }
                }
            }
            if(onClickDelegate != null)
            {
                onClickDelegate.Invoke(mouseButton);
            }
        }

        public void SideFeatureUpdate(bool isButtonHolding)
        {
            foreach(var layer in layersAndGraphics)
            {
                foreach(var graphics in layer)
                {
                    graphics.SideFeatureUpdate(isButtonHolding);
                }
            }
        }

        public delegate void OnClick(int button);
    }
}
