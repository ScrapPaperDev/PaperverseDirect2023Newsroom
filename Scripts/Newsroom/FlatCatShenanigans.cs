using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Paperverse
{
    public class FlatCatShenanigans : CharacterAnimator
    {
        private int currentCaption;
        private int slams;
        public float slideTimes;
        public float peppSpeed;
        public Animator camAnim;
        public Animator deskAnim;
        public Pepper pepp;
        public Transform peppSpot;
        public Transform peppSpot2;
        private Vector3 pepperStart;
        public Image capImg;
        public Sprite[] captions;
        public Ruckus[] rbsLeft;
        public Ruckus[] rbsRight;
        public Texture2D[] flats;


        private void Start() => pepperStart = pepp.transform.position;

        public void PepperBit(int i)
        {
            if (!Application.isPlaying)
                return;

            if (i == 0)
            {
                pepp.transform.position = pepperStart;
                StartCoroutine(MovePepper(peppSpot));
            }


            if (i == 1)
            {
                pepp.transform.position = peppSpot.position;
                StartCoroutine(MovePepper(peppSpot2));
            }

        }

        private IEnumerator MovePepper(Transform point)
        {
            while (Vector3.Distance(pepp.transform.position, point.position) > .01f)
            {
                pepp.transform.position = Vector3.MoveTowards(pepp.transform.position, point.position, Time.deltaTime * peppSpeed);
                yield return null;
            }
        }

        public void HitLeft()
        {
            camAnim.SetLayerWeight(1, .1f);
            camAnim.SetTrigger("camShake1");
            foreach (var item in rbsLeft)
            {
                item.torque = Vector3.zero;
                item.force /= 6f;
                if (!item.convex)
                    item.Ruck();
            }

        }

        public void HitRight()
        {
            camAnim.SetLayerWeight(1, 1f);
            camAnim.SetTrigger("camShake1");
            foreach (var item in rbsRight)
                item.Ruck();
        }

        public void ScatCat()
        {
            camAnim.SetTrigger("camShake1");
            deskAnim.SetTrigger("ruckus");
        }

        public void FlatCatEvent(int i)
        {
            if (!Application.isPlaying)
                return;

            switch (i)
            {
                case 0:
                    if (slams == 0)
                    {
                        HitLeft();
                        slams++;
                    }
                    else if (slams == 1)
                        HitRight();

                    break;
            }
        }

        [ContextMenu("GetSlammables")]
        private void Slammms()
        {
            var s = GameObject.Find("Slammables").GetComponentsInChildren<Rigidbody>();
            rbsLeft = new Ruckus[s.Length];
            rbsRight = new Ruckus[s.Length];
            for (int i = 0; i < s.Length; i++)
            {
                rbsLeft[i] = new Ruckus();
                rbsLeft[i].rb = s[i];
                rbsLeft[i].force = new Vector3(0, UnityEngine.Random.Range(80, 300), 0);
                rbsLeft[i].torque = UnityEngine.Random.insideUnitSphere * 100;

                rbsRight[i] = new Ruckus();
                rbsRight[i].rb = s[i];
                rbsRight[i].force = new Vector3(0, UnityEngine.Random.Range(80, 300), 0);
                rbsRight[i].torque = UnityEngine.Random.insideUnitSphere * 100;
            }
        }


        public void StartSlides()
        {
            StartCoroutine(DoSlides());
        }

        private IEnumerator DoSlides()
        {
            Monolith.Instance.StopScreenSlide();
            foreach (var item in flats)
            {
                Monolith.Instance.UpdateScreenTex(item);
                yield return new WaitForSeconds(slideTimes);
            }
        }


        public void ChangeCaption()
        {
            // StartCoroutine(StartCaptionChange());
        }

        private IEnumerator StartCaptionChange()
        {
            float t = 0;

            float fadeSpeed = 4f;



            if (currentCaption == 6)
            {
                capImg.sprite = captions[currentCaption];
                capImg.color = Color.white;
                capImg.rectTransform.localPosition = new Vector3(0, -0, 0);
                capImg.rectTransform.sizeDelta = new Vector2(1920, 1080);

                yield break;
            }


            if (currentCaption > 0)
            {
                while (t < 1)
                {
                    capImg.color = Color.Lerp(Color.white, Color.clear, t);
                    t += Time.deltaTime * fadeSpeed;
                    yield return null;
                }
            }
            capImg.color = Color.clear;
            if (currentCaption == 5)
            {
                capImg.rectTransform.localPosition = new Vector3(0, -51, 0);
                capImg.rectTransform.sizeDelta = new Vector2(1855.22f, 885.659f);
            }
            capImg.sprite = captions[currentCaption];
            t = 0;
            while (t < 1)
            {
                capImg.color = Color.Lerp(Color.clear, Color.white, t);
                t += Time.deltaTime * fadeSpeed;
                yield return null;
            }
            capImg.color = Color.white;
            currentCaption++;

            if (currentCaption == 6)
            {
                yield return new WaitForSeconds(2.8f);
                t = 0;
                while (t < 1)
                {
                    capImg.color = Color.Lerp(Color.white, Color.clear, t);
                    t += Time.deltaTime * fadeSpeed;
                    yield return null;
                }
                capImg.color = Color.clear;
            }
        }
    }

    [Serializable]
    public class Ruckus
    {
        public Rigidbody rb;
        public Vector3 force;
        public Vector3 torque;
        public bool convex;

        public void Ruck()
        {
            rb.isKinematic = false;
            rb.AddForce(force);
            rb.AddTorque(torque);
            if (convex)
                rb.GetComponent<MeshCollider>().convex = true;
        }
    }
}