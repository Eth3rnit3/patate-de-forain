using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Patate.Combat;
using UnityEngine;

namespace Patate.UnityAdapters
{
    /// <summary>
    /// Traduit les événements de combat en signaux sonores. Aucun son n'est
    /// livré pour l'instant : les signaux sans fichier sont simplement comptés
    /// (visibles dans l'affichage de débogage). Brancher un son = renseigner
    /// Resources/Audio/sound-bank.json, ou presentation.json › sounds pour un
    /// son propre à un combattant ; aucun code à modifier.
    /// </summary>
    public sealed class AudioCues
    {
        readonly AudioSource source;
        AudioSource musicSource;
        readonly Dictionary<string, AudioClip> common = new Dictionary<string, AudioClip>();
        readonly FighterVisuals[] fighters;
        readonly Dictionary<string, AudioClip> fighterClips = new Dictionary<string, AudioClip>();
        public readonly HashSet<string> Missing = new HashSet<string>();

        public AudioCues(GameObject host, FighterVisuals first, FighterVisuals second, AudioClip music)
        {
            source = host.AddComponent<AudioSource>();
            fighters = new[] { first, second };
            var bank = Resources.Load<TextAsset>("Audio/sound-bank");
            if (bank != null)
            {
                foreach (var cue in (JObject)JObject.Parse(bank.text)["cues"])
                {
                    var file = cue.Value?.Type == JTokenType.String ? (string)cue.Value : null;
                    common[cue.Key] = file == null ? null : Resources.Load<AudioClip>($"Audio/{file}");
                }
            }
            if (music != null)
            {
                musicSource = host.AddComponent<AudioSource>();
                musicSource.clip = music;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        /// <summary>Retire les sources audio de l'hôte (fin du combat).</summary>
        public void Dispose()
        {
            Object.Destroy(source);
            if (musicSource != null) Object.Destroy(musicSource);
        }

        public void SetFighters(FighterVisuals first, FighterVisuals second)
        {
            fighters[0] = first;
            fighters[1] = second;
        }

        public void OnEvents(IReadOnlyList<MatchEvent> events)
        {
            foreach (var e in events)
            {
                switch (e.Type)
                {
                    case MatchEventType.RoundStart: Play("roundStart"); break;
                    case MatchEventType.MoveStart: Play("moveStart", e.Slot, $"moveStart:{e.MoveId}"); break;
                    case MatchEventType.Hit: Play(e.Damage >= 80 ? "hitHeavy" : "hit", e.Slot, e.MoveId != null ? $"hit:{e.MoveId}" : null); break;
                    case MatchEventType.Block: Play("block"); break;
                    case MatchEventType.ProjectileSpawn: Play("projectileSpawn", e.Slot, $"projectileSpawn:{e.ProjectileId}"); break;
                    case MatchEventType.ProjectileEnd when e.Reason != ProjectileEndReason.Expired && e.Reason != ProjectileEndReason.OutOfArena:
                        Play("projectileImpact", e.Slot, $"projectileImpact:{e.ProjectileId}");
                        break;
                    case MatchEventType.KO: Play("ko", e.Slot, "ko"); break;
                    case MatchEventType.TimeOver: Play("timeOver"); break;
                }
            }
        }

        /// <summary>Son propre au combattant s'il existe, sinon son commun, sinon rien.</summary>
        void Play(string cue, int slot = -1, string fighterCue = null)
        {
            var clip = slot >= 0 && fighterCue != null ? FighterClip(fighters[slot], fighterCue) : null;
            if (clip == null && common.TryGetValue(cue, out var shared)) clip = shared;
            if (clip != null) source.PlayOneShot(clip);
            else Missing.Add(fighterCue ?? cue);
        }

        AudioClip FighterClip(FighterVisuals fighter, string cue)
        {
            if (!fighter.Package.Presentation.Sounds.TryGetValue(cue, out var file) || file == null) return null;
            var key = $"{fighter.Package.Id}/{file}";
            if (!fighterClips.TryGetValue(key, out var clip))
                fighterClips[key] = clip = Resources.Load<AudioClip>(ResourcesFighterSource.ResourcePath(fighter.Package.Id, file));
            return clip;
        }
    }
}
