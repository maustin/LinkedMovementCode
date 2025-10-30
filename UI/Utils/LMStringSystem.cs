using System.Collections.Generic;

namespace LinkedMovement.UI.Utils {
    public enum LMStringKey {
        TODO,
        CREATE_NEW_ANIM_NAME,
        SELECT_TARGET_INTRO,
        SELECT_ORIGIN_INTRO,
        SELECT_ORIGIN_POSITION,
        ANIMATE_INTRO,
        ANIMATE_IS_TRIGGERABLE,
        ANIMATE_DELAY_ON_PARK_LOAD,
        ANIMATE_STEPS_INTRO,
        ANIMATE_STEP_NAME,
        ANIMATE_START_DELAY,
        ANIMATE_DURATION,
        ANIMATE_EASE,
        ANIMATE_CHANGE_POSITION,
        ANIMATE_CHANGE_ROTATION,
        ANIMATE_CHANGE_SCALE,
        ANIMATE_END_DELAY,
        EDIT_EXISTING_ANIM_NAME,
    }

    public class LMStringSystem {
        private static readonly Dictionary<LMStringKey, string> texts = new() {
            { LMStringKey.TODO, "To Do" },
            { LMStringKey.CREATE_NEW_ANIM_NAME, "Give the new animation a name to help distinguish it from other animations you create." },
            { LMStringKey.SELECT_TARGET_INTRO, "Select the objects that will be 'attached' to a parent object." },
            { LMStringKey.SELECT_ORIGIN_INTRO, "Select or Generate the object that will animate. The attached Targets will move along with this object." },
            { LMStringKey.SELECT_ORIGIN_POSITION, "Change to Origin object's pivot point." },
            { LMStringKey.ANIMATE_INTRO, "TODO: Intro to animate" },
            { LMStringKey.ANIMATE_IS_TRIGGERABLE, "If selected, this animation will only animate when triggered via an Effects Controller object. (When editing the animation, this option is ignored.)" },
            { LMStringKey.ANIMATE_DELAY_ON_PARK_LOAD, "When the park is loaded, all animations without this option will start at the same time. This can make things look unnatural.\nSet this option to delay the animation from starting immediately on park-load.\nTODO additional details" },
            { LMStringKey.ANIMATE_STEPS_INTRO, "TODO: Intro to Steps, including step buttons" },
            { LMStringKey.ANIMATE_STEP_NAME, "Give the step a name to help distinguish it from other steps in this animation." },
            { LMStringKey.ANIMATE_START_DELAY, "Time (in seconds) to pause before the step begins." },
            { LMStringKey.ANIMATE_DURATION, "Time (in seconds) that the step should take to compete." },
            { LMStringKey.ANIMATE_EASE, "How the step should animate from its initial values to its final values. See https://easings.net/ for examples." },
            { LMStringKey.ANIMATE_CHANGE_POSITION, "How the objects should move relative to the origin position.\nPositive and negative values can be used.\nTODO: give examples" },
            { LMStringKey.ANIMATE_CHANGE_ROTATION, "How the objects should rotate (in degrees) around the origin.\nPositive and negative values can be used.\nTODO: give examples" },
            { LMStringKey.ANIMATE_CHANGE_SCALE, "How the objects should scale relative to the origin scale.\nPositive and negative values can be used.\nTODO: give examples" },
            { LMStringKey.ANIMATE_END_DELAY, "Time (in seconds) to pause after the step has completed before moving on." },
            { LMStringKey.EDIT_EXISTING_ANIM_NAME, "Change the animation name." },
        };

        public static string GetText(LMStringKey key) {
            return texts.TryGetValue(key, out string text) ? text : "ERROR: Couldn't find string key: " + key.ToString();
        }
    }
}
