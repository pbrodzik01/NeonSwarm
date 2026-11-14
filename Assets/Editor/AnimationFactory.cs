using System.IO;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace NeonSwarm.EditorTools
{
    internal sealed class AnimationSet
    {
        public AnimatorController Player;
        public AnimatorController Drifter;
        public AnimatorOverrideController Chaser;
        public AnimatorOverrideController Shooter;
    }

    internal static class AnimationFactory
    {
        private const string Folder = "Assets/Animations";

        public static AnimationSet Create(SpriteSet sprites)
        {
            Directory.CreateDirectory(Folder + "/Player");
            Directory.CreateDirectory(Folder + "/Enemy");

            var playerIdle = Clip("Player/Player_Idle", 0.4f, true, c =>
            {
                Sprites(c, "Visual/Thruster", 0.1f, sprites.ThrusterSmall0, sprites.ThrusterSmall1, sprites.ThrusterSmall0, sprites.ThrusterSmall1, sprites.ThrusterSmall0);
                Scale(c, "Visual/Thruster", (0f, 0.9f), (0.2f, 1f), (0.4f, 0.9f));
            });
            var playerRun = Clip("Player/Player_Run", 0.24f, true, c =>
            {
                Sprites(c, "Visual/Thruster", 0.06f, sprites.ThrusterBig0, sprites.ThrusterBig1, sprites.ThrusterBig0, sprites.ThrusterBig1, sprites.ThrusterBig0);
                Scale(c, "Visual/Thruster", (0f, 1.25f), (0.12f, 1.1f), (0.24f, 1.25f));
            });
            var playerAttack = Clip("Player/Player_Attack", 0.14f, false, c =>
            {
                Alpha(c, "Visual/Muzzle", (0f, 1f), (0.14f, 0f));
                Scale(c, "Visual/Muzzle", (0f, 0.6f), (0.14f, 1.2f));
                Scale(c, "Visual", (0f, 1f), (0.05f, 0.9f), (0.14f, 1f));
            });
            var playerHurt = Clip("Player/Player_Hurt", 0.3f, false, c =>
            {
                Alpha(c, "Visual/Flash", (0f, 0.95f), (0.3f, 0f));
                Scale(c, "Visual", (0f, 1f), (0.08f, 1.2f), (0.3f, 1f));
            });
            var playerDead = Clip("Player/Player_Dead", 0.7f, false, c =>
            {
                Alpha(c, "Visual/Flash", (0f, 1f), (0.7f, 0.6f));
                Scale(c, "Visual", (0f, 1f), (0.2f, 1.5f), (0.7f, 0f));
                Scale(c, "Visual/Thruster", (0f, 0f), (0.7f, 0f));
            });

            var enemyIdle = Clip("Enemy/Enemy_Idle", 0.8f, true, c => Scale(c, "Visual/Core", (0f, 0.8f), (0.4f, 1.1f), (0.8f, 0.8f)));
            var enemyMove = Clip("Enemy/Enemy_Move", 0.4f, true, c =>
            {
                Scale(c, "Visual/Core", (0f, 0.9f), (0.2f, 1.3f), (0.4f, 0.9f));
                Scale(c, "Visual/Body", (0f, 1f), (0.2f, 1.05f), (0.4f, 1f));
            });
            var enemyAttack = Clip("Enemy/Enemy_Attack", 0.35f, false, c =>
            {
                Scale(c, "Visual", (0f, 1f), (0.12f, 1.25f), (0.35f, 1f));
                Scale(c, "Visual/Core", (0f, 1f), (0.12f, 1.7f), (0.35f, 1f));
            });
            var enemyHurt = Clip("Enemy/Enemy_Hurt", 0.2f, false, c =>
            {
                Alpha(c, "Visual/Flash", (0f, 0.9f), (0.2f, 0f));
                Scale(c, "Visual", (0f, 1.15f), (0.2f, 1f));
            });
            var enemyDead = Clip("Enemy/Enemy_Dead", 0.38f, false, c =>
            {
                Alpha(c, "Visual/Flash", (0f, 1f), (0.38f, 0f));
                Scale(c, "Visual", (0f, 1f), (0.1f, 1.4f), (0.38f, 0f));
            });
            var chaserAttack = Clip("Enemy/Chaser_Dash", 0.35f, false, c =>
            {
                ScaleXY(c, "Visual", (0f, 1f, 1f), (0.08f, 0.75f, 1.35f), (0.35f, 1f, 1f));
                Alpha(c, "Visual/Flash", (0f, 0.5f), (0.35f, 0f));
            });
            var shooterAttack = Clip("Enemy/Shooter_Charge", 0.45f, false, c =>
            {
                Scale(c, "Visual/Core", (0f, 1f), (0.38f, 2.1f), (0.45f, 0.8f));
                Alpha(c, "Visual/Flash", (0f, 0f), (0.38f, 0.6f), (0.45f, 0f));
            });

            var set = new AnimationSet
            {
                Player = Controller("Player/Player.controller", playerIdle, playerRun, playerAttack, playerHurt, playerDead),
                Drifter = Controller("Enemy/Enemy.controller", enemyIdle, enemyMove, enemyAttack, enemyHurt, enemyDead)
            };
            set.Chaser = Override("Enemy/Chaser.overrideController", set.Drifter, enemyAttack, chaserAttack);
            set.Shooter = Override("Enemy/Shooter.overrideController", set.Drifter, enemyAttack, shooterAttack);
            AssetDatabase.SaveAssets();
            return set;
        }

        private static AnimatorController Controller(string relativePath, AnimationClip idle, AnimationClip run, AnimationClip attack, AnimationClip hurt, AnimationClip dead)
        {
            var path = $"{Folder}/{relativePath}";
            AssetDatabase.DeleteAsset(path);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Dead", AnimatorControllerParameterType.Bool);

            var machine = controller.layers[0].stateMachine;
            var idleState = machine.AddState("Idle", new Vector3(300f, 0f));
            idleState.motion = idle;
            var runState = machine.AddState("Run", new Vector3(300f, 120f));
            runState.motion = run;
            var attackState = machine.AddState("Attack", new Vector3(560f, 60f));
            attackState.motion = attack;
            var hurtState = machine.AddState("Hurt", new Vector3(560f, 180f));
            hurtState.motion = hurt;
            var deadState = machine.AddState("Dead", new Vector3(560f, 300f));
            deadState.motion = dead;
            machine.defaultState = idleState;

            var toRun = idleState.AddTransition(runState);
            Instant(toRun);
            toRun.AddCondition(AnimatorConditionMode.Greater, 0.1f, "Speed");
            var toIdle = runState.AddTransition(idleState);
            Instant(toIdle);
            toIdle.AddCondition(AnimatorConditionMode.Less, 0.1f, "Speed");

            var toDead = machine.AddAnyStateTransition(deadState);
            Instant(toDead);
            toDead.canTransitionToSelf = false;
            toDead.AddCondition(AnimatorConditionMode.If, 0f, "Dead");

            var toHurt = machine.AddAnyStateTransition(hurtState);
            Instant(toHurt);
            toHurt.AddCondition(AnimatorConditionMode.If, 0f, "Hurt");
            toHurt.AddCondition(AnimatorConditionMode.IfNot, 0f, "Dead");

            var toAttack = machine.AddAnyStateTransition(attackState);
            Instant(toAttack);
            toAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");
            toAttack.AddCondition(AnimatorConditionMode.IfNot, 0f, "Dead");

            foreach (var state in new[] { attackState, hurtState })
            {
                var back = state.AddTransition(idleState);
                back.hasExitTime = true;
                back.exitTime = 1f;
                back.duration = 0f;
            }
            return controller;
        }

        private static AnimatorOverrideController Override(string relativePath, AnimatorController baseController, AnimationClip original, AnimationClip replacement)
        {
            var path = $"{Folder}/{relativePath}";
            AssetDatabase.DeleteAsset(path);
            var overrideController = new AnimatorOverrideController(baseController) { name = Path.GetFileNameWithoutExtension(path) };
            overrideController[original] = replacement;
            AssetDatabase.CreateAsset(overrideController, path);
            return overrideController;
        }

        private static void Instant(AnimatorStateTransition transition)
        {
            transition.hasExitTime = false;
            transition.duration = 0f;
        }

        private static AnimationClip Clip(string relativePath, float length, bool loop, System.Action<AnimationClip> build)
        {
            var path = $"{Folder}/{relativePath}.anim";
            AssetDatabase.DeleteAsset(path);
            var clip = new AnimationClip { frameRate = 30f };
            build(clip);
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = loop;
            settings.stopTime = length;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, path);
            return clip;
        }

        private static void Sprites(AnimationClip clip, string path, float frameTime, params Sprite[] frames)
        {
            var keys = new ObjectReferenceKeyframe[frames.Length];
            for (var i = 0; i < frames.Length; i++)
            {
                keys[i] = new ObjectReferenceKeyframe { time = i * frameTime, value = frames[i] };
            }
            var binding = EditorCurveBinding.PPtrCurve(path, typeof(SpriteRenderer), "m_Sprite");
            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);
        }

        private static void Scale(AnimationClip clip, string path, params (float time, float value)[] keys)
        {
            var curve = Curve(keys);
            clip.SetCurve(path, typeof(Transform), "localScale.x", curve);
            clip.SetCurve(path, typeof(Transform), "localScale.y", curve);
            clip.SetCurve(path, typeof(Transform), "localScale.z", AnimationCurve.Constant(0f, keys[keys.Length - 1].time, 1f));
        }

        private static void ScaleXY(AnimationClip clip, string path, params (float time, float x, float y)[] keys)
        {
            var x = new AnimationCurve();
            var y = new AnimationCurve();
            foreach (var key in keys)
            {
                x.AddKey(key.time, key.x);
                y.AddKey(key.time, key.y);
            }
            clip.SetCurve(path, typeof(Transform), "localScale.x", x);
            clip.SetCurve(path, typeof(Transform), "localScale.y", y);
            clip.SetCurve(path, typeof(Transform), "localScale.z", AnimationCurve.Constant(0f, keys[keys.Length - 1].time, 1f));
        }

        private static void Alpha(AnimationClip clip, string path, params (float time, float value)[] keys)
        {
            clip.SetCurve(path, typeof(SpriteRenderer), "m_Color.a", Curve(keys));
        }

        private static AnimationCurve Curve((float time, float value)[] keys)
        {
            var curve = new AnimationCurve();
            foreach (var key in keys)
            {
                curve.AddKey(key.time, key.value);
            }
            return curve;
        }
    }
}
