using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace ArcadeVP
{
    public class ArcadeVehicleController : MonoBehaviour
    {
        public enum GroundCheckType { RayCast, SphereCast };
        public enum MovementMode { Velocity, AngularVelocity };

        public MovementMode movementMode;

        [FormerlySerializedAs("GroundCheck")]
        public GroundCheckType groundCheck;

        public LayerMask drivableSurface;

        [FormerlySerializedAs("MaxSpeed")]
        public float maxSpeed;

        [FormerlySerializedAs("accelaration")]
        public float acceleration;

        public float turn;
        public float gravity = 7f;
        public float downforce = 5f;

        [FormerlySerializedAs("AirControl")]
        [Tooltip("if true : can turn vehicle in air")]
        public bool airControl = false;

        [Tooltip("if true : vehicle will drift instead of brake while holding space")]
        public bool kartLike = false;

        [Tooltip("turn more while drifting (while holding space) only if kart Like is true")]
        public float driftMultiplier = 1.5f;

        [Header("Drift & Skid Settings")]
        [Tooltip("Aracın yanlamasına kayma hızı bu değeri geçince lastik izi ve sesi başlar. Düşürdükçe daha kolay iz çıkarır.")]
        public float driftThreshold = 4.0f;

        public Rigidbody rb;
        public Rigidbody carBody;

        [HideInInspector]
        public RaycastHit hit;

        public AnimationCurve frictionCurve;
        public AnimationCurve turnCurve;
        public PhysicsMaterial frictionMaterial;

        [Header("Visuals")]
        [FormerlySerializedAs("BodyMesh")]
        public Transform bodyMesh;

        [FormerlySerializedAs("FrontWheels")]
        public Transform[] frontWheels = new Transform[2];

        [FormerlySerializedAs("RearWheels")]
        public Transform[] rearWheels = new Transform[2];

        [HideInInspector]
        public Vector3 carVelocity;

        [Range(0, 10)]
        [FormerlySerializedAs("BodyTilt")]
        public float bodyTilt;

        [Header("Audio settings")]
        public AudioSource engineSound;

        [Range(0, 1)]
        public float minPitch;

        [Range(1, 3)]
        [FormerlySerializedAs("MaxPitch")]
        public float maxPitch;

        [FormerlySerializedAs("SkidSound")]
        public AudioSource skidSound;

        [HideInInspector]
        public float skidWidth;

        private float radius, steeringInput, accelerationInput, brakeInput;
        private Vector3 origin;
        private bool isGrounded;

        private void Start()
        {
            radius = rb.GetComponent<SphereCollider>().radius;
            if (movementMode == MovementMode.AngularVelocity)
            {
                Physics.defaultMaxAngularSpeed = 100;
            }
        }

        private void Update()
        {
            Visuals();
            AudioManager();
        }

        public void ProvideInputs(float _steeringInput, float _accelerationInput, float _brakeInput)
        {
            steeringInput = _steeringInput;
            accelerationInput = _accelerationInput;
            brakeInput = _brakeInput;
        }

        public void AudioManager()
        {
            engineSound.pitch = Mathf.Lerp(minPitch, maxPitch, Mathf.Abs(carVelocity.z) / maxSpeed);

            // Artık sadece yana kaymayı değil, F1 frenini ve sert AI dönüşlerini de dinliyor
            if (IsTireSkidding())
            {
                skidSound.mute = false;
            }
            else
            {
                skidSound.mute = true;
            }
        }

        private void FixedUpdate()
        {
            UpdateGroundedState();

            carVelocity = carBody.transform.InverseTransformDirection(carBody.linearVelocity);

            if (Mathf.Abs(carVelocity.x) > 0)
            {
                frictionMaterial.dynamicFriction = frictionCurve.Evaluate(Mathf.Abs(carVelocity.x / 100));
            }

            if (grounded())
            {
                float sign = Mathf.Sign(carVelocity.z);
                float TurnMultiplyer = turnCurve.Evaluate(carVelocity.magnitude / maxSpeed);
                if (kartLike && brakeInput > 0.1f)
                {
                    TurnMultiplyer *= driftMultiplier;
                }

                if (accelerationInput > 0.1f || carVelocity.z > 1)
                {
                    carBody.AddTorque(Vector3.up * steeringInput * sign * turn * 100 * TurnMultiplyer);
                }
                else if (accelerationInput < -0.1f || carVelocity.z < -1)
                {
                    carBody.AddTorque(Vector3.up * steeringInput * sign * turn * 100 * TurnMultiplyer);
                }

                if (!kartLike)
                {
                    if (brakeInput > 0.1f)
                    {
                        rb.constraints = RigidbodyConstraints.FreezeRotationX;
                    }
                    else
                    {
                        rb.constraints = RigidbodyConstraints.None;
                    }
                }

                if (movementMode == MovementMode.AngularVelocity)
                {
                    if (Mathf.Abs(accelerationInput) > 0.1f && brakeInput < 0.1f && !kartLike)
                    {
                        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, carBody.transform.right * accelerationInput * maxSpeed / radius, acceleration * Time.deltaTime);
                    }
                    else if (Mathf.Abs(accelerationInput) > 0.1f && kartLike)
                    {
                        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, carBody.transform.right * accelerationInput * maxSpeed / radius, acceleration * Time.deltaTime);
                    }
                }
                else if (movementMode == MovementMode.Velocity)
                {
                    if (Mathf.Abs(accelerationInput) > 0.1f && brakeInput < 0.1f && !kartLike)
                    {
                        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, carBody.transform.forward * accelerationInput * maxSpeed, acceleration / 10 * Time.deltaTime);
                    }
                    else if (Mathf.Abs(accelerationInput) > 0.1f && kartLike)
                    {
                        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, carBody.transform.forward * accelerationInput * maxSpeed, acceleration / 10 * Time.deltaTime);
                    }
                }

                rb.AddForce(-transform.up * downforce * rb.mass);

                carBody.MoveRotation(Quaternion.Slerp(carBody.rotation, Quaternion.FromToRotation(carBody.transform.up, hit.normal) * carBody.transform.rotation, 0.12f));
            }
            else
            {
                if (airControl)
                {
                    float TurnMultiplyer = turnCurve.Evaluate(carVelocity.magnitude / maxSpeed);
                    carBody.AddTorque(Vector3.up * steeringInput * turn * 100 * TurnMultiplyer);
                }

                carBody.MoveRotation(Quaternion.Slerp(carBody.rotation, Quaternion.FromToRotation(carBody.transform.up, Vector3.up) * carBody.transform.rotation, 0.02f));
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, rb.linearVelocity + Vector3.down * gravity, Time.deltaTime * gravity);
            }
        }

        public void Visuals()
        {
            foreach (Transform FW in frontWheels)
            {
                FW.localRotation = Quaternion.Slerp(FW.localRotation, Quaternion.Euler(FW.localRotation.eulerAngles.x,
                                   30 * steeringInput, FW.localRotation.eulerAngles.z), 0.7f * Time.deltaTime / Time.fixedDeltaTime);
                FW.GetChild(0).localRotation = rb.transform.localRotation;
            }
            rearWheels[0].localRotation = rb.transform.localRotation;
            rearWheels[1].localRotation = rb.transform.localRotation;

            if (carVelocity.z > 1)
            {
                bodyMesh.localRotation = Quaternion.Slerp(bodyMesh.localRotation, Quaternion.Euler(Mathf.Lerp(0, -5, carVelocity.z / maxSpeed),
                                   bodyMesh.localRotation.eulerAngles.y, bodyTilt * steeringInput), 0.4f * Time.deltaTime / Time.fixedDeltaTime);
            }
            else
            {
                bodyMesh.localRotation = Quaternion.Slerp(bodyMesh.localRotation, Quaternion.Euler(0, 0, 0), 0.4f * Time.deltaTime / Time.fixedDeltaTime);
            }

            if (kartLike)
            {
                if (brakeInput > 0.1f)
                {
                    bodyMesh.parent.localRotation = Quaternion.Slerp(bodyMesh.parent.localRotation,
                    Quaternion.Euler(0, 45 * steeringInput * Mathf.Sign(carVelocity.z), 0),
                    0.1f * Time.deltaTime / Time.fixedDeltaTime);
                }
                else
                {
                    bodyMesh.parent.localRotation = Quaternion.Slerp(bodyMesh.parent.localRotation,
                    Quaternion.Euler(0, 0, 0),
                    0.1f * Time.deltaTime / Time.fixedDeltaTime);
                }
            }
        }

        private void UpdateGroundedState()
        {
            origin = rb.position + radius * Vector3.up;
            Vector3 direction = -transform.up;
            float maxdistance = radius + 0.2f;

            if (groundCheck == GroundCheckType.RayCast)
            {
                isGrounded = Physics.Raycast(rb.position, Vector3.down, out hit, maxdistance, drivableSurface);
            }
            else if (groundCheck == GroundCheckType.SphereCast)
            {
                isGrounded = Physics.SphereCast(origin, radius + 0.1f, direction, out hit, maxdistance, drivableSurface);
            }
            else
            {
                isGrounded = false;
            }
        }

        public bool grounded()
        {
            return isGrounded;
        }

        /// <summary>
        /// Aracın lastik izi/sesi çıkarıp çıkarmayacağını hesaplayan merkezi Lego parçası.
        /// </summary>
        public bool IsTireSkidding()
        {
            // Araç havadaysa iz çıkmaz
            if (!grounded()) return false;

            // 1. Klasik Yana Kayma (Senin klavye hareketlerinle tetiklenen durum)
            bool lateralSlip = Mathf.Abs(carVelocity.x) > driftThreshold;

            // 2. F1 Tekerlek Kilitlemesi (Yüksek hızda sert fren yaparken iz bırakma)
            bool heavyBraking = brakeInput > 0.4f && carVelocity.z > 20f;

            // 3. Yüksek Hızda Keskin Dönüş (AI'ın pürüzsüz ama yüksek G kuvvetli dönüşleri)
            bool sharpTurn = Mathf.Abs(steeringInput) > 0.6f && carVelocity.z > 35f;

            // Üçünden biri bile geçerliyse lastik izi çıksın!
            return lateralSlip || heavyBraking || sharpTurn;
        }

        private void OnDrawGizmos()
        {
            if (rb == null || Application.isPlaying) return;

            SphereCollider sphere = rb.GetComponent<SphereCollider>();
            if (sphere == null) return;

            float rad = sphere.radius;
            float width = 0.02f;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(rb.transform.position + ((rad + width) * Vector3.down), new Vector3(2 * rad, 2 * width, 4 * rad));

            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position, box.size);
            }
        }
    }
}