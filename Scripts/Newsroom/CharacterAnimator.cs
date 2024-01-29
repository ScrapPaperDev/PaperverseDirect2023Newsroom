using System.Collections;
using UnityEngine;

namespace Paperverse
{
    public class CharacterAnimator : MonoBehaviour
    {

        [SerializeField, Hidden] private Transform eyes;
        [SerializeField, Hidden] private Transform mouth;
        [SerializeField] private TextureSetter eyeTexture;
        [SerializeField] private TextureSetter mouthTexture;
        [SerializeField] private SkinnedMeshRenderer eyeMesh;
        [SerializeField] private SkinnedMeshRenderer mouthMeth;

        public Texture2D[] allEyes;
        public Texture2D[] blinkableEyes;

        public Texture2D[] allMouths;

        public string GetCharacterAccesses() => accessories.ColToCSV(x => ScrapUtils.GetFieldValue(x.name, '_'));
        [SerializeField, BitMask(nameof(GetCharacterAccesses))] private int accessIDs;
        [SerializeField] private GameObject[] accessories;
        public int numberOfAccessories => accessories.Length;

        public string GetEmotes() => emotes.ColToCSV(x => x.transform.parent.name.GetTrailingString('_'));
        [SerializeField, EasyID(nameof(GetEmotes))] private int emoteID;
        public GameObject emoteRoot;
        public ParticleSystem[] emotes;

        [ContextMenu("Get Emote Systems")]
        private void GetEmoteSystems() => emotes = emoteRoot.GetComponentsInChildren<ParticleSystem>();


        [SerializeField] private Animator anim;

        [SerializeField] private float blinkLength;

        [SerializeField] private float minTimeToNextBlink;
        [SerializeField] private float maxTimeToNextBlink;

        [Tooltip("Allow blink timing to overlap")]
        [SerializeField] private bool clipBlinks;

        [SerializeField] private bool blinkOnAwake;

        [SerializeField] private AnimationCurve blinkEaseIn;
        [SerializeField] private AnimationCurve blinkEaseOut;

        private const int blinkShapeIndex = 3;

        public const int eyeAngerIndex = 4;
        public const int rightHalfIndex = 5;
        public const int leftHalfIndex = 6;

        private IEnumerator blinking;
        [Hidden] public int currentSelectedEyes;
        [Hidden] public int currentSelectedMouth;


        private void OnValidate()
        {
            if (eyeTexture != null)
                eyeTexture.OnValidate();
            if (mouthTexture != null)
                mouthTexture.OnValidate();

            if (eyes == null)
                eyes = eyeTexture.transform.parent;

            if (mouth == null)
                mouth = mouthTexture.transform.parent;

        }


        private void Awake()
        {
            if (blinkOnAwake)
                StartAutoBlink();
        }

        public void UpdateAccessory(int i, bool b) => accessories[i].SetActive(b);


        public void UpdateEyes(Texture2D tex)
        {
            eyeTexture.UpdateTexture(tex);

            bool blinkable = false;
            int id = tex.GetInstanceID();
            foreach (var item in blinkableEyes)
            {
                if (id == item.GetInstanceID())
                {
                    blinkable = true;
                    break;
                }
            }

            if (blinkable)
                StartAutoBlink();
            else
                StopAutoBlink();
        }

        public void UpdateMouth(Texture2D tex) => mouthTexture.UpdateTexture(tex);

        public void SetPepperEyes(int i)
        {
            currentSelectedEyes += i;
            currentSelectedEyes %= allEyes.Length;
            if (currentSelectedEyes < 0)
                currentSelectedEyes = allEyes.LastIndex();

            eyeTexture.UpdateTexture(allEyes[currentSelectedEyes]);
        }

        public void SetPepperMouth(int i)
        {
            currentSelectedMouth += i;
            currentSelectedMouth %= allMouths.Length;
            if (currentSelectedMouth < 0)
                currentSelectedMouth = allMouths.LastIndex();

            mouthTexture.UpdateTexture(allMouths[currentSelectedMouth]);
        }


        //-- The up direction wound up being on the z axis cause blender shenanigans!

        public void UpdateEyePos(float xInterp, float yInterp, float t)
        {
            float newXPos = Mathf.Lerp(.03f, -.03f, xInterp);
            float newYPos = Mathf.Lerp(-.03f, .03f, yInterp);

            eyes.transform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(newXPos, 0, newYPos), t);
        }


        public void UpdateMouthPos(float xInterp, float yInterp, float t)
        {
            float newXPos = Mathf.Lerp(.03f, -.03f, xInterp);
            float newYPos = Mathf.Lerp(-.03f, .03f, yInterp);

            mouth.transform.localPosition = Vector3.Lerp(Vector3.zero, new Vector3(newXPos, 0, newYPos), t);
        }

        public void UpdateEyeShape(int key, float t, float i)
        {
            float amount = Mathf.Lerp(0.0f, t, i);
            eyeMesh.SetBlendShapeWeight(key, amount);
        }




        public void PlayPepperAnim(string paramName) => anim.SetTrigger(paramName);

        public void SetLeftArmPoseStrength(float blendStrength)
        {
            const int leftArmLayerIndex = 2;
            anim.SetLayerWeight(leftArmLayerIndex, blendStrength);
        }

        public void SetRightArmPoseStrength(float blendStrength)
        {
            const int rightArmLayerIndex = 3;
            anim.SetLayerWeight(rightArmLayerIndex, blendStrength);
        }


        public void StartAutoBlink()
        {
            StopAutoBlink();

            blinking = AutoBlink();
            StartCoroutine(blinking);
        }


        public void StopAutoBlink()
        {
            if (blinking != null)
                StopCoroutine(blinking);
        }


        public void BlinkOnce() => StartCoroutine(Blink());



        private IEnumerator AutoBlink()
        {
            while (true)
            {
                yield return new WaitForSeconds(Random.Range(minTimeToNextBlink, maxTimeToNextBlink));
                if (clipBlinks)
                    StartCoroutine(Blink());
                else
                    yield return Blink();
            }
        }


        private IEnumerator Blink()
        {
            float t = 0;

            while (t < 1)
            {
                eyeMesh.SetBlendShapeWeight(blinkShapeIndex, blinkEaseIn.Evaluate(t) * 100.00f);
                t += Time.deltaTime / blinkLength;
                yield return null;
            }

            while (t > 0)
            {
                eyeMesh.SetBlendShapeWeight(blinkShapeIndex, blinkEaseOut.Evaluate(t) * 100.00f);
                t -= Time.deltaTime / blinkLength;
                yield return null;
            }

            eyeMesh.SetBlendShapeWeight(blinkShapeIndex, 0);
        }
    }
}