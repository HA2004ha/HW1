using Runner.Player;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Runner.EditorTools
{
    /// <summary>
    /// Turns the generated frames into a real Animator state machine: a sprite clip per pose, then
    /// states and transitions driven by the parameters PlayerAnimationDriver writes every frame.
    /// </summary>
    internal static class RunnerAnimations
    {
        private const string ClipFolder = "Assets/Art/Animations";
        private const string ControllerFolder = "Assets/Art/AnimatorControllers";
        private const string ControllerPath = ControllerFolder + "/Runner.controller";
        private const string VisualPath = "Visual";

        public static RuntimeAnimatorController Build()
        {
            SpriteWriter.EnsureFolder(ClipFolder);
            SpriteWriter.EnsureFolder(ControllerFolder);

            var run = Clip("Runner_Run", RunnerCharacterArt.Run, 13f, true);
            var jump = Clip("Runner_Jump", new[] { RunnerCharacterArt.Jump }, 12f, false);
            var fall = Clip("Runner_Fall", new[] { RunnerCharacterArt.Fall }, 12f, true);
            var dash = Clip("Runner_Dash", new[] { RunnerCharacterArt.Run[2] }, 12f, false);
            var hit = Clip("Runner_Hit", new[] { RunnerCharacterArt.Hit }, 10f, false);
            var death = Clip("Runner_Death", RunnerCharacterArt.Death, 7f, false);

            return BuildController(run, jump, fall, dash, hit, death);
        }

        private static AnimationClip Clip(string name, Sprite[] frames, float fps, bool loop)
        {
            var path = $"{ClipFolder}/{name}.anim";
            var clip = new AnimationClip
            {
                name = name,
                legacy = false,
                frameRate = fps,
                wrapMode = loop ? WrapMode.Loop : WrapMode.Default
            };

            var step = 1f / Mathf.Max(1f, fps);
            var keys = new ObjectReferenceKeyframe[frames.Length + 1];

            for (var i = 0; i < frames.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = i * step, value = frames[i] };
            }

            keys[frames.Length] = new ObjectReferenceKeyframe
            {
                time = frames.Length * step,
                value = frames[frames.Length - 1]
            };

            AnimationUtility.SetObjectReferenceCurve(clip,
                EditorCurveBinding.PPtrCurve(VisualPath, typeof(SpriteRenderer), "m_Sprite"), keys);

            if (loop)
            {
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = true;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
            }

            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(clip, path);
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        }

        private static AnimatorController BuildController(
            Motion run, Motion jump, Motion fall, Motion dash, Motion hit, Motion death)
        {
            AssetDatabase.DeleteAsset(ControllerPath);

            var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
            controller.AddParameter(AnimatorHash.GroundedName, AnimatorControllerParameterType.Bool);
            controller.AddParameter(AnimatorHash.RisingName, AnimatorControllerParameterType.Bool);
            controller.AddParameter(AnimatorHash.SpeedName, AnimatorControllerParameterType.Float);
            controller.AddParameter(AnimatorHash.JumpName, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(AnimatorHash.DashName, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(AnimatorHash.HitName, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(AnimatorHash.DeathName, AnimatorControllerParameterType.Trigger);

            var machine = controller.layers[0].stateMachine;
            var sRun = State(machine, run, 0f, 0f);
            var sJump = State(machine, jump, 280f, 40f);
            var sFall = State(machine, fall, 560f, 0f);
            var sDash = State(machine, dash, 280f, -180f);
            var sHit = State(machine, hit, -280f, 40f);
            var sDeath = State(machine, death, -280f, 300f);

            machine.defaultState = sRun;

            machine.AnyTransitionTo(sDeath).Trigger(AnimatorHash.DeathName);
            machine.AnyTransitionTo(sHit).Trigger(AnimatorHash.HitName);
            machine.AnyTransitionTo(sDash).Trigger(AnimatorHash.DashName);

            sRun.TransitionTo(sJump).Trigger(AnimatorHash.JumpName);
            sRun.ExitTo(sFall).Bool(AnimatorHash.GroundedName, false);

            sJump.ExitTo(sFall).Bool(AnimatorHash.RisingName, false);
            sJump.ExitTo(sRun).Bool(AnimatorHash.GroundedName, true);

            sFall.ExitTo(sRun).Bool(AnimatorHash.GroundedName, true);
            sFall.TransitionTo(sJump).Trigger(AnimatorHash.JumpName);

            sDash.ExitTo(sRun).Bool(AnimatorHash.GroundedName, true);
            sDash.ExitTo(sFall).Bool(AnimatorHash.GroundedName, false);

            sHit.ExitTo(sRun).Bool(AnimatorHash.GroundedName, true);
            sHit.ExitTo(sFall).Bool(AnimatorHash.GroundedName, false);

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorState State(AnimatorStateMachine machine, Motion motion, float x, float y)
        {
            var state = machine.AddState(motion.name, new Vector3(x, y, 0f));
            state.motion = motion;
            state.writeDefaultValues = false;
            return state;
        }

        private static AnimatorStateTransition AnyTransitionTo(this AnimatorStateMachine machine, AnimatorState target)
        {
            var transition = machine.AddAnyStateTransition(target);
            transition.hasExitTime = false;
            transition.duration = 0.04f;
            transition.canTransitionToSelf = false;
            return transition;
        }

        private static AnimatorStateTransition TransitionTo(this AnimatorState from, AnimatorState to)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = false;
            transition.exitTime = 0f;
            transition.duration = 0.06f;
            transition.canTransitionToSelf = false;
            return transition;
        }

        private static AnimatorStateTransition ExitTo(this AnimatorState from, AnimatorState to)
        {
            var transition = from.AddTransition(to);
            transition.hasExitTime = true;
            transition.exitTime = 0.8f;
            transition.duration = 0.08f;
            return transition;
        }

        private static AnimatorStateTransition Trigger(this AnimatorStateTransition transition, string name)
        {
            transition.AddCondition(AnimatorConditionMode.If, 0f, name);
            return transition;
        }

        private static AnimatorStateTransition Bool(this AnimatorStateTransition transition, string name, bool value)
        {
            transition.AddCondition(value ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0f, name);
            return transition;
        }
    }
}
