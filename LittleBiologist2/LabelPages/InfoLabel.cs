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
    public class InfoLabel : LBioHUDGraphics
    {
        public FLabel testLabel = new FLabel(Custom.GetFont(),"");
        public CreatureInfoGetter creatureInfoGetter = new CreatureInfoGetter();

        public List<BaseLabelPage> labelPages = new List<BaseLabelPage>();

        Creature _focusCreature = null;
        public Creature FocusCreature
        {
            get => _focusCreature;
            set
            {
                if (value == FocusCreature) return;
                _focusCreature = value;
                LoadPagesForCreature(FocusCreature);
            }
        }

        int _currentPage = 0;
        public int CurrentPage
        {
            get => _currentPage;
            set
            {
                if (value == _currentPage)
                {
                    labelPages[_currentPage].AlternateSubPage();
                    return;
                }
                if (value >= labelPages.Count) return;

                foreach (var page in labelPages)
                {
                    page.AlternateMainPage(value);
                }
                _currentPage = value;
            }
        }

        public InfoLabel(LBioHUD part, LBioLabelCanvas canvas) : base(part,canvas)
        {
            alpha = 1f;
            isVisible = true;
            AnchorPos = new Vector2(100f, Screen.height - 50f);
            Plugin.instance.keyDownEventTrigger += KeyDownControl;
            LoadPages();
        }

        public void LoadPages()
        {
            labelPages.Add(new StaticInfoPage(hud, this, 0));
            labelPages.Add(new RelationshipPage(hud, this, 1));
        }

        public override void InitSprites()
        {
            fnodes.Add(testLabel);
            base.InitSprites();

            foreach (var page in labelPages)
            {
                page.AlternateMainPage(CurrentPage);
            }
        }

        public override void DrawSprites(float timeStacker)
        {
            base.DrawSprites(timeStacker);

            testLabel.SetPosition(AnchorPos.x, AnchorPos.y);
            testLabel.alpha = Alpha;
        }

        public override void Update()
        {
            base.Update();
            if (!ShouldDrawOrUpdate) return;
            creatureInfoGetter.UpdateDynamicInfo();
            //testFunc
        }

        public void KeyDownControl()
        {

            if (Input.GetKeyDown(KeyCode.F1))
            {
                CurrentPage = 0;
            }
            else if (Input.GetKeyDown(KeyCode.F2))
            {
                CurrentPage = 1;
            }
        }

        public override void Destroy()
        {
            Plugin.instance.keyDownEventTrigger -= KeyDownControl;
            base.Destroy();
        }

        public void LoadPagesForCreature(Creature creature)
        {
            foreach (var page in labelPages)
            {
                page.Destroy();
            }
            labelPages.Clear();

            testLabel.text = _focusCreature == null ? "null" : _focusCreature.abstractCreature.creatureTemplate.type.ToString() + " " + _focusCreature.abstractCreature.ID.number.ToString();//test func

            creatureInfoGetter.SetTarget(creature);
            LoadPages();

            foreach (var page in labelPages)
            {
                page.InitSprites();
                //page.isVisible = true;
                //page.alpha = 1f;
                page.AlternateMainPage(CurrentPage);
            }
        }
    }
}
