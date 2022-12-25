﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModLoader
{
    public class GameStatWarpper : WarpperBase
    {
        public GameStatWarpper(string SrcPath) : base(SrcPath) { }

        public void WarpperCustomSelf(GameStat obj)
        {
            WarpperFunction.ClassWarpper(obj, "TimeOfDayMods", TimeOfDayModsWarpType, TimeOfDayModsWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "RequiredPerks", RequiredPerksWarpType, RequiredPerksWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "DefaultStatusIcon", DefaultStatusIconWarpType, DefaultStatusIconWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "Statuses", StatusesWarpType, StatusesWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "FeedbackInfo", FeedbackInfoWarpType, FeedbackInfoWarpData, SrcPath);

            WarpperFunction.ClassWarpper(obj, "NotEnoughIcon", NotEnoughIconWarpType, NotEnoughIconWarpData, SrcPath);
        }

        // VisibleGameStatTab: StatListTab
        public String VisibleGameStatStatListTab;

        // TimeOfDayMods: StatTimeOfDayModifier[]
        public WarpperFunction.WarpType TimeOfDayModsWarpType;
        public List<String> TimeOfDayModsWarpData;

        // RequiredPerks: CharacterPerk[]
        public WarpperFunction.WarpType RequiredPerksWarpType;
        public List<String> RequiredPerksWarpData;

        // DefaultStatusIcon: Sprite
        public WarpperFunction.WarpType DefaultStatusIconWarpType;
        public String DefaultStatusIconWarpData;

        // Statuses: StatStatus[]
        public WarpperFunction.WarpType StatusesWarpType;
        public List<String> StatusesWarpData;

        // FeedbackInfo: AmtFeedbackInfo
        public WarpperFunction.WarpType FeedbackInfoWarpType;
        public String FeedbackInfoWarpData;

        //// OverrideFeedbackPrefab: UIFeedbackStepsBase
        //public WarpperFunction.WarpType OverrideFeedbackPrefabWarpType;
        //public String OverrideFeedbackPrefabWarpData;

        // NotEnoughIcon: Sprite
        public WarpperFunction.WarpType NotEnoughIconWarpType;
        public String NotEnoughIconWarpData;
    }
}
