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

        public static Pick ForFighter(FighterState fighter, PresentationDefinition presentation)
        {
            var action = fighter.Action;
            AnimationBinding binding;
            int duration = 0;
            switch (action.Kind)
            {
                case ActionKind.Move:
                    if (!presentation.Moves.TryGetValue(action.MoveId ?? "", out binding)) binding = presentation.States[FighterVisualState.Idle];
                    break;
                case ActionKind.Walk:
                    binding = presentation.States[action.Forward ? FighterVisualState.WalkForward : FighterVisualState.WalkBack];
                    break;
                case ActionKind.Guard:
                    binding = presentation.States[FighterVisualState.Guard];
                    break;
                case ActionKind.Hitstun:
                    binding = presentation.States[FighterVisualState.Hitstun];
                    duration = action.Duration;
                    break;
                case ActionKind.Blockstun:
                    binding = presentation.States[FighterVisualState.Blockstun];
                    duration = action.Duration;
                    break;
                case ActionKind.KO:
                    binding = presentation.States[FighterVisualState.KO];
                    break;
                default:
                    binding = presentation.States[FighterVisualState.Idle];
                    break;
            }
            return Resolve(binding, presentation, action.Tick, duration);
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
