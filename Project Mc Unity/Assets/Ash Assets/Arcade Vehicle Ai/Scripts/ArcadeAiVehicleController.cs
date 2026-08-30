using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ArcadeVP
{
    public class ArcadeAiVehicleController : MonoBehaviour
    {
        public enum groundCheck { rayCast, sphereCaste };
        public enum MovementMode { Velocity, AngularVelocity };
        public MovementMode movementMode;
        public groundCheck GroundCheck;
        public LayerMask drivableSurface;

        public float MaxSpeed, accelaration, turn;
        public Rigidbody rb, carBody;

        [HideInInspector]
        public RaycastHit hit;
        public AnimationCurve frictionCurve;
        public AnimationCurve turnCurve;
        public PhysicsMaterial frictionMaterial;
        [Header("Visuals")]
        public Transform BodyMesh;
        public Transform[] FrontWheels = new Transform[2];
        public Transform[] RearWheels = new Transform[2];
        [HideInInspector]
        public Vector3 carVelocity;

        [Range(0, 10)]
        public float BodyTilt;
        [Header("Audio settings")]
        public AudioSource engineSound;
        [Range(0, 1)]
        public float minPitch;
        [Range(1, 3)]
        public float MaxPitch;
        public AudioSource SkidSound;

        [HideInInspector]
        public float skidWidth;

        private float radius;
        private Vector3 origin;
        private bool isGrounded; // OPTİMİZASYON: Her frame hesaplamak yerine saklıyoruz

        public Transform target;

        [HideInInspector]
        public float TurnAI = 1f;
        [HideInInspector]
        public float SpeedAI = 1f;
        [HideInInspector]
        public float brakeAI = 0f;
        public float brakeAngle = 30f;

        private float desiredTurning;

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

        private void FixedUpdate()
        {
            // OPTİMİZASYON: Ağır zemin kontrolünü Update yerine burada, seyrek yapıyoruz.
            UpdateGroundedState();

            carVelocity = carBody.transform.InverseTransformDirection(carBody.linearVelocity);

            if (Mathf.Abs(carVelocity.x) > 0)
            {
                frictionMaterial.dynamicFriction = frictionCurve.Evaluate(Mathf.Abs(carVelocity.x / 100));
            }

            // --- AI HEDEF HESAPLAMA (Gereksiz değişkenler ve atamalar temizlendi) ---
            if (target != null)
            {
                Vector3 aimedPoint = target.position;
                aimedPoint.y = transform.position.y;
                Vector3 aimedDir = (aimedPoint - transform.position).normalized;
                Vector3 myDir = transform.forward;
                desiredTurning = Mathf.Abs(Vector3.Angle(myDir, Vector3.ProjectOnPlane(aimedDir, transform.up)));

                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                Vector3 dirToMovePosition = (target.position - transform.position).normalized;
                float dot = Vector3.Dot(transform.forward, dirToMovePosition);
                float angleToMove = Vector3.Angle(transform.forward, dirToMovePosition);

                brakeAI = (angleToMove > brakeAngle && carVelocity.z > 15) ? 1f : 0f;

                if (distanceToTarget > 1f)
                {
                    if (dot > 0)
                    {
                        SpeedAI = 1f;
                        brakeAI = (distanceToTarget < 5f) ? 1f : 0f;
                    }
                    else
                    {
                        if (distanceToTarget > 5f)
                        {
                            SpeedAI = 1f;
                        }
                        else
                        {
                            brakeAI = -1f;
                        }
                    }

                    float angleToDir = Vector3.SignedAngle(transform.forward, dirToMovePosition, Vector3.up);
                    TurnAI = (angleToDir > 0 ? 1f : -1f) * turnCurve.Evaluate(desiredTurning / 90);
                }
                else
                {
                    brakeAI = (carVelocity.z > 1f) ? -1f : 0f;
                    TurnAI = 0f;
                }
            }

            // --- FİZİK HAREKETİ ---
            if (isGrounded)
            {
                float sign = Mathf.Sign(carVelocity.z);
                float TurnMultiplyer = turnCurve.Evaluate(carVelocity.magnitude / MaxSpeed);

                if (Mathf.Abs(SpeedAI) > 0.1f || Mathf.Abs(carVelocity.z) > 1)
                {
                    carBody.AddTorque(Vector3.up * TurnAI * sign * turn * 100 * TurnMultiplyer);
                }

                rb.constraints = (brakeAI > 0.1f) ? RigidbodyConstraints.FreezeRotationX : RigidbodyConstraints.None;

                if (movementMode == MovementMode.AngularVelocity)
                {
                    if (Mathf.Abs(SpeedAI) > 0.1f)
                    {
                        rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, carBody.transform.right * SpeedAI * MaxSpeed / radius, accelaration * Time.deltaTime);
                    }
                }
                else if (movementMode == MovementMode.Velocity)
                {
                    if (Mathf.Abs(SpeedAI) > 0.1f && brakeAI < 0.1f)
                    {
                        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, carBody.transform.forward * SpeedAI * MaxSpeed, accelaration / 10 * Time.deltaTime);
                    }
                }

                carBody.MoveRotation(Quaternion.Slerp(carBody.rotation, Quaternion.FromToRotation(carBody.transform.up, hit.normal) * carBody.transform.rotation, 0.12f));
            }
            else
            {
                carBody.MoveRotation(Quaternion.Slerp(carBody.rotation, Quaternion.FromToRotation(carBody.transform.up, Vector3.up) * carBody.transform.rotation, 0.02f));
            }
        }

        public void AudioManager()
        {
            if (engineSound != null)
                engineSound.pitch = Mathf.Lerp(minPitch, MaxPitch, Mathf.Abs(carVelocity.z) / MaxSpeed);

            if (SkidSound != null)
                SkidSound.mute = !(Mathf.Abs(carVelocity.x) > 10 && isGrounded);
        }

        public void Visuals()
        {
            foreach (Transform FW in FrontWheels)
            {
                if (FW != null)
                {
                    FW.localRotation = Quaternion.Slerp(FW.localRotation, Quaternion.Euler(FW.localRotation.eulerAngles.x,
                                       30 * TurnAI, FW.localRotation.eulerAngles.z), 0.1f);
                    if (FW.childCount > 0)
                        FW.GetChild(0).localRotation = rb.transform.localRotation;
                }
            }
            if (RearWheels[0] != null) RearWheels[0].localRotation = rb.transform.localRotation;
            if (RearWheels[1] != null) RearWheels[1].localRotation = rb.transform.localRotation;

            if (carVelocity.z > 1)
            {
                BodyMesh.localRotation = Quaternion.Slerp(BodyMesh.localRotation, Quaternion.Euler(Mathf.Lerp(0, -5, carVelocity.z / MaxSpeed),
                                   BodyMesh.localRotation.eulerAngles.y, Mathf.Clamp(desiredTurning * TurnAI, -BodyTilt, BodyTilt)), 0.05f);
            }
            else
            {
                BodyMesh.localRotation = Quaternion.Slerp(BodyMesh.localRotation, Quaternion.Euler(0, 0, 0), 0.05f);
            }
        }

        private void UpdateGroundedState()
        {
            origin = rb.position + radius * Vector3.up;
            Vector3 direction = -transform.up;
            float maxdistance = radius + 0.2f;

            if (GroundCheck == groundCheck.rayCast)
                isGrounded = Physics.Raycast(rb.position, Vector3.down, out hit, maxdistance, drivableSurface);
            else if (GroundCheck == groundCheck.sphereCaste)
                isGrounded = Physics.SphereCast(origin, radius + 0.1f, direction, out hit, maxdistance, drivableSurface);
            else
                isGrounded = false;
        }

        // Dışarıdan anlık zemin bilgisi istenirse bunu kullanabiliriz
        public bool grounded() { return isGrounded; }

        private void OnDrawGizmos()
        {
            if (rb == null || Application.isPlaying) return;
            radius = rb.GetComponent<SphereCollider>().radius;
            float width = 0.02f;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(rb.transform.position + ((radius + width) * Vector3.down), new Vector3(2 * radius, 2 * width, 4 * radius));

            BoxCollider box = GetComponent<BoxCollider>();
            if (box != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(transform.position, box.size);
            }
        }
    }
}