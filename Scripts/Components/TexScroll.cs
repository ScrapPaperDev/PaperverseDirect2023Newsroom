using System.Collections;
using UnityEngine;

public class TexScroll : MonoBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private Vector4 scale;

    private Renderer rend;
    private MaterialPropertyBlock prop;
    private int mainTexOffset;
    private float t;

    private void Awake()
    {
        prop = new MaterialPropertyBlock();
        rend = GetComponent<Renderer>();
        rend.GetPropertyBlock(prop);
        mainTexOffset = Shader.PropertyToID("_BaseMap_ST");
    }

    private IEnumerator Start()
    {
        while (true)
        {
            t += Time.deltaTime * speed;
            UpdateUV(t);
            yield return null;
        }
    }

    public void UpdateUV(float newPos)
    {
        Vector4 vec = new Vector4(scale.x, scale.y, newPos, scale.w);
        prop.SetVector(mainTexOffset, vec);
        rend.SetPropertyBlock(prop);
    }

    public void Release()
    {
        StopAllCoroutines();
        prop.SetVector(mainTexOffset, scale);
        rend.SetPropertyBlock(prop);
        prop.Clear();
        rend.SetPropertyBlock(prop);
        Destroy(this);
    }
}
