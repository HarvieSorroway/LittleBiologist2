using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LittleBiologist
{
    public class CreaturePointer : LBioHUDGraphics
    {
        public static float size = 10f;

        public TriangleMesh pointer;
        public int mapOwnerRoom => hud.hud.owner.MapOwnerRoom;
        public bool ShouldReval => hud.infoLabel.FocusCreature != null && hud.infoLabel.FocusCreature.abstractCreature.pos.room == mapOwnerRoom;

        public Vector2 smoothBasePos = Vector2.zero;
        public Vector2 lastBasePos = Vector2.zero;
        
        public CreaturePointer(LBioHUD hud,LBioLabelCanvas canvas) : base(hud, canvas, false)
        {
            isVisible = true;
            alpha = 0f;
        }

        public override void InitSprites()
        {
            pointer = new TriangleMesh("pixel", new TriangleMesh.Triangle[] { new TriangleMesh.Triangle(0, 1, 2) }, true, true);
            fnodes.Add(pointer);

            base.InitSprites();
        }

        public override void DrawSprites(float timeStacker)
        {
            base.DrawSprites(timeStacker);
            if (!ShouldDrawOrUpdate) return;

            alpha = Mathf.Lerp(alpha, ShouldReval ? 1f : 0f, 0.1f);
            if (ShouldReval)
            {
                Vector2 basePoint = hud.infoLabel.FocusCreature.DangerPos + Vector2.up * 30f;

                smoothBasePos = Vector2.Lerp(lastBasePos, basePoint, 0.1f);
                lastBasePos = smoothBasePos;

                pointer.MoveVertice(1, smoothBasePos - hud.cam.pos);
                pointer.MoveVertice(0, smoothBasePos - hud.cam.pos + Vector2.up * size + Vector2.left * size);
                pointer.MoveVertice(2, smoothBasePos - hud.cam.pos + Vector2.up * size + Vector2.right * size);
            }
            
            for(int i = 0;i < 3; i++)
            {
                pointer.verticeColors[i].a = Alpha;
            }
        }
    }
}
