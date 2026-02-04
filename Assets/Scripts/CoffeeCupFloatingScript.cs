using UnityEngine;
using System.Collections.Generic;

public class FloatingPickupManager : MonoBehaviour
{
    [Header("Float Settings")]
    [Tooltip("Which axis should the pickups float along? 'Random' picks a different axis for each pickup.")]
    public Axis floatAxis = Axis.Y;

    [Tooltip("How high the object floats")]
    public float floatAmplitude = 0.5f;

    [Tooltip("How fast the object floats")]
    public float floatSpeed = 1f;

    [Header("Rotation Settings")]
    [Tooltip("Which axis should the pickups rotate around? 'Random' picks a different axis for each pickup.")]
    public RotationAxis rotationAxis = RotationAxis.Y;

    [Tooltip("Enable rotation")]
    public bool enableRotation = true;

    [Tooltip("Speed of rotation")]
    public float rotationSpeed = 50f;

  
    public enum Axis { X, Y, Z, Random }
    public enum RotationAxis { X, Y, Z, Random }

    private class ChildData
    {
        public Transform childTransform;
        public Vector3 startPosition;
        public float randomOffset;
        public Axis specificFloatAxis;    
        public Vector3 rotationVector;     
    }

    private List<ChildData> childrenData = new List<ChildData>();

    void Start()
    {
    
        foreach (Transform child in transform)
        {
            ChildData data = new ChildData();
            data.childTransform = child;
            data.startPosition = child.localPosition;
            data.randomOffset = Random.Range(0f, 2f * Mathf.PI);

          
            if (floatAxis == Axis.Random)
            {
                data.specificFloatAxis = (Axis)Random.Range(0, 3);
            }
            else
            {
                data.specificFloatAxis = floatAxis;
            }

            
            if (rotationAxis == RotationAxis.Random)
            {
                
                int rand = Random.Range(0, 3);
                if (rand == 0) data.rotationVector = Vector3.right;
                else if (rand == 1) data.rotationVector = Vector3.up;
                else data.rotationVector = Vector3.forward;
            }
            else
            {
           
                switch (rotationAxis)
                {
                    case RotationAxis.X: data.rotationVector = Vector3.right; break;
                    case RotationAxis.Y: data.rotationVector = Vector3.up; break;
                    case RotationAxis.Z: data.rotationVector = Vector3.forward; break;
                }
            }

            childrenData.Add(data);
        }
    }

    void Update()
    {
      
        foreach (ChildData data in childrenData)
        {
      
            float offset = Mathf.Sin((Time.time * floatSpeed) + data.randomOffset) * floatAmplitude;
            Vector3 newPos = data.startPosition;

            switch (data.specificFloatAxis)
            {
                case Axis.X: newPos.x += offset; break;
                case Axis.Y: newPos.y += offset; break;
                case Axis.Z: newPos.z += offset; break;
            }

            data.childTransform.localPosition = newPos;

     
            if (enableRotation)
            {
                
                data.childTransform.Rotate(data.rotationVector, rotationSpeed * Time.deltaTime, Space.Self);
            }
        }
    }
}