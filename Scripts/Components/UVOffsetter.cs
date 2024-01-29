using UnityEngine;
namespace Paperverse
{
    public class UVOffsetter : MonoBehaviour
    {
        private Renderer rend;
        private MaterialPropertyBlock prop;
        private int mainTexOffset;

        private void Awake()
        {
            prop = new MaterialPropertyBlock();
            rend = GetComponent<Renderer>();
            rend.GetPropertyBlock(prop);
            mainTexOffset = Shader.PropertyToID("_BaseMap_ST");
        }

        public void UpdateUV(float newPos)
        {
            Vector4 vec = new Vector4(1, 1, newPos, 0);
            prop.SetVector(mainTexOffset, vec);
            rend.SetPropertyBlock(prop);
        }
    }
}