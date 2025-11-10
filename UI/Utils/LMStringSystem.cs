using System;
using System.Collections.Generic;

namespace LinkedMovement.UI.Utils {
    public enum LMStringKey {
        TODO,
        INTRODUCTION,
        MODE_SELECT,
        CREATE_ANIM_INTRO,
        SELECT_ANIM_TARGET_EXISTS,
        ANIM_NAME,
        ANIM_IS_TRIGGERABLE,
        ANIM_DELAY_ON_PARK_LOAD,
        ANIM_DELAY_ON_PARK_LOAD_MINMAX,
        ANIM_STEPS,
        ANIM_STEP_NAME,
        ANIM_STEP_START_DELAY,
        ANIM_STEP_DURATION,
        ANIM_STEP_EASE,
        ANIM_STEP_POSITION,
        ANIM_STEP_ROTATION,
        ANIM_STEP_SCALE,
        ANIM_STEP_COLOR,
        ANIM_STEP_END_DELAY,
        CREATE_LINK_PARENT_INTRO,
        CREATE_LINK_TARGETS_INTRO,
        SELECT_LINK_TARGET_IS_PARENT,
        SELECT_LINK_TARGET_IS_TARGET,
        SELECT_LINK_PARENT_EXISTS,
        SELECT_LINK_TARGET_EXISTS,
        SELECT_LINK_CIRCULAR,
        LINK_NAME,
    }

    public class LMStringSystem {
        private static readonly Dictionary<LMStringKey, string> texts = new() {
            { LMStringKey.TODO, "To Do" },
            { LMStringKey.INTRODUCTION, "To Do: Mod introduction text" },
            { LMStringKey.MODE_SELECT, "To Do: Mode select text" },
            { LMStringKey.CREATE_ANIM_INTRO, "Select the object that will animate." },
            { LMStringKey.SELECT_ANIM_TARGET_EXISTS, "Selection already has Animation '{0}'." },
            { LMStringKey.ANIM_NAME, "Give the animation a unique name to distinguish it from other animations." },
            { LMStringKey.ANIM_IS_TRIGGERABLE, "If selected, the animation will only animate when triggered via an Effect Controller.\n\nWhen editing, the animation will automatically play so you can see the effects." },
            { LMStringKey.ANIM_DELAY_ON_PARK_LOAD, "When the park is loaded all animations without this flag will start immediately. This can make animations look unnatural as they all 'start' at the same time.\n\nSet this option to delay the animation from starting on park load." },
            { LMStringKey.ANIM_DELAY_ON_PARK_LOAD_MINMAX, "Amount of time (in seconds) to delay the animation from starting when the park is loaded.\n\nThe 'min' value is the minumum amount of time to delay.\nThe 'max' value is the maximum amount of time to delay.\n\nA random time between 'min' and 'max' will be chosen to delay the animation from starting on park load." },
            { LMStringKey.ANIM_STEPS, "Here you'll create the animation 'steps' that will happen, in order.\n\nThe Up ('↑') button will move the step up in the list.\nThe Down ('↓') button will move the step down in the list.\nThe '+Dup' button will add a duplicate of the step to the list.\nThe '+Inv' button will add a 'reverse' version of the step to the list. This is useful for animating back to the previous values.\nThe '✕' button will delete the step." },
            { LMStringKey.ANIM_STEP_NAME, "Give the step a unique name to distinguish it from other steps." },
            { LMStringKey.ANIM_STEP_START_DELAY, "The amount of time (in seconds) to pause the animation before running this step." },
            { LMStringKey.ANIM_STEP_DURATION, "The amount of time (in seconds) that this step will take to complete." },
            { LMStringKey.ANIM_STEP_EASE, "How the step should animate from its initial values to its final values.\n\nSee 'easings.net' for examples." },
            { LMStringKey.ANIM_STEP_POSITION, "The amount of tiles, relative to its starting position, the object should move.\n\nTiles have a size of '1' in width, length, and height.\nPositive and negative values are permitted." },
            { LMStringKey.ANIM_STEP_ROTATION, "The degrees, relative to its starting rotation, the object should rotate.\n\nA full rotation around an axis is '360'.\nPositive an negative values are permitted." },
            { LMStringKey.ANIM_STEP_SCALE, "The scale, relative to its starting scale, to resize the object.\n\nNegative values will shrink the object.\nPositive values will expand the object." },
            { LMStringKey.ANIM_STEP_COLOR, "Set these to animate the objects custom colors." },
            { LMStringKey.ANIM_STEP_END_DELAY, "The amount of time (in seconds) to pause the animation after completing this step." },
            { LMStringKey.CREATE_LINK_PARENT_INTRO, "Select the parent object that targets will attach to." },
            { LMStringKey.CREATE_LINK_TARGETS_INTRO, "Select the target objects that will attach to the parent object." },
            { LMStringKey.SELECT_LINK_TARGET_IS_PARENT, "Selection {0} is already the Link parent object." },
            { LMStringKey.SELECT_LINK_TARGET_IS_TARGET, "Selection {0} is already a Link target object." },
            { LMStringKey.SELECT_LINK_PARENT_EXISTS, "Selection {0} is already the parent of Link '{1}'." },
            { LMStringKey.SELECT_LINK_TARGET_EXISTS, "Selection {0} is already the target of Link '{1}'." },
            { LMStringKey.SELECT_LINK_CIRCULAR, "Circular link! Selection {0} is already the parent of this link." },
            { LMStringKey.LINK_NAME, "Give the link a unique name to distinguish it from other links." },
        };

        public static string GetText(LMStringKey key, params object[] args) {
            if (!texts.TryGetValue(key, out var template)) {
                LinkedMovement.Log("LMStringSystem.GetText failed to find key: " + key);
                return "ERROR: Couldn't find string key: " + key.ToString();
            }

            if (args == null || args.Length == 0) {
                return template;
            }

            try {
                return string.Format(template, args);
            } catch (FormatException e) {
                LinkedMovement.Log("ERROR: LMStringSystem.GetText FormatException for key: " + key.ToString());
                LinkedMovement.Log(e.Message);
                return template;
            }
        }
    }
}
