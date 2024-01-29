using System.Collections;
using UnityEngine;

namespace Paperverse
{
    public class Pepper : CharacterAnimator
    {
        public FakeParent mug;

        public Slammable[] slammables;
        public AnimationCurve slamCurve;
        public float slamTime;
        public ParticleSystem slamParticles;
        private bool toggle;

        //-- ANIM EVENTS Invoked implicitly
        public void AE_ParentProp()
        {
            if (!Application.isPlaying)
                return;

            toggle = !toggle;

            if (toggle)
                mug.Parent();
            else
                mug.Unparent();
        }

        public void AE_Sip() => mug.sip = true;
        public void AE_UnSip() => mug.sip = false;


        [ContextMenu("Set Slammabes Orig")]
        private void SetSlam()
        {
            foreach (var item in slammables)
                item.orig = item.obj.position;
        }

        public void AE_DeskSlam()
        {
            if (!Application.isPlaying)
                return;

            FindObjectOfType<Monolith>().PlayCameraAnim("camShake1");
            slamParticles.Play();
            StartCoroutine(SlamAnims());
        }

        private IEnumerator SlamAnims()
        {
            float t = 0;

            while (t < 1)
            {
                foreach (var item in slammables)
                    item.obj.localPosition = new Vector3(item.obj.localPosition.x, item.orig.y + (slamCurve.Evaluate(t) * item.height), item.obj.localPosition.z);

                t += Time.deltaTime / slamTime;
                yield return null;
            }

            foreach (var item in slammables)
                item.obj.position = item.orig;
        }
    }
}