using UnityEngine;
namespace Paperverse
{
    public class TextureSetter : MonoBehaviour
    {
        [SerializeField] private Texture2D tex;

        private Renderer rend;
        private MaterialPropertyBlock prop;

        public void OnValidate() => Awake();

        private void Awake()
        {
            prop = new MaterialPropertyBlock();
            rend = GetComponent<Renderer>();
            rend.GetPropertyBlock(prop);

            if (tex != null)
                UpdateTexture(tex);
        }

        public void UpdateTexture(Texture2D t)
        {

            prop.SetTexture("_BaseMap", t);
            rend.SetPropertyBlock(prop);
        }
    }
}