using UnityEngine;
using UnityEngine.Playables;
using System;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEditor;
#endif

// TODO: use serializedObject for editor instead of making fields public
namespace Paperverse.TimelineExtensions
{
    [Serializable]
    public class CharacterPlayableAsset : PlayableAsset
    {
        public CharacterAnimator character;
        public CharacterModifiers characterModifier;

        public Texture2D newEyeTex;
        public Texture2D newMouthTex;

        [Range(0, 1)]
        public float eyeHoriPos = 0.5f;

        [Range(0, 1)]
        public float eyeVertiPos = 0.5f;

        [Range(0, 1)]
        public float mouthHoriPos = 0.5f;

        [Range(0, 1)]
        public float mouthVertiPos = 0.5f;

        [Range(0, 100)]
        public float eyeAnger;

        [Range(0, 100)]
        public float leftEyeHalf;

        [Range(0, 100)]
        public float rightEyeHalf;

        [BitMask(nameof(GetCharacterAccesses))]
        public int characterAccessories;

        [EasyID(nameof(GetCharacterEmotes))]
        public int characterEmotes;

        public string GetCharacterEmotes() => character.GetEmotes();
        public string GetCharacterAccesses() => character.GetCharacterAccesses();


        public void OnBehaviourPlayed()
        {
            switch (characterModifier)
            {
                case CharacterModifiers.ChangeEyes:
                    character.UpdateEyes(newEyeTex);
                    break;

                case CharacterModifiers.ChangeMouth:
                    character.UpdateMouth(newMouthTex);
                    break;

                case CharacterModifiers.SetAccessory:
                    for (int i = 0; i < character.numberOfAccessories; i++)
                    {
                        bool active = ((1 << i) & characterAccessories) != 0;
                        character.UpdateAccessory(i, active);
                    }
                    break;

                case CharacterModifiers.Emote:
                    character.emotes[characterEmotes].Play();
                    break;
            }
        }

        public void OnFrameProcessed(float weight)
        {
            switch (characterModifier)
            {
                case CharacterModifiers.MoveEyes:
                    character.UpdateEyePos(eyeHoriPos, eyeVertiPos, weight);
                    break;

                case CharacterModifiers.MoveMouth:
                    character.UpdateMouthPos(mouthHoriPos, mouthVertiPos, weight);
                    break;

                case CharacterModifiers.EyeShape:
                    character.UpdateEyeShape(CharacterAnimator.eyeAngerIndex, eyeAnger, weight);
                    character.UpdateEyeShape(CharacterAnimator.leftHalfIndex, leftEyeHalf, weight);
                    character.UpdateEyeShape(CharacterAnimator.rightHalfIndex, rightEyeHalf, weight);
                    break;
            }
        }

        // Factory method that generates a playable based on this asset
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var playable = ScriptPlayable<CharacterPlayable>.Create(graph);
            CharacterPlayable b = playable.GetBehaviour();
            b.characterPlayable = this;

            return playable;
        }


        public void TestThings()
        {
            var dir = FindObjectOfType<PlayableDirector>();

            var asset = dir.playableAsset as TimelineAsset;

            foreach (var item in asset.GetOutputTracks())
            {
                Debug.Log(item.parent);
            }
        }

    }


#if UNITY_EDITOR
    [CustomEditor(typeof(CharacterPlayableAsset)), CanEditMultipleObjects]
    public class CharacterPlayableAsset_Editor : Editor
    {
        private CharacterPlayableAsset linked;
        private SerializedObject obj;
        private CharacterAnimator chara;
        private SerializedObject serializedPepper;

        private void OnEnable()
        {
            linked = (CharacterPlayableAsset)target;
            obj = new SerializedObject(linked);
            chara = linked.character;
            serializedPepper = new SerializedObject(chara);
        }

        public bool CenteredButton(string label, string tooltip = "")
        {
            var rect = EditorGUILayout.GetControlRect();
            var smallerRect = new Rect(EditorGUIUtility.currentViewWidth / 4.0f, rect.y, rect.width / 2.0f, rect.height);

            return GUI.Button(smallerRect, new GUIContent(label, tooltip));
        }

        public override void OnInspectorGUI()
        {
            linked.character = (CharacterAnimator)EditorGUILayout.ObjectField(nameof(linked.character), linked.character, typeof(CharacterAnimator), true);
            EditorGUILayout.PropertyField(obj.FindProperty(nameof(linked.characterModifier)));

            if (!Monolith.Instance.ValidatePlayableType(linked))
            {
                EditorGUILayout.HelpBox($"This clip must be of type {Monolith.Instance.GetTrackType(linked)} !", MessageType.Error);
                obj.ApplyModifiedProperties();
                return;
            }

            DisplayCurrentModifierProperties();

            if (CenteredButton("Test"))
                linked.TestThings();

            if (CenteredButton("Auto Name Clip", "Auto names this track clip to roughly what the clip does."))
                AutoNameClip();

            obj.ApplyModifiedProperties();
        }

        private void DisplayCurrentModifierProperties()
        {
            switch (linked.characterModifier)
            {
                case CharacterModifiers.ChangeEyes:
                    //-- Using object field here instead of property field for the larger texture preiview.
                    linked.newEyeTex = (Texture2D)EditorGUILayout.ObjectField(nameof(linked.newEyeTex), linked.newEyeTex, typeof(Texture2D), false);
                    break;

                case CharacterModifiers.ChangeMouth:
                    //-- Cause if you drag in a tex it fills in the first texture field wich is super neat, but since
                    //-- everything is being put in the same class it auto goes to eye tex, so we just change it here.
                    if (linked.newEyeTex != null)
                    {
                        linked.newMouthTex = linked.newEyeTex;
                        linked.newEyeTex = null;
                    }
                    linked.newMouthTex = (Texture2D)EditorGUILayout.ObjectField(nameof(linked.newMouthTex), linked.newMouthTex, typeof(Texture2D), false);
                    break;

                case CharacterModifiers.MoveEyes:
                    EditorGUILayout.HelpBox($"Use 'Ease in Duration' to determine how fast the movement occurs. Ease out to return eyes to center.", MessageType.Info);
                    EditorGUILayout.PropertyField(obj.FindProperty(nameof(linked.eyeHoriPos)));
                    EditorGUILayout.PropertyField(obj.FindProperty(nameof(linked.eyeVertiPos)));
                    if (CenteredButton("Reset"))
                    {
                        Undo.RegisterFullObjectHierarchyUndo(linked, "reset");
                        obj.FindProperty(nameof(linked.eyeHoriPos)).floatValue = .5f;
                        obj.FindProperty(nameof(linked.eyeVertiPos)).floatValue = .5f;
                    }
                    break;


                case CharacterModifiers.MoveMouth:
                    EditorGUILayout.HelpBox($"Use 'Ease in Duration' to determine how fast the movement occurs. Ease out to return mouth to center.", MessageType.Info);
                    EditorGUILayout.PropertyField(obj.FindProperty(nameof(linked.mouthHoriPos)));
                    EditorGUILayout.PropertyField(obj.FindProperty(nameof(linked.mouthVertiPos)));
                    if (CenteredButton("Reset"))
                    {
                        Undo.RegisterFullObjectHierarchyUndo(linked, "reset");
                        linked.mouthHoriPos = .5f;
                        linked.mouthHoriPos = .5f;
                        EditorUtility.SetDirty(linked);
                    }
                    break;

                case CharacterModifiers.SetAccessory:
                    EditorGUILayout.PropertyField(obj.FindProperty(nameof(linked.characterAccessories)));
                    EditorGUILayout.HelpBox($"If you are experiencing draw order issues, select the object and add a sorting group.", MessageType.Info);

                    var accessArray = serializedPepper.FindProperty("accessories");

                    EditorGUILayout.Separator();

                    for (int i = 0; i < chara.numberOfAccessories; i++)
                    {
                        if (((1 << i) & linked.characterAccessories) != 0)
                        {
                            EditorGUILayout.BeginHorizontal();
                            if (GUILayout.Button("Edit", GUILayout.Width(48)))
                                EditorUtility.OpenPropertyEditor(accessArray.GetArrayElementAtIndex(i).objectReferenceValue);
                            EditorGUILayout.ObjectField("", accessArray.GetArrayElementAtIndex(i).objectReferenceValue, typeof(GameObject), true);
                            EditorGUILayout.EndHorizontal();

                        }
                    }
                    //-- Don't apply serializedPepper modified props cause it is for display purposes only.
                    break;

                case CharacterModifiers.Emote:
                    EditorGUILayout.PropertyField(obj.FindProperty(nameof(linked.characterEmotes)));
                    break;

                case CharacterModifiers.EyeShape:
                    EditorGUILayout.HelpBox($"Use 'ease in' to adjust the speed of the shape shift.", MessageType.Info);
                    EditorGUILayout.PropertyField(obj.FindProperty(nameof(linked.eyeAnger)));
                    EditorGUILayout.PropertyField(obj.FindProperty(nameof(linked.leftEyeHalf)));
                    EditorGUILayout.PropertyField(obj.FindProperty(nameof(linked.rightEyeHalf)));
                    break;
            }
        }

        private void AutoNameClip()
        {
            var clip = Monolith.Instance.GetClipFromPlayable(typeof(CharacterTrack), linked);

            switch (linked.characterModifier)
            {
                case CharacterModifiers.ChangeEyes:
                case CharacterModifiers.ChangeMouth:
                    {

                        if ((linked.characterModifier == CharacterModifiers.ChangeEyes && linked.newEyeTex == null) || (linked.characterModifier == CharacterModifiers.ChangeMouth && linked.newMouthTex == null))
                        {
                            Debug.LogError("Choose a tex first!!!");
                            return;
                        }

                        string actualTexToUse = linked.characterModifier == CharacterModifiers.ChangeEyes ? linked.newEyeTex.name : linked.newMouthTex.name;
                        clip.displayName = actualTexToUse;
                    }
                    break;

                case CharacterModifiers.MoveEyes:
                case CharacterModifiers.MoveMouth:
                    {
                        float xTotal = Mathf.Abs(linked.eyeHoriPos - .5f);
                        float yTotal = Mathf.Abs(linked.eyeVertiPos - .5f);
                        string strengthTerm = xTotal + yTotal < .25f ? "Nudge " : "Move ";

                        if (linked.eyeHoriPos > .5f)
                            strengthTerm += " Right";
                        else if (linked.eyeHoriPos < .5f)
                            strengthTerm += " Left";

                        if (linked.eyeVertiPos > .5f)
                            strengthTerm += " Up";
                        else if (linked.eyeVertiPos < .5f)
                            strengthTerm += " Down";

                        if (Mathf.Approximately(linked.eyeHoriPos, linked.eyeVertiPos))
                            strengthTerm = "Default";

                        clip.displayName = strengthTerm;

                    }
                    break;

                case CharacterModifiers.SetAccessory:
                    {
                        var serializedPepper = new SerializedObject(chara);
                        var accessArray = serializedPepper.FindProperty("accessories");
                        string accesses = string.Empty;
                        for (int i = 0; i < chara.numberOfAccessories; i++)
                        {
                            if (((1 << i) & linked.characterAccessories) != 0)
                                accesses += ScrapUtils.GetFieldValue(accessArray.GetArrayElementAtIndex(i).objectReferenceValue.name, '_');
                        }
                        clip.displayName = accesses;
                        break;
                    }

                default:
                    throw new NotImplementedException("Scrappie must have been too busy to do this :( ");
            }
        }
    }
#endif
}