using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Reflection;
using System.Text.RegularExpressions;
using MonoMod.RuntimeDetour;
using LittleBiologist.LBioExpand;

namespace LittleBiologist
{
    public class CreatureInfoGetter
    {
        public static BindingFlags fieldFlag = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        public static FieldInfo[] ScavSkillFieldInfos;

        public AbstractCreature target;

        public Dictionary<string, float> PersonalityInfo = new Dictionary<string, float>();
        public Dictionary<string, float> ScavSkillInfo = new Dictionary<string, float>();

        bool last_DynamicInfoValid = true;
        public bool dynamicInfoValid = true;

        public List<DynamicFloatInfo> dynamicFloatInfos = new List<DynamicFloatInfo>();

        //customs
        public List<DynamicInfoGetter> customInfoGetter = new List<DynamicInfoGetter>();
        public List<DynamicFloatInfo> customFloatInfos = new List<DynamicFloatInfo>();

        public IUseRelationShipInfoGetter relationShipInfoGetter;

        static CreatureInfoGetter()
        {
            Type scavType = typeof(Scavenger);
            ScavSkillFieldInfos = new FieldInfo[5]
            {
                scavType.GetField("dodgeSkill",fieldFlag),
                scavType.GetField("midRangeSkill",fieldFlag),
                scavType.GetField("meleeSkill",fieldFlag),
                scavType.GetField("blockingSkill",fieldFlag),
                scavType.GetField("reactionSkill",fieldFlag),
            };
        }

        public CreatureInfoGetter()
        {
            AddDynamicGetters();
        }

        public void SetTarget(Creature creature)
        {
            Plugin.Log("Clear Infos");
            PersonalityInfo.Clear();
            ScavSkillInfo.Clear();
            dynamicFloatInfos.Clear();
            customFloatInfos.Clear();

            if (creature == null) return;
            target = creature.abstractCreature;


            relationShipInfoGetter.SetTarget(creature);
            if (customInfoGetter.Count > 0)
            {
                foreach (var hook in customInfoGetter)
                {
                    hook.SetTarget(creature);
                }
            }

            Plugin.Log("Label start getting info for : " + creature.ToString());
            GetStaticInfo();
            InitDynamicInfo();

            Plugin.Log("Info getting done, Personality count : " + PersonalityInfo.Count.ToString() + " ScavSkillInfo count : " + ScavSkillInfo.Count.ToString());
            Plugin.Log("DynamicInfo getting done, dynamicFloatInfos count : " + dynamicFloatInfos.Count.ToString());
        }

        public void InitDynamicInfo()
        {
            if (target == null) return;
            if (target.abstractAI == null) return;
            if (target.abstractAI.RealAI == null) return;
            if (target.abstractAI.RealAI is IUseARelationshipTracker && target.creatureTemplate.socialMemory)
            {
                dynamicFloatInfos.Add(new DynamicFloatInfo("Like", GetRelationshipLike));
                dynamicFloatInfos.Add(new DynamicFloatInfo("TempLike", GetRelationshipTempLike));
                dynamicFloatInfos.Add(new DynamicFloatInfo("Fear", GetRelationshipFear));
                dynamicFloatInfos.Add(new DynamicFloatInfo("TempFear", GetRelationshipTempFear));
                dynamicFloatInfos.Add(new DynamicFloatInfo("Know", GetRelationshipKnow));
            }

            if(LBioExpandCore.customPages.Count > 0)
            {
                foreach (var customPage in LBioExpandCore.customPages)
                {
                    customPage.dynamicFloatInfos.Clear();
                    customPage.InitDynamicInfos(this);
                }
            }
        }

        public void AddDynamicGetters()
        {
            relationShipInfoGetter = new IUseRelationShipInfoGetter();

            if (LBioExpandCore.customPages.Count > 0)
            {
                foreach (var customPage in LBioExpandCore.customPages)
                {
                    customPage.AddDynamicGetters(this);
                }
            }
        }

        public void UpdateDynamicInfo()
        {
            last_DynamicInfoValid = dynamicInfoValid;
            if (target == null)
            {
                dynamicInfoValid = false;
                return;
            }

            dynamicInfoValid = target.realizedCreature != null;

            if (!dynamicInfoValid) return;
            if (dynamicFloatInfos.Count == 0) return;

            try
            {
                foreach (var dynamicInfo in dynamicFloatInfos)
                {
                    dynamicInfo.UpdateInfo(target);
                }
                if(customFloatInfos.Count > 0)
                {
                    foreach(var customFloatInfo in customFloatInfos)
                    {
                        customFloatInfo.UpdateInfo(target);
                    }
                }
            }
            catch (Exception e)
            {
                if (e is NullReferenceException)
                {
                    string message = e.Message;
                    message += "\nTarget Info: ";

                    bool targetNull = target == null;
                    bool targetRealizeNull = targetNull ? true : target.realizedCreature == null;
                    bool targetStateNull = targetRealizeNull ? true : target.realizedCreature.State == null;
                    bool targetSocialNull = targetStateNull ? true : target.realizedCreature.State.socialMemory == null;

                    message += " AbCreature-" + (targetNull ? " Null " : target.ToString());
                    message += " RealCreature-" + (targetRealizeNull ? "Null" : target.realizedCreature.ToString());
                    message += " State-" + (targetStateNull ? "Null" : target.realizedCreature.State.ToString());
                    message += " SocialMemory-" + (targetSocialNull ? "Null" : target.realizedCreature.State.socialMemory.ToString());
                    Debug.LogException(new NullReferenceException(message));
                }
                else
                {
                    Debug.LogException(e);
                }
            }
        }

        public void GetStaticInfo()
        {
            if (target == null) return;

            CreatureTemplate.Type targetType = target.creatureTemplate.type;

            bool usingAggression = targetType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC ||
                                   targetType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.Yeek ||
                                   targetType == CreatureTemplate.Type.PoleMimic ||
                                   targetType.ToString().Contains("Scavenger") ||
                                   targetType == CreatureTemplate.Type.TentaclePlant;

            bool usingBravery = targetType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC ||
                                   targetType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.Yeek ||
                                   targetType.ToString().Contains("Cicada") ||
                                   targetType.ToString().Contains("Scavenger");

            bool usingDominance = targetType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC ||
                                  targetType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.Yeek ||
                                  targetType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.MotherSpider ||
                                  targetType == CreatureTemplate.Type.BigSpider ||
                                  targetType == CreatureTemplate.Type.SpitterSpider ||
                                  targetType == CreatureTemplate.Type.Deer ||
                                  targetType == CreatureTemplate.Type.DropBug ||
                                  targetType == CreatureTemplate.Type.EggBug ||
                                  targetType == CreatureTemplate.Type.MirosBird ||
                                  targetType.ToString().Contains("Scavenger") ||
                                  targetType.ToString().Contains("Lizard") ||
                                  targetType == CreatureTemplate.Type.TempleGuard;

            bool usingEnergy = targetType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC ||
                               targetType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.Yeek ||
                               targetType == CreatureTemplate.Type.JetFish ||
                               targetType.ToString().Contains("Scavenger") ||
                               targetType.ToString().Contains("Lizard");

            bool usingNervous = targetType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC ||
                                targetType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.Yeek ||
                                targetType.ToString().Contains("Scavenger");

            bool usingSympathy = targetType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.SlugNPC ||
                                 targetType == MoreSlugcats.MoreSlugcatsEnums.CreatureTemplateType.Yeek ||
                                 targetType.ToString().Contains("Scavenger");

            if (usingAggression) PersonalityInfo.Add("Aggression", target.personality.aggression);
            if (usingBravery) PersonalityInfo.Add("Bravery", target.personality.bravery);
            if (usingDominance) PersonalityInfo.Add("Dominance", target.personality.dominance);
            if (usingEnergy) PersonalityInfo.Add("Energy", target.personality.energy);
            if (usingNervous) PersonalityInfo.Add("Nervous", target.personality.nervous);
            if (usingSympathy) PersonalityInfo.Add("Sympathy", target.personality.sympathy);


            if (!targetType.ToString().Contains("Scavenger") || target.realizedCreature == null) return;
            Scavenger scav = target.realizedCreature as Scavenger;
            for (int i = 0; i < ScavSkillFieldInfos.Length; i++)
            {
                string name = Regex.Replace(ScavSkillFieldInfos[i].Name, "Skill", "");
                ScavSkillInfo.Add(name, (float)ScavSkillFieldInfos[i].GetValue(scav));
            }
        }

        float GetRelationshipLike(AbstractCreature abstractCreature)
        {
            var socialMemory = abstractCreature.realizedCreature.State.socialMemory;
            return socialMemory.GetLike(abstractCreature.realizedCreature.room.world.game.Players[0].ID);
        }
        float GetRelationshipTempLike(AbstractCreature abstractCreature)
        {
            var socialMemory = abstractCreature.realizedCreature.State.socialMemory;
            return socialMemory.GetTempLike(abstractCreature.realizedCreature.room.world.game.Players[0].ID);
        }
        float GetRelationshipKnow(AbstractCreature abstractCreature)
        {
            var socialMemory = abstractCreature.realizedCreature.State.socialMemory;
            return socialMemory.GetKnow(abstractCreature.realizedCreature.room.world.game.Players[0].ID);
        }
        float GetRelationshipFear(AbstractCreature abstractCreature)
        {
            var socialMemory = abstractCreature.realizedCreature.State.socialMemory;
            var relationship = socialMemory.GetOrInitiateRelationship(abstractCreature.realizedCreature.room.world.game.Players[0].ID);
            return relationship.fear;
        }
        float GetRelationshipTempFear(AbstractCreature abstractCreature)
        {
            var socialMemory = abstractCreature.realizedCreature.State.socialMemory;
            var relationship = socialMemory.GetOrInitiateRelationship(abstractCreature.realizedCreature.room.world.game.Players[0].ID);
            return relationship.tempFear;
        }


        public delegate float UpdateDynamicInfoDelegate(AbstractCreature abstractCreature);

        public class DynamicFloatInfo
        {
            public string name;
            public float floatInfo;

            public DynamicFloatInfo(string name, UpdateDynamicInfoDelegate updateFunc, float defaultValue = 0f)
            {
                this.updateFunc = null;
                this.updateFunc += updateFunc;
                this.name = name;
                floatInfo = defaultValue;
            }

            public void UpdateInfo(AbstractCreature abstractCreature)
            {
                floatInfo = updateFunc(abstractCreature);
                //Debug.Log(abstractCreature.ToString() + " " + name + "Update Value to " + floatInfo.ToString());
            }

            public UpdateDynamicInfoDelegate updateFunc;
        }
    }

    public class DynamicInfoGetter
    {
        List<TimeLine.TimeLineEventInfo> buffer = new List<TimeLine.TimeLineEventInfo>();
        public List<TimeLine.TimeLineEventInfo> allEvents = new List<TimeLine.TimeLineEventInfo>();
        public bool eventsLock = false;

        public WeakReference<Creature> target = new WeakReference<Creature>(null);
        public List<Hook> hooks = new List<Hook>();

        float lastEventInfoTime = 0f;
        public virtual float minPopEventTimeSpan => 0.1f;

        public bool TargetAvailable => target.TryGetTarget(out Creature creature);

        public void SetTarget(Creature creature)
        {
            DisposeAllHooks();
            target.SetTarget(creature);
            target.TryGetTarget(out var cret);
            Plugin.Log("Set Target : " + cret.ToString());
            if (IsTargetEligible(creature)) HookOn(creature);
        }

        public virtual bool IsTargetEligible(Creature creature)
        {
            return false;
        }

        public List<TimeLine.TimeLineEventInfo> PopCurrentEventInfos()
        {
            eventsLock = true;
            List<TimeLine.TimeLineEventInfo> result = new List<TimeLine.TimeLineEventInfo>();

            while(allEvents.Count > 0)
            {
                result.Add(allEvents.Pop());
            }
            eventsLock = false;

            while(buffer.Count > 0)
            {
                allEvents.Add(buffer.Pop());
            }

            return result;
        }

        public void AddEventInfo(TimeLine.TimeLineEventInfo info,bool forceAdd = false)
        {
            if (info.triggerTime - lastEventInfoTime < minPopEventTimeSpan && !forceAdd) return;
            if (eventsLock) buffer.Add(info);
            else allEvents.Add(info);
            lastEventInfoTime = info.triggerTime;
        }

        public virtual void HookOn(Creature creature)
        {
        }

        public void DisposeAllHooks()
        {
            Plugin.Log(ToString() + " DisposeAllHooks");
            if(hooks.Count > 0)
            {
                foreach (var hook in hooks)
                {
                    hook.Dispose();
                }
                hooks.Clear();
            }
        }

        public void ReApplyAllHooks()
        {
            if(hooks.Count > 0)
            {
                foreach (var hook in hooks)
                {
                    hook.Apply();
                }
            }
        }
    }

    public class DynamicInfoHook
    {
        public Hook hook;
        public DynamicInfoHook(Player player)
        {
            hook = new Hook(
                player.GetType().GetMethod("Update",BindingFlags.Instance | BindingFlags.Public),
                typeof(DynamicInfoHook).GetMethod("TestUpdate",BindingFlags.Static | BindingFlags.Public)
                );
        }

        public static void TestUpdate(Action<Player,bool> orig,Player self,bool eu)
        {
            orig.Invoke(self, eu);
            Plugin.Log("Hook Player Update");
        }
    }

    public class IUseRelationShipInfoGetter : DynamicInfoGetter
    {
        public override float minPopEventTimeSpan => 0.5f;
        public static IUseRelationShipInfoGetter instance;

        public IUseRelationShipInfoGetter()
        {
            instance = this;
        }

        public override bool IsTargetEligible(Creature creature)
        {
            if (creature.abstractCreature == null) return false;
            if (creature.abstractCreature.abstractAI == null) return false;
            if (creature.abstractCreature.abstractAI.RealAI == null) return false;
            return creature.abstractCreature.abstractAI.RealAI is IUseARelationshipTracker;
        }

        public override void HookOn(Creature creature)
        {
            IUseARelationshipTracker instance = creature.abstractCreature.abstractAI.RealAI as IUseARelationshipTracker;

            Hook dynamicRelationShipHook = new Hook(
                instance.GetType().GetMethod("IUseARelationshipTracker.UpdateDynamicRelationship", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
                this.GetType().GetMethod("UpdateDynamicRelationshipHook", BindingFlags.Instance | BindingFlags.Public),
                this
                );

            hooks.Add(dynamicRelationShipHook);
            Plugin.Log("Hook On for " + creature.ToString());
        }

        public CreatureTemplate.Relationship UpdateDynamicRelationshipHook(Func<IUseARelationshipTracker,RelationshipTracker.DynamicRelationship, CreatureTemplate.Relationship> orig, IUseARelationshipTracker self, RelationshipTracker.DynamicRelationship dRelationship)
        {
            CreatureTemplate.Relationship origRelationship = dRelationship.trackerRep.dynamicRelationship.currentRelationship;
            CreatureTemplate.Relationship newRelationShip = orig.Invoke(self, dRelationship);

            if(target.TryGetTarget(out var creature) && creature == (self as ArtificialIntelligence).creature.realizedCreature)
            {
                if(origRelationship != newRelationShip || origRelationship.intensity != newRelationShip.intensity)
                {
                    string type = newRelationShip.type.ToString();
                    string origRel = origRelationship.type.ToString() + string.Format("{0:F3}", origRelationship.intensity);
                    string newRel = newRelationShip.type.ToString() + string.Format("{0:F3}", newRelationShip.intensity);
                    string description = "Target:" + dRelationship.trackerRep.representedCreature.ToString() + "| " + origRel + " -> " + newRel;
                    float time = Time.time;

                    AddEventInfo(new TimeLine.TimeLineEventInfo(type, description, time,newRelationShip.intensity));
                }
            }

            return newRelationShip;
        }
    }
}
