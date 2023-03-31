using UnityEngine;

namespace LittleBiologist
{
    public class ControlHandle : LBioHUDGraphics
    {
        public Vector2 dragStartTargetLocalPos;

        public Color baseColor;
        public Color favoriteColor;

        public float baseSize;
        public float bumpSizr;

        public float targetSize = 1f;
        public float smoothSize = 0f;
        public float lastSize = 0f;

        public FSprite handle;
        public ControlHandle(LBioHUD hud,LBioHUDGraphics controlTarget,Color baseColor,Color favoriteColor,float baseSize, float bumpSizeFactor) : base(hud, controlTarget, true, InputModule.HighestLayer)
        {
            this.baseColor = baseColor;
            this.favoriteColor = favoriteColor;
            this.baseSize = baseSize;
            this.bumpSizr = bumpSizeFactor * baseSize;

            alpha = 1f;
            isVisible = true;
        }

        public override void InitSprites()
        {
            handle = new FSprite("Circle4", true) { color = baseColor};
            fnodes.Add(handle);
            base.InitSprites();
        }

        public override void DrawSprites(float timeStacker)
        {
            base.DrawSprites(timeStacker);
            if (!ShouldDrawOrUpdate) return;

            smoothSize = Mathf.Lerp(lastSize, targetSize, 0.1f);
            lastSize = smoothSize;

            handle.scale = smoothSize;
            handle.SetPosition(AnchorPos);
            handle.alpha = Alpha * 0.2f;
        }

        public override void DragStart()
        {
            dragStartTargetLocalPos = parentGraphics.localPos;
            lastSize = 0.5f;
        }

        public override void DragUpdate(Vector2 startDragPos, Vector2 currentDragPos)
        {
            Vector2 delta = currentDragPos - startDragPos;
            parentGraphics.localPos = Vector2.Lerp(parentGraphics.localPos, dragStartTargetLocalPos + delta, 0.5f);
        }

        public override bool IsMouseOverMe(Vector2 mousePos, bool higherPiorityAlreadyOver)
        {
            float distance = Vector2.Distance(mousePos, AnchorPos);
            bool result = (!higherPiorityAlreadyOver) && (distance < 7f);

            targetSize = result ? bumpSizr : baseSize;
            return result;
        }
    }
}
