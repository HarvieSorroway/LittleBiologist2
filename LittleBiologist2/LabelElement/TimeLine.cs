using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LittleBiologist
{
    public class TimeLine : LBioHUDGraphics
    {
        public static readonly float textLineSpan = 5f;

        public readonly float axisHeight = 2f;

        public readonly float typeBarWidth;

        //Axis settings
        public readonly float width;
        public readonly float timeSpan;
        public readonly Color axisColor;
        public readonly string description;

        public FSprite axis;
        public FLabel label;
        public FLabel warningLabel;

        public ShowDetailLabel detailLabel;

        public Dictionary<string, List<TimeLineEvent>> events = new Dictionary<string, List<TimeLineEvent>>();
        public Dictionary<string, float> eventHeights = new Dictionary<string, float>();

        public TimeLine(LBioHUD hud,LBioHUDGraphics parent,float width,float timeSpan,string description,Color axisColor) : base(hud, parent, true)
        {
            this.width = width;
            this.timeSpan = timeSpan;
            this.axisColor = axisColor;
            this.description = description;

            detailLabel = new ShowDetailLabel(hud, this);
            detailLabel.localPos = new Vector2(0f, -40f);

            alpha = 1f;
        }

        public override void InitSprites()
        {
            axis = new FSprite("pixel", true)
            {
                scaleX = width,
                scaleY = axisHeight,
                color = axisColor,

                anchorX = 0f,
                anchorY = 1f,
            };
            label = new FLabel(Custom.GetFont(), description)
            {
                anchorY = 1f,
                color = axisColor,
                scale = 1.01f
            };
            warningLabel = new FLabel(Custom.GetFont(), "Target not available,please refresh label")
            {
                scale = 1.5f,
                color = Color.red,
            };

            fnodes.Add(axis);
            fnodes.Add(label);
            base.InitSprites();
        }

        public override void DrawSprites(float timeStacker)
        {
            base.DrawSprites(timeStacker);
            if (!ShouldDrawOrUpdate) return;

            axis.SetPosition(AnchorPos.x, AnchorPos.y);
            axis.alpha = Alpha;
            label.SetPosition(AnchorPos.x + width / 2f, AnchorPos.y - TimeLine.textLineSpan);
            axis.alpha = Alpha;
            warningLabel.SetPosition(AnchorPos.x + width / 2f, AnchorPos.y + 50f);
            warningLabel.alpha = Alpha * (hud.infoLabel.creatureInfoGetter.relationShipInfoGetter.TargetAvailable ? 0f : 1f);
        }

        public override void Update()
        {
            base.Update();
            if (!ShouldDrawOrUpdate) return;
        }

        public override void Destroy()
        {
            events.Clear();
            eventHeights.Clear();
            base.Destroy();
        }

        public void AddEvent(string type, string description,float time,float intensity)
        {
            if (!eventHeights.ContainsKey(type))
            {
                eventHeights.Add(type, Mathf.Lerp(0f, 60f, Random.value));
                events.Add(type, new List<TimeLineEvent>());
            }

            var newEvent = new TimeLineEvent(type, description, time, eventHeights[type], this, intensity);
            events[type].Add(newEvent);
            newEvent.InitSprites();
        }
        public void AddEvent(TimeLineEventInfo eventInfo)
        {
            AddEvent(eventInfo.type, eventInfo.description, eventInfo.triggerTime,eventInfo.intensity);
        }

        public void RemoveEvent(TimeLineEvent tEvent)
        {
            events[tEvent.type].Remove(tEvent);
            if(events[tEvent.type].Count == 0)
            {
                events.Remove(tEvent.type);
                eventHeights.Remove(tEvent.type);
            }
            tEvent.Destroy();
        }

        public class TimeLineEvent : LBioHUDGraphics
        {
            public string type;
            public float triggerTime;
            public string description;

            public bool loaded = false;

            public float height;
            public float intensity;

            public FLabel typeLabel;
            public FSprite line;

            public TimeLine TimeLine => parentGraphics as TimeLine;

            public TimeLineEvent(string type,string description,float time,float height,TimeLine timeline,float intensity) : base(timeline.hud, timeline, true)
            {
                this.type = type;
                this.triggerTime = time;
                this.description = description;
                this.height = height;
                this.intensity = intensity;

                loaded = true;
                isVisible = TimeLine.IsVisible;
                alpha = 1f;
            }

            public TimeLineEvent(TimeLineEventInfo info,float height,TimeLine timeLine) : base(timeLine.hud, timeLine, true)
            {
                type = info.type;
                triggerTime = info.triggerTime;
                description = info.description;
                this.height = height;

                loaded = true;
                isVisible = TimeLine.IsVisible;
                alpha = 1f;
            }

            public override void InitSprites()
            {
                //左下角定位，方便渲染
                line = new FSprite("pixel", true)
                {
                    anchorX = 0,
                    anchorY = 0,
                    scaleY = height,
                    scaleX = 2f,
                    isVisible = false,
                    color = Color.Lerp(Color.green,Color.red,intensity),
                };
                typeLabel = new FLabel(Custom.GetFont(), type)
                {
                    anchorX = 0,
                    anchorY = 0,
                    isVisible = false,
                    rotation = -45f,
                    color = Color.Lerp(Color.green, Color.red, intensity),
                };

                fnodes.Add(typeLabel);
                fnodes.Add(line);
                base.InitSprites();
            }

            public override void DrawSprites(float timeStacker)
            {
                base.DrawSprites(timeStacker);
                if (!ShouldDrawOrUpdate) return;

                line.SetPosition(AnchorPos);
                line.alpha = Alpha;

                typeLabel.SetPosition(AnchorPos + Vector2.up * height);
                typeLabel.alpha = Alpha;
            }

            public override void Update()
            {
                base.Update();
                if (!ShouldDrawOrUpdate) return;

                float cof = Mathf.InverseLerp(Time.time - TimeLine.timeSpan, Time.time, triggerTime);
                float biasX = Mathf.Lerp(0f, TimeLine.width, cof);
                localPos = new Vector2(biasX, 0f);

                if (cof == 0f) TimeLine.RemoveEvent(this);
            }

            public override bool IsMouseOverMe(Vector2 mousePos, bool higherPiorityAlreadyOver)
            {
                Vector2 rootPos = AnchorPos + Vector2.up * height;
                float width = typeLabel.textRect.width;
                float _height = typeLabel.textRect.height;
                Vector2 delta = mousePos - rootPos;
                return delta.x > 0 && delta.x < width && delta.y > 0 && delta.y < _height;
            }

            public override void ClickOnMe(int mouseButton)
            {
                if(mouseButton == 0)
                {
                    TimeLine.detailLabel.SetDisplayInfos(type, triggerTime, description);
                }
            }
        }

        public class TimeLineEventInfo
        {
            public string type;
            public float triggerTime;
            public string description;
            public float intensity;
            public TimeLineEventInfo(string type, string description, float time,float intensity)
            {
                this.type = type;
                this.triggerTime = time;
                this.description = description;
                this.intensity = intensity;
            }
        }

        public class ShowDetailLabel : LBioHUDGraphics
        {
            public FLabel titleLabel;
            public FLabel timeLabel;
            public FLabel descriptionLabel;
            public ShowDetailLabel(LBioHUD hud,TimeLine timeLine) : base(hud, timeLine)
            {
                isVisible = true;
                alpha = 1f;
            }

            public override void InitSprites()
            {
                titleLabel = new FLabel(Custom.GetFont(), "")
                {
                    anchorX = 0,
                    anchorY = 1,
                    isVisible = false,
                };
                timeLabel = new FLabel(Custom.GetFont(), "")
                {
                    anchorX = 0,
                    anchorY = 1,
                    isVisible = false,
                };
                descriptionLabel = new FLabel(Custom.GetFont(), "")
                {
                    anchorX = 0,
                    anchorY = 1,
                    isVisible = false,
                };
                fnodes.Add(titleLabel);
                fnodes.Add(timeLabel);
                fnodes.Add(descriptionLabel);

                base.InitSprites();
            }

            public override void DrawSprites(float timeStacker)
            {
                base.DrawSprites(timeStacker);
                if (!ShouldDrawOrUpdate) return;

                float biasY = 0f;
                titleLabel.SetPosition(AnchorPos.x, AnchorPos.y);
                titleLabel.alpha = Alpha;

                biasY = titleLabel.textRect.height + TimeLine.textLineSpan;
                timeLabel.SetPosition(AnchorPos.x, AnchorPos.y - biasY);
                timeLabel.alpha = Alpha;

                biasY += timeLabel.textRect.height + TimeLine.textLineSpan;
                descriptionLabel.SetPosition(AnchorPos.x, AnchorPos.y - biasY);
                descriptionLabel.alpha = Alpha;
            }

            public void SetDisplayInfos(TimeLineEventInfo timeLineEventInfo)
            {
                titleLabel.text = "Type : " + timeLineEventInfo.type;
                timeLabel.text = "Time : " + timeLineEventInfo.triggerTime.ToString();
                descriptionLabel.text = "Details : " + timeLineEventInfo.description;
            }

            public void SetDisplayInfos(string type,float time,string description)
            {
                titleLabel.text = "Type : " + type;
                timeLabel.text = "Time : " + time.ToString();
                descriptionLabel.text = "Details : " + description;
            }
        }
    }

    public class RelationShipTrackerTimeLine : TimeLine
    {
        public RelationShipTrackerTimeLine(LBioHUD hud, LBioHUDGraphics parent, float width, Color axisColor) : base(hud, parent, width, 10f,"RelationShip and Item events", Color.yellow)
        {
        }

        public override void Update()
        {
            base.Update();
            if (!ShouldDrawOrUpdate) return;

            var lst = hud.infoLabel.creatureInfoGetter.relationShipInfoGetter.PopCurrentEventInfos();
            if(lst.Count > 0)
            {
                foreach (var eventInfo in lst)
                {
                    AddEvent(eventInfo);
                }
            }
        }
    }
}
