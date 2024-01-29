using UnityEngine;
using UnityEngine.Playables;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Paperverse.TimelineExtensions
{
    public class NewsRoomEvent : PlayableAsset
    {

        public NewsRoomEvents theEvent;

        public int amount;

        public Texture2D tex;

        public ParticleSystem particleSystem;


        // Factory method that generates a playable based on this asset
        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var playable = ScriptPlayable<NewsRoomBehaviour>.Create(graph);
            var b = playable.GetBehaviour();
            b.theEvent = this;
            return playable;
        }
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(NewsRoomEvent)), CanEditMultipleObjects]
    public class NewsRoomEvent_Editor : Editor
    {
        private NewsRoomEvent linked;

        private SerializedObject obj;

        private void OnEnable()
        {
            linked = (NewsRoomEvent)target;
            obj = new SerializedObject(linked);
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.PropertyField(obj.FindProperty("theEvent"));


            if (linked.theEvent == 0)
                EditorGUILayout.HelpBox($"Select an event! You can select multiple types of events to fire different ones at the same time!", MessageType.Info);

            if (linked.theEvent.HasFlag(NewsRoomEvents.NextSlide))
                EditorGUILayout.PropertyField(obj.FindProperty(nameof(linked.amount)));

            if (linked.theEvent.HasFlag(NewsRoomEvents.SetScreenImage))
                EditorGUILayout.PropertyField(obj.FindProperty(nameof(linked.tex)));

            if (linked.theEvent.HasFlag(NewsRoomEvents.PlayParticles))
                EditorGUILayout.PropertyField(obj.FindProperty(nameof(linked.particleSystem)));

            obj.ApplyModifiedProperties();
        }
    }
#endif

    [Flags]
    public enum NewsRoomEvents
    {
        None = 0b0,
        NextSlide = 0b1,
        PreviousSlide = 0b10,
        SetScreenImage = 0b100,
        PlayParticles = 0b1000,
        FlatCat_PepperBit = 0b1_0000,
        FlatCat_PepperBit2 = 0b10_0000,
        FlatCatStartSlides = 0b100_0000,
        ChangeCaption = 0b1000_0000
    }
}