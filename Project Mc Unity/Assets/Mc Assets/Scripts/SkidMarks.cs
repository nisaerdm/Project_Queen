using UnityEngine;

namespace ArcadeVP
{
    public class SkidMarks : MonoBehaviour
    {
        public ArcadeVehicleController carController;

        private TrailRenderer skidMark;
        private ParticleSystem smoke;

        private void Awake()
        {
            smoke = GetComponent<ParticleSystem>();
            skidMark = GetComponent<TrailRenderer>();

            skidMark.emitting = false;
            skidMark.startWidth = carController.skidWidth;
        }

        private void OnEnable()
        {
            skidMark.enabled = true;
        }

        private void OnDisable()
        {
            skidMark.enabled = false;
        }

        private void FixedUpdate()
        {
            // Bütün hesaplamayı ArcadeVehicleController'a devrettik!
            if (carController != null && carController.IsTireSkidding())
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