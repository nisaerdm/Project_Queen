using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcadeVP
{
    [RequireComponent(typeof(TrailRenderer))]
    [RequireComponent(typeof(ParticleSystem))]
    public class AiSkidMarks : MonoBehaviour
    {
        private TrailRenderer skidMark;
        private ParticleSystem smoke;

        [Header("Bağlantılar")]
        [Tooltip("Aracın üzerindeki ana motor scriptini sürükle")]
        public ArcadeVehicleController carController;

        private void Awake()
        {
            smoke = GetComponent<ParticleSystem>();
            skidMark = GetComponent<TrailRenderer>();
            skidMark.emitting = false;

            if (carController != null)
            {
                skidMark.startWidth = carController.skidWidth;
            }
        }

        private void OnEnable()
        {
            skidMark.enabled = true;
        }

        private void OnDisable()
        {
            skidMark.enabled = false;
        }

        void FixedUpdate()
        {
            if (carController == null) return;

            // --- İZ ÇIKARTMA MANTIĞI ---
            if (carController.grounded())
            {
                // Sabit '10' değeri yerine senin ArcadeVehicleController'da tanımladığın driftThreshold değerini okuyoruz
                if (Mathf.Abs(carController.carVelocity.x) > carController.driftThreshold)
                {
                    skidMark.emitting = true;
                }
                else
                {
                    skidMark.emitting = false;
                }
            }
            else
            {
                skidMark.emitting = false;
            }

            // --- DUMAN MANTIĞI VE PERFORMANS OPTİMİZASYONU ---
            if (skidMark.emitting)
            {
                // Update içinde her karede Play() çağırmak performansı öldürür. 
                // Sadece duman halihazırda çalışmıyorsa başlatıyoruz.
                if (!smoke.isPlaying)
                {
                    smoke.Play();
                }
            }
            else
            {
                if (smoke.isPlaying)
                {
                    smoke.Stop();
                }
            }
        }
    }
}