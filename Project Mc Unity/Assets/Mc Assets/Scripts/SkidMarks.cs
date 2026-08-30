using UnityEngine;

namespace ArcadeVP
{
    public class SkidMarks : MonoBehaviour
    {
        //Aracın teker izi
        public ArcadeVehicleController carController;

        private TrailRenderer skidMark;
        private ParticleSystem smoke;

        private void Awake()
        {
            smoke = GetComponent<ParticleSystem>();
            skidMark = GetComponent<TrailRenderer>();

            if (skidMark != null && carController != null)
            {
                skidMark.emitting = false;
                skidMark.startWidth = carController.skidWidth;
            }
        }

        private void OnEnable()
        {
            if (skidMark != null) skidMark.enabled = true;
        }

        private void OnDisable()
        {
            if (skidMark != null) skidMark.enabled = false;
        }

        private void FixedUpdate()
        {
            // OPTİMİZASYON KORUMASI: Araba yedeğe alınırsa veya parçalanırsa kod çökmesin
            if (carController == null || skidMark == null || smoke == null) return;

            if (carController.IsTireSkidding())
            {
                skidMark.emitting = true;
                if (!smoke.isPlaying) smoke.Play();
            }
            else
            {
                skidMark.emitting = false;
                if (smoke.isPlaying) smoke.Stop();
            }
        }
    }
}