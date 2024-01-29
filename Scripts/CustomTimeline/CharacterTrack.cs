using UnityEngine.Timeline;
namespace Paperverse.TimelineExtensions
{
    [TrackClipType(typeof(CharacterPlayableAsset))]
    public class CharacterTrack : TrackAsset
    {
        [Hidden] public CharacterModifiers trackType;
    }
}