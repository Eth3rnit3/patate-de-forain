using Patate.Combat;

namespace Patate.Content
{
    /// <summary>
    /// Choix de l'image à afficher pour un état de simulation. Le rendu observe :
    /// il lit l'action et son tick, il ne décide jamais de la durée d'un coup.
    /// Logique pure, testable sans moteur graphique.
    /// </summary>
    public static class FramePicker
    {
        public readonly struct Pick
        {
            public readonly string Animation;
            public readonly int FrameIndex;
            public readonly string Sprite;

            public Pick(string animation, int frameIndex, string sprite)
            {
                Animation = animation; FrameIndex = frameIndex; Sprite = sprite;
            }
        }

        /// <summary>État facultatif → état à utiliser s'il manque (lui-même éventuellement remplacé).</summary>
        public static readonly System.Collections.Generic.IReadOnlyDictionary<FighterVisualState, FighterVisualState> Fallback =
            new System.Collections.Generic.Dictionary<FighterVisualState, FighterVisualState>
            {
                [FighterVisualState.Crouch] = FighterVisualState.Idle,
                [FighterVisualState.CrouchGuard] = FighterVisualState.Guard,
                [FighterVisualState.CrouchHitstun] = FighterVisualState.Hitstun,
                [FighterVisualState.CrouchBlockstun] = FighterVisualState.Blockstun,
                [FighterVisualState.PreJump] = FighterVisualState.Crouch,
                [FighterVisualState.Jump] = FighterVisualState.Idle,
                [FighterVisualState.Land] = FighterVisualState.PreJump,
                [FighterVisualState.AirHitstun] = FighterVisualState.Hitstun,
            };

        public static Pick ForFighter(FighterState fighter, PresentationDefinition presentation)
        {
            var action = fighter.Action;
            bool crouched = fighter.Posture == Posture.Crouch;
            int duration = 0;
            FighterVisualState state;
            switch (action.Kind)
            {
                case ActionKind.Move:
                    if (presentation.Moves.TryGetValue(action.MoveId ?? "", out var move)) return Resolve(move, presentation, action.Tick);
                    state = FighterVisualState.Idle;
                    break;
                case ActionKind.Walk:
                    state = action.Forward ? FighterVisualState.WalkForward : FighterVisualState.WalkBack;
                    break;
                case ActionKind.Guard:
                    state = crouched ? FighterVisualState.CrouchGuard : FighterVisualState.Guard;
                    break;
                case ActionKind.Hitstun:
                    state = fighter.Posture == Posture.Air ? FighterVisualState.AirHitstun : crouched ? FighterVisualState.CrouchHitstun : FighterVisualState.Hitstun;
                    duration = action.Duration;
                    break;
                case ActionKind.Blockstun:
                    state = crouched ? FighterVisualState.CrouchBlockstun : FighterVisualState.Blockstun;
                    duration = action.Duration;
                    break;
                case ActionKind.KO:
                    state = FighterVisualState.KO;
                    break;
                case ActionKind.Crouch:
                    state = FighterVisualState.Crouch;
                    break;
                case ActionKind.PreJump:
                    state = FighterVisualState.PreJump;
                    break;
                case ActionKind.Jump:
                    state = FighterVisualState.Jump;
                    break;
                case ActionKind.Land:
                    state = FighterVisualState.Land;
                    duration = action.Duration;
                    break;
                default:
                    state = FighterVisualState.Idle;
                    break;
            }
            return Resolve(Binding(presentation, state), presentation, action.Tick, duration);
        }

        /// <summary>Animation de l'état, ou de l'état de repli s'il n'est pas fourni.</summary>
        public static AnimationBinding Binding(PresentationDefinition presentation, FighterVisualState state)
        {
            while (!presentation.States.ContainsKey(state) && Fallback.TryGetValue(state, out var next)) state = next;
            return presentation.States[state];
        }

        public static Pick Resolve(AnimationBinding binding, PresentationDefinition presentation, int tick, int stateDuration = 0)
        {
            var animation = presentation.Animations[binding.Animation];
            int count = animation.Frames.Count;
            int index;
            switch (binding.Playback)
            {
                case Playback.Hold:
                    index = binding.Frame;
                    break;
                case Playback.Stretch:
                    index = stateDuration <= 0 ? 0 : System.Math.Min(count - 1, tick * count / stateDuration);
                    break;
                case Playback.Loop:
                    index = IndexAt(animation, Modulo(tick, animation.TotalDuration));
                    break;
                case Playback.ReverseLoop:
                    index = count - 1 - IndexAt(animation, Modulo(tick, animation.TotalDuration));
                    break;
                default: // Once : reste sur la dernière image une fois terminée.
                    index = IndexAt(animation, tick);
                    break;
            }
            index = System.Math.Max(0, System.Math.Min(count - 1, index));
            return new Pick(binding.Animation, index, animation.Frames[index].Sprite);
        }

        /// <summary>Image d'une animation à <paramref name="tick"/>, d'après les durées cumulées.</summary>
        public static int IndexAt(AnimationDefinition animation, int tick)
        {
            int elapsed = 0;
            for (int i = 0; i < animation.Frames.Count; i++)
            {
                elapsed += animation.Frames[i].Duration;
                if (tick < elapsed) return i;
            }
            return animation.Frames.Count - 1;
        }

        static int Modulo(int value, int length) => length <= 0 ? 0 : ((value % length) + length) % length;
    }
}
