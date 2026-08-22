using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AshesOfRyu
{
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private TextAsset jsonAsset;
        [SerializeField] private Texture2D spriteSheet;

        private SpriteRenderer sr;
        private Dictionary<string, Sprite[]> animations;
        private Coroutine activeRoutine;
        private string currentAnim;
        private bool oneShotActive;

        private PlayerController controller;
        private PlayerVisuals visuals;

        private const string DefaultAnim = "Sitting Idle";
        private const float LoopFrameTime = 0.1f;
        private const float DeathFrameTime = 0.18f;

        public void Init(Texture2D texture, string json)
        {
            spriteSheet = texture;
            sr = GetComponent<SpriteRenderer>();
            controller = GetComponent<PlayerController>();
            visuals = GetComponent<PlayerVisuals>();
            animations = new Dictionary<string, Sprite[]>();
            ParseJson(json);
            PlayAnim(DefaultAnim, true);
        }

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            controller = GetComponent<PlayerController>();
            visuals = GetComponent<PlayerVisuals>();
            animations = new Dictionary<string, Sprite[]>();
            if (spriteSheet != null && jsonAsset != null)
            {
                ParseJson(jsonAsset.text);
                PlayAnim(DefaultAnim, true);
            }
        }

        private void LateUpdate()
        {
            if (controller == null) { controller = GetComponent<PlayerController>(); if (controller == null) return; }
            if (sr == null || animations == null || animations.Count == 0) return;

            sr.flipX = controller.Facing.x < -0.01f;

            // Los one-shots (Attack/Damage/Death/Fall) se dejan terminar solos.
            if (oneShotActive) return;
            // Muerto: quedarse en el ultimo frame de Death, sin volver al idle.
            if (visuals != null && visuals.IsDead) return;

            bool moving = controller.Velocity.sqrMagnitude > 0.06f;
            bool dragon = visuals != null && visuals.IsDragonForm;
            string desired = moving || dragon ? "Flight" : DefaultAnim;
            if (!animations.ContainsKey(desired)) desired = DefaultAnim;
            if (currentAnim != desired) PlayAnim(desired, true);
        }

        public void PlayIdle() => PlayAnim(DefaultAnim, true);
        public void PlayAttack() => PlayAnim("Attack", false);
        public void PlayDamage() => PlayAnim("Damage", false);
        public void PlayDeath() => PlayAnim("Death", false, DeathFrameTime);
        public void PlayFlight() => PlayAnim("Flight", true);
        public void PlayFall() => PlayAnim("Fall", false);

        public void PlayAnim(string animName, bool loop) => PlayAnim(animName, loop, LoopFrameTime);

        public void PlayAnim(string animName, bool loop, float frameTime)
        {
            if (animations == null || !animations.ContainsKey(animName)) return;
            if (activeRoutine != null) StopCoroutine(activeRoutine);
            currentAnim = animName;
            oneShotActive = !loop;
            activeRoutine = StartCoroutine(PlaySequence(animations[animName], loop, frameTime));
        }

        private IEnumerator PlaySequence(Sprite[] frames, bool loop, float frameTime)
        {
            do
            {
                for (int i = 0; i < frames.Length; i++)
                {
                    sr.sprite = frames[i];
                    yield return new WaitForSeconds(frameTime);
                }
            }
            while (loop);

            oneShotActive = false;
        }

        private void ParseJson(string json)
        {
            if (spriteSheet == null) return;

            var grouped = new Dictionary<string, List<SpriteEntry>>();

            int framesIdx = json.IndexOf("\"frames\"");
            if (framesIdx < 0) return;
            int objStart = json.IndexOf('{', framesIdx + 8);
            if (objStart < 0) return;

            int depth = 0;
            int pos = objStart;
            while (pos < json.Length)
            {
                if (json[pos] == '{') depth++;
                else if (json[pos] == '}')
                {
                    depth--;
                    if (depth == 0) break;
                }
                else if (json[pos] == '"' && depth == 1)
                {
                    int keyEnd = json.IndexOf('"', pos + 1);
                    if (keyEnd < 0) break;
                    string frameKey = json.Substring(pos + 1, keyEnd - pos - 1);

                    int colon = json.IndexOf(':', keyEnd + 1);
                    int innerStart = json.IndexOf('{', colon + 1);
                    if (innerStart < 0) break;

                    string frameObj = ExtractBalanced(json, innerStart);
                    if (frameObj == null) break;

                    int frameKeyIdx = frameObj.IndexOf("\"frame\"");
                    if (frameKeyIdx < 0) { pos = innerStart + frameObj.Length; continue; }
                    int frameObjStart = frameObj.IndexOf('{', frameKeyIdx + 7);
                    if (frameObjStart < 0) { pos = innerStart + frameObj.Length; continue; }
                    string frameRect = ExtractBalanced(frameObj, frameObjStart);
                    if (frameRect == null) { pos = innerStart + frameObj.Length; continue; }

                    int x = ExtractInt(frameRect, "\"x\"");
                    int y = ExtractInt(frameRect, "\"y\"");
                    int w = ExtractInt(frameRect, "\"w\"");
                    int h = ExtractInt(frameRect, "\"h\"");

                    string animName = ExtractAnimName(frameKey);
                    int frameNum = ExtractFrameNumber(frameKey);

                    if (!grouped.ContainsKey(animName))
                        grouped[animName] = new List<SpriteEntry>();
                    grouped[animName].Add(new SpriteEntry(frameNum, x, y, w, h));

                    pos = innerStart + frameObj.Length;
                    continue;
                }
                pos++;
            }

            foreach (var kvp in grouped)
            {
                kvp.Value.Sort((a, b) => a.frameNum.CompareTo(b.frameNum));

                var sprites = new Sprite[kvp.Value.Count];
                for (int i = 0; i < kvp.Value.Count; i++)
                {
                    var e = kvp.Value[i];
                    Rect rect = new Rect(e.x, spriteSheet.height - e.y - e.h, e.w, e.h);
                    sprites[i] = Sprite.Create(spriteSheet, rect, new Vector2(0.5f, 0.5f), 100f);
                }
                animations[kvp.Key] = sprites;
            }
        }

        private string ExtractBalanced(string json, int start)
        {
            if (json[start] != '{') return null;
            int depth = 0;
            for (int i = start; i < json.Length; i++)
            {
                if (json[i] == '{') depth++;
                else if (json[i] == '}')
                {
                    depth--;
                    if (depth == 0)
                        return json.Substring(start, i - start + 1);
                }
            }
            return null;
        }

        private int ExtractInt(string obj, string key)
        {
            int idx = obj.IndexOf(key);
            if (idx < 0) return 0;
            int colon = obj.IndexOf(':', idx + key.Length);
            int start = colon + 1;
            while (start < obj.Length && (obj[start] == ' ' || obj[start] == '\t')) start++;
            int end = start;
            while (end < obj.Length && (char.IsDigit(obj[end]) || obj[end] == '-')) end++;
            int.TryParse(obj.Substring(start, end - start), out int val);
            return val;
        }

        private string ExtractAnimName(string frameName)
        {
            int start = frameName.LastIndexOf('(');
            int end = frameName.LastIndexOf(')');
            if (start >= 0 && end > start)
                return frameName.Substring(start + 1, end - start - 1).Trim();
            return frameName;
        }

        private int ExtractFrameNumber(string frameName)
        {
            int dot = frameName.LastIndexOf(".ase");
            if (dot < 0) dot = frameName.LastIndexOf('.');
            if (dot < 0) return 0;
            int space = frameName.LastIndexOf(' ', dot);
            if (space < 0) return 0;
            string numStr = frameName.Substring(space + 1, dot - space - 1);
            int.TryParse(numStr, out int num);
            return num;
        }

        private class SpriteEntry
        {
            public int frameNum, x, y, w, h;
            public SpriteEntry(int frameNum, int x, int y, int w, int h)
            {
                this.frameNum = frameNum;
                this.x = x; this.y = y;
                this.w = w; this.h = h;
            }
        }
    }
}
