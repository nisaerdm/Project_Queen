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
        private float speed;

        [Header("AI Şerit Sistemi (Lane Offset)")]
        [SerializeField] private float maxLaneOffset = 2.5f;
        public float currentLaneOffset = 0f;

        // Referansları önbelleğe aldık ki GetComponent yormasın
        private ArcadeVehicleController playerCar;
        private ArcadeAiVehicleController aiCar;

        private void Start()
        {
            // OPTİMİZASYON: Gereksiz GameObejct oluşumunu engelledik
            if (target == null)
            {
                GameObject targetObj = new GameObject(name + " Waypoint Target");
                target = targetObj.transform;
            }

            if (circuit == null)
            {
                // OPTİMİZASYON: Eski hantal FindObjectOfType yerine Unity 6'nın hızlı aramasını kullandık
                circuit = UnityEngine.Object.FindFirstObjectByType<WaypointCircuit>();
            }

            playerCar = GetComponent<ArcadeVehicleController>();
            aiCar = GetComponent<ArcadeAiVehicleController>();

            if (!gameObject.CompareTag("Player"))
            {
                currentLaneOffset = UnityEngine.Random.Range(-maxLaneOffset, maxLaneOffset);
            }

            Reset();
        }

        public void Reset()
        {
            progressDistance = 0;
            progressNum = 0;
            if (progressStyle == ProgressStyle.PointToPoint && circuit != null && circuit.Waypoints.Length > 0)
            {
                target.position = circuit.Waypoints[progressNum].position;
                target.rotation = circuit.Waypoints[progressNum].rotation;
            }
        }

        private void Update()
        {
            if (circuit == null) return;

            // OPTİMİZASYON: Hızı referans üzerinden çekiyoruz (GetComponent çağrısı silindi)
            if (playerCar != null) speed = playerCar.carVelocity.z;
            else if (aiCar != null) speed = aiCar.carVelocity.z;
            else speed = 0f;

            if (progressStyle == ProgressStyle.SmoothAlongRoute)
            {
                WaypointCircuit.RoutePoint aimPoint = circuit.GetRoutePoint(progressDistance + lookAheadForTargetOffset + lookAheadForTargetFactor * speed);
                Vector3 rightVector = Vector3.Cross(Vector3.up, aimPoint.direction).normalized;

                target.position = aimPoint.position + (rightVector * currentLaneOffset);
                target.rotation = Quaternion.LookRotation(circuit.GetRoutePoint(progressDistance + lookAheadForSpeedOffset + lookAheadForSpeedFactor * speed).direction);

                progressPoint = circuit.GetRoutePoint(progressDistance);
                Vector3 progressDelta = progressPoint.position - transform.position;

                if (Vector3.Dot(progressDelta, progressPoint.direction) < 0)
                {
                    progressDistance += progressDelta.magnitude * 0.5f;
                }
            }
            else
            {
                Vector3 targetDelta = target.position - transform.position;
                if (targetDelta.magnitude < pointToPointThreshold)
                {
                    progressNum = (progressNum + 1) % circuit.Waypoints.Length;
                }

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
            }
        }

        private void OnDrawGizmos()
        {
            // Editör performansı için Gizmos kapatıldı (İstersen içini açabilirsin)
        }
    }
}