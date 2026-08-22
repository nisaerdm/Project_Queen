using System;
using UnityEngine;
using ArcadeVP;

namespace ArcadeVP
{
    public class WaypointProgressTracker : MonoBehaviour
    {
        public WaypointCircuit circuit;

        [SerializeField] private float lookAheadForTargetOffset = 5;
        [SerializeField] private float lookAheadForTargetFactor = .1f;
        private float lookAheadForSpeedOffset = 50;
        private float lookAheadForSpeedFactor = .2f;

        [SerializeField] private ProgressStyle progressStyle = ProgressStyle.SmoothAlongRoute;
        private float pointToPointThreshold = 4;

        public enum ProgressStyle
        {
            SmoothAlongRoute,
            PointToPoint,
        }

        public WaypointCircuit.RoutePoint targetPoint { get; private set; }
        public WaypointCircuit.RoutePoint speedPoint { get; private set; }
        public WaypointCircuit.RoutePoint progressPoint { get; private set; }

        public Transform target;

        [HideInInspector]
        public float progressDistance;
        private int progressNum;
        private Vector3 lastPosition;
        private float speed;

        // YENİLİK: Her aracın kendine ait olan yanal sapma (şerit) değeri
        [Header("AI Şerit Sistemi (Lane Offset)")]
        [Tooltip("Araç ana rotadan sağa veya sola ne kadar uzaklaşabilir?")]
        [SerializeField] private float maxLaneOffset = 2.5f;

        [Tooltip("Araç doğduğunda rastgele bir şerit seçer. İstersen inspector'dan elle de girebilirsin.")]
        public float currentLaneOffset = 0f;

        private void Start()
        {
            if (target == null)
            {
                target = new GameObject(name + " Waypoint Target").transform;
            }

            Reset();
            if (circuit == null)
            {
                circuit = FindObjectOfType<WaypointCircuit>();
            }

            // ÇÖZÜM: Araç doğduğu anda kendine kalıcı bir şerit seçer (Sol, Sağ veya Merkez)
            // Eğer aracın ismi Player ise offset 0 kalır, AI ise rastgele atanır
            if (!gameObject.CompareTag("Player"))
            {
                currentLaneOffset = UnityEngine.Random.Range(-maxLaneOffset, maxLaneOffset);
            }
        }


        public void Reset()
        {
            progressDistance = 0;
            progressNum = 0;
            if (progressStyle == ProgressStyle.PointToPoint)
            {
                target.position = circuit.Waypoints[progressNum].position;
                target.rotation = circuit.Waypoints[progressNum].rotation;
            }
        }


        private void Update()
        {
            if (progressStyle == ProgressStyle.SmoothAlongRoute)
            {
                if (Time.deltaTime > 0)
                {
                    speed = GetComponent<ArcadeVehicleController>().carVelocity.z;
                }

                // 1. Orijinal hedef rotayı alıyoruz
                WaypointCircuit.RoutePoint aimPoint = circuit.GetRoutePoint(progressDistance + lookAheadForTargetOffset + lookAheadForTargetFactor * speed);

                // 2. YENİLİK: Aracın hedefini rotanın sağına veya soluna (currentLaneOffset kadar) kaydırıyoruz
                Vector3 rightVector = Vector3.Cross(Vector3.up, aimPoint.direction).normalized;
                target.position = aimPoint.position + (rightVector * currentLaneOffset);

                target.rotation = Quaternion.LookRotation(circuit.GetRoutePoint(progressDistance + lookAheadForSpeedOffset + lookAheadForSpeedFactor * speed).direction);

                progressPoint = circuit.GetRoutePoint(progressDistance);
                Vector3 progressDelta = progressPoint.position - transform.position;
                if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)
                {
                    progressDistance += progressDelta.magnitude * 0.5f;
                }

                lastPosition = transform.position;
            }
            else
            {
                Vector3 targetDelta = target.position - transform.position;
                if (targetDelta.magnitude < pointToPointThreshold)
                {
                    progressNum = (progressNum + 1) % circuit.Waypoints.Length;
                }

                // Point-To-Point modu için de yanal sapma eklendi
                WaypointCircuit.RoutePoint aimPoint = circuit.GetRoutePoint(progressDistance);
                Vector3 rightVector = Vector3.Cross(Vector3.up, aimPoint.direction).normalized;

                target.position = circuit.Waypoints[progressNum].position + (rightVector * currentLaneOffset);
                target.rotation = circuit.Waypoints[progressNum].rotation;

                progressPoint = circuit.GetRoutePoint(progressDistance);
                Vector3 progressDelta = progressPoint.position - transform.position;
                if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)
                {
                    progressDistance += progressDelta.magnitude;
                }
                lastPosition = transform.position;
            }
        }

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, target.position);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(circuit.GetRoutePosition(progressDistance), 0.2f);
                Gizmos.DrawLine(transform.position, circuit.GetRoutePosition(progressDistance));
                Gizmos.DrawLine(target.position, target.position + target.forward);
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(target.position, 1);
            }
        }
    }
}