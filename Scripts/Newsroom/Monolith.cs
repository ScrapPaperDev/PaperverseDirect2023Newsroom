using System.Collections.Generic;
using UnityEngine;
using Paperverse.TimelineExtensions;
using UnityEngine.Timeline;
using UnityEngine.Playables;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace Paperverse
{
    public class Monolith : MonoBehaviour
    {
        private static Monolith instance;

        [HideInInspector] public bool directorWasOn;

        [Header("Pepper")]
        public Pepper pepper;

        [SerializeField, Header("News Room Events")]
        private TextureSetter screenTexture;

        [SerializeField, Header("Test Stuff")]
        private Animator camAnim;

        [field: SerializeField] public Texture2D[] screenSlides { get; private set; }
        public int currentSlide { get; private set; }

        public Transform browGuy;
        public ParticleSystem testEmote;
        public CharacterAnimator flattoCatto;

        public static Monolith Instance
        {
            get
            {
                //-- So playing stuff in edit mode will work.
                if (instance == null)
                    instance = FindObjectOfType<Monolith>();

                return instance;
            }
        }

        private void Awake()
        {
            instance = this;
            directorWasOn = GetComponent<UnityEngine.Playables.PlayableDirector>().enabled;
        }

        public void UpdateScreenTex(int dir)
        {
            currentSlide += dir;
            currentSlide %= screenSlides.Length;
            if (currentSlide < 0)
                currentSlide = screenSlides.Length - 1;
            var nextTex = screenSlides[currentSlide];
            screenTexture.UpdateTexture(nextTex);
        }


        public void StopScreenSlide() => screenTexture.GetComponent<TexScroll>().Release();
        public void UpdateScreenTex(Texture2D tex) => screenTexture.UpdateTexture(tex);
        public void PlayCameraAnim(string trigName) => camAnim.SetTrigger(trigName);
        public void TestEmote() => testEmote.Play();

        public void PlayConfettiParticles()
        {
        }

        public TimelineClip GetClipFromPlayable(Type trackType, PlayableAsset playableType)
        {
            var asset = GetComponent<PlayableDirector>().playableAsset as TimelineAsset;

            foreach (var track in asset.GetOutputTracks())
            {
                if (track.GetType() == trackType)
                {
                    foreach (var clip in track.GetClips())
                    {
                        if (clip.asset == playableType)
                            return clip;
                    }
                }
            }

            throw new KeyNotFoundException("thats no good");
        }


        public T GetPlayableInstanceFromClip<T>(Type trackType, PlayableAsset playableType) where T : PlayableAsset
        {
            var asset = GetComponent<PlayableDirector>().playableAsset as TimelineAsset;

            foreach (var track in asset.GetOutputTracks())
            {
                if (track.GetType() == trackType)
                {
                    foreach (var clip in track.GetClips())
                    {
                        if (clip.asset == playableType)
                            return clip.asset as T;
                    }
                }
            }

            throw new KeyNotFoundException("thats no good");
        }


        public bool ValidatePlayableType(CharacterPlayableAsset charClip)
        {
            var asset = GetComponent<PlayableDirector>().playableAsset as TimelineAsset;

            foreach (var item in asset.GetOutputTracks())
            {
                if (item is CharacterTrack p)
                {
                    foreach (var clip in item.GetClips())
                    {
                        if (clip.asset is CharacterPlayableAsset pp)
                            if (pp == charClip)
                                return p.trackType == pp.characterModifier;
                    }
                }
            }
            return false;
        }

        public CharacterModifiers GetTrackType(CharacterPlayableAsset charClip)
        {
            var asset = GetComponent<PlayableDirector>().playableAsset as TimelineAsset;

            foreach (var item in asset.GetOutputTracks())
            {
                if (item is CharacterTrack p)
                {
                    foreach (var clip in item.GetClips())
                    {
                        if (clip.asset is CharacterPlayableAsset pp)
                            if (pp == charClip)
                                return p.trackType;
                    }
                }
            }
            throw new KeyNotFoundException("huhhhh????");
        }




        public void PlayEvent(NewsRoomEvent eve)
        {
            if (eve.theEvent == 0)
                return;

            if (eve.theEvent.HasFlag(NewsRoomEvents.NextSlide))
                UpdateScreenTex(1);

            if (eve.theEvent.HasFlag(NewsRoomEvents.PreviousSlide))
                UpdateScreenTex(-1);

            if (eve.theEvent.HasFlag(NewsRoomEvents.SetScreenImage))
                UpdateScreenTex(eve.tex);

            if (eve.theEvent.HasFlag(NewsRoomEvents.PlayParticles))
                eve.particleSystem.Play();

            if (eve.theEvent.HasFlag(NewsRoomEvents.FlatCat_PepperBit))
                FindObjectOfType<FlatCatShenanigans>().PepperBit(0);

            if (eve.theEvent.HasFlag(NewsRoomEvents.FlatCat_PepperBit2))
                FindObjectOfType<FlatCatShenanigans>().PepperBit(1);

            if (eve.theEvent.HasFlag(NewsRoomEvents.FlatCatStartSlides))
                FindObjectOfType<FlatCatShenanigans>().StartSlides();

            if (eve.theEvent.HasFlag(NewsRoomEvents.ChangeCaption))
                FindObjectOfType<FlatCatShenanigans>().ChangeCaption();

        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(Monolith)), CanEditMultipleObjects]
    public class Monolith_Editor : Editor
    {
        private Monolith linked;

        private void OnEnable()
        {
            linked = (Monolith)target;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button(new GUIContent("Pepper Deck", "Opens the control center.")))
                PepperDeck.Open(linked);

            DrawDefaultInspector();

        }
    }


    [CustomEditor(typeof(DefaultAsset)), CanEditMultipleObjects]
    public class FolderHint : Editor
    {
        private DefaultAsset linked;

        private void OnEnable()
        {
            linked = (DefaultAsset)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            switch (linked.name)
            {
                case "ScreenSlides":
                    EditorGUILayout.HelpBox($"A folder to place all slides that will be used on the newsroom screen. This folder will be read from to get the slides needed.", MessageType.Info);
                    break;

                case "Animations":
                    EditorGUILayout.HelpBox($"All animations meant for compositing are to go here!", MessageType.Info);
                    break;
            }
        }
    }


    public class PepperDeck : EditorWindow
    {
        private Monolith control;

        private bool showExtras;
        private bool showPepper;
        private bool showBrow;

        private Vector2 scroll;

        private float eyeVertiPos = 0.5f;
        private float eyeHoriPos = 0.5f;

        private float mouthVertiPos = 0.5f;
        private float mouthHoriPos = 0.5f;

        private float leftArmPoseStrength;

        public static void Open(Monolith c)
        {
            var win = GetWindow<PepperDeck>("Pepper Deck");
            win.control = c;
        }

        private void OnGUI()
        {
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox($"Enter play mode to begin.", MessageType.Info);
                return;
            }

            if (control.directorWasOn)
            {
                EditorGUILayout.HelpBox($"Play mode started with Playable Director enabled. Some controls will be locked due director having control. To have full control, disable the PlayableDirector component before entering playmode!", MessageType.Warning);
            }

            scroll = EditorGUILayout.BeginScrollView(scroll);

            showPepper = EditorGUILayout.BeginFoldoutHeaderGroup(showPepper, "Pepper");

            if (showPepper)
            {

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Primary Actions: ", GUILayout.Width(92));
                if (GUILayout.Button("Idle"))
                    control.pepper.PlayPepperAnim("idle");

                if (GUILayout.Button("Talk"))
                    control.pepper.PlayPepperAnim("talk");

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Left Arm Pose Strength: ", GUILayout.Width(142));
                leftArmPoseStrength = EditorGUILayout.Slider(leftArmPoseStrength, 0.0f, 1.0f);
                control.pepper.SetLeftArmPoseStrength(leftArmPoseStrength);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("No Pose"))
                    control.pepper.PlayPepperAnim("noPose");

                if (GUILayout.Button("Think"))
                    control.pepper.PlayPepperAnim("poseTest");

                if (GUILayout.Button("Slam"))
                    control.pepper.PlayPepperAnim("poseTest2");
                EditorGUILayout.EndHorizontal();

                //-- EYES
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("<<", GUILayout.Width(28)))
                    control.pepper.SetPepperEyes(-1);

                EditorGUILayout.LabelField($"Eyes: {control.pepper.currentSelectedEyes}/{control.pepper.allEyes.LastIndex()}", GUILayout.Width(80));

                if (GUILayout.Button(">>", GUILayout.Width(28)))
                    control.pepper.SetPepperEyes(1);

                eyeHoriPos = EditorGUILayout.Slider(eyeHoriPos, 0.0f, 1.0f);
                eyeVertiPos = EditorGUILayout.Slider(eyeVertiPos, 0.0f, 1.0f);
                control.pepper.UpdateEyePos(eyeHoriPos, eyeVertiPos, 1);

                EditorGUILayout.EndHorizontal();

                //-- MOUTH
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("<<", GUILayout.Width(28)))
                    control.pepper.SetPepperMouth(-1);

                EditorGUILayout.LabelField($"Mouth: {control.pepper.currentSelectedMouth}/{control.pepper.allMouths.LastIndex()}", GUILayout.Width(80));

                if (GUILayout.Button(">>", GUILayout.Width(28)))
                    control.pepper.SetPepperMouth(1);

                mouthHoriPos = EditorGUILayout.Slider(mouthHoriPos, 0.0f, 1.0f);
                mouthVertiPos = EditorGUILayout.Slider(mouthVertiPos, 0.0f, 1.0f);
                control.pepper.UpdateMouthPos(mouthHoriPos, mouthVertiPos, 1);
                EditorGUILayout.EndHorizontal();

            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button(new GUIContent("Start Auto Blink", "This is my tooltip")))
                control.pepper.StartAutoBlink();

            if (GUILayout.Button(new GUIContent("Stop Auto Blink", "This is my tooltip")))
                control.pepper.StopAutoBlink();

            if (GUILayout.Button(new GUIContent("Blink Once", "This is my tooltip")))
                control.pepper.BlinkOnce();

            EditorGUILayout.EndHorizontal();

            showExtras = EditorGUILayout.BeginFoldoutHeaderGroup(showExtras, "Extras");

            if (showExtras)
            {

                EditorGUILayout.BeginHorizontal();

                if (GUILayout.Button("Previous Slide"))
                    control.UpdateScreenTex(-1);

                EditorGUILayout.LabelField($"{control.currentSlide + 1}/{control.screenSlides.Length}", GUILayout.Width(32));

                if (GUILayout.Button("Next Slide"))
                    control.UpdateScreenTex(1);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Play Cam Persp"))
                    control.PlayCameraAnim("toPersp");

                if (GUILayout.Button("Play Cam Ortho"))
                    control.PlayCameraAnim("toOrtho");
                EditorGUILayout.EndHorizontal();


                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Left", GUILayout.MaxWidth(64)))
                    control.PlayCameraAnim("orbitLeft");
                if (GUILayout.Button("Center", GUILayout.MaxWidth(64)))
                    control.PlayCameraAnim("orbitCenter");
                if (GUILayout.Button("Right", GUILayout.MaxWidth(64)))
                    control.PlayCameraAnim("orbitRight");
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();


                if (GUILayout.Button("Test Emote"))
                    control.TestEmote();


                if (GUILayout.Button("Play Confetti"))
                    control.PlayConfettiParticles();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.EndScrollView();
        }
    }
#endif
}