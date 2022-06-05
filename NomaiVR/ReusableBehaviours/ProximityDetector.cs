using System;
using UnityEngine;

namespace NomaiVR.ReusableBehaviours
{
    internal class ProximityDetector : MonoBehaviour
    {
        public event Action<Transform> OnEnter;
        public event Action<Transform> OnExit;
        public float MinDistance { get; set; } = 0.15f;
        public float ExitThreshold { get; set; } = 0.01f;
        public Vector3 LocalOffset { get; set; }

        public Transform Other 
        { 
            get => trackedObjects != null ? GetTrackedObject() : null;
            set
            {
                if (trackedObjects != null)
                {
                    trackedObjects[0] = value;
                    isInside[0] = false;
                    distances[0] = float.PositiveInfinity;
                }
                else
                {
                    trackedObjects = new[] { value };
                    isInside = new bool[1];
                    distances = new float[] { float.PositiveInfinity };
                }
            }
        }

        private float[] distances;
        private bool[] isInside;
        private Transform[] trackedObjects;


        public void SetTrackedObjects(params Transform[] others)
        {
            if (others == null || others.Length <= 0) return;
            trackedObjects = others;
            isInside = new bool[others.Length];
            distances = new float[others.Length];
        }

        public bool IsInside(int index = 0)
        {
            return index < isInside.Length && isInside[index];
        }

        public Transform GetTrackedObject(int index = 0)
        {
            return index >= trackedObjects.Length ? null : trackedObjects[index];
        }
        public float GetTrackedObjectDistance(int index = 0)
        {
            return index >= trackedObjects.Length ? float.PositiveInfinity : distances[index];
        }

        internal void Update()
        {
            for(var i = 0; i < trackedObjects.Length; i++)
            {
                var other = trackedObjects[i];

                if (!other.gameObject.activeSelf)
                    continue;

                var offset = transform.TransformVector(LocalOffset);
                var distance = Vector3.Distance(transform.position + offset, other.position);
                distances[i] = distance;
                if (!isInside[i] && distance <= MinDistance)
                {
                    OnEnter?.Invoke(other);
                    isInside[i] = true;
                }

                if (isInside[i] && distance > MinDistance + ExitThreshold)
                {
                    OnExit?.Invoke(other);
                    isInside[i] = false;
                }
            }
        }

        internal void OnDisable()
        {
            for (var i = 0; i < trackedObjects.Length; i++)
                if (isInside[i]) OnExit?.Invoke(trackedObjects[i]);
        }
    }
}
