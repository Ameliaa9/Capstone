using UnityEngine;

public class FlexibleBikeLegs_MeshData : MonoBehaviour
{
    [System.Serializable]
    public class LegDeformSegment
    {
        public Transform meshTarget;
        public Transform anchor;
        public Transform tip;
        public Transform footJoint;
        public bool recalcNormals = true;

        [HideInInspector] public bool ready;
        [HideInInspector] public Mesh deformMesh;
        [HideInInspector] public float[] vertT;

        // These are now stored as Local coordinates relative to the Bike Root
        [HideInInspector] public Vector3[] localVertPerp;
        [HideInInspector] public Vector3 localRestDir;
        [HideInInspector] public float restLen;
    }

    [System.Serializable]
    public class LegMember
    {
        public string sideName;
        public Transform legMesh;
        public Transform hingePoint;
        public Transform footMesh;
        public Transform pedalTarget;
        public bool reverse;
        public LegDeformSegment[] deformSegments = new LegDeformSegment[0];

        [HideInInspector] public Vector3 localStartPos;
        [HideInInspector] public Quaternion localStartRot;
        [HideInInspector] public Vector3 footRelOffset;
        [HideInInspector] public Quaternion footRelRot;
        [HideInInspector] public float startPedalAngle;

        public bool HasDeformation => deformSegments != null && deformSegments.Length > 0;
    }

    public LegMember left;
    public LegMember right;
    public Vector3 rotationAxis = Vector3.right;

    void Start()
    {
        SetupBase(left);
        SetupBase(right);
        SetupAllSegments(left);
        SetupAllSegments(right);
    }

    void SetupBase(LegMember leg)
    {
        if (leg.legMesh == null || leg.hingePoint == null || leg.pedalTarget == null) return;
        leg.localStartPos = transform.InverseTransformPoint(leg.legMesh.position);
        leg.localStartRot = Quaternion.Inverse(transform.rotation) * leg.legMesh.rotation;

        if (leg.footMesh != null)
        {
            leg.footRelOffset = leg.pedalTarget.InverseTransformPoint(leg.footMesh.position);
            leg.footRelRot = Quaternion.Inverse(transform.rotation) * leg.footMesh.rotation;
        }
        leg.startPedalAngle = GetAngle(leg.pedalTarget);
    }

    void SetupAllSegments(LegMember leg)
    {
        for (int i = 0; i < leg.deformSegments.Length; i++)
            SetupSegment(leg.deformSegments[i]);
    }

    void SetupSegment(LegDeformSegment seg)
    {
        seg.ready = false;
        if (seg.meshTarget == null || seg.anchor == null || seg.tip == null || seg.footJoint == null) return;

        MeshFilter mf = seg.meshTarget.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null) return;

        // Create writable copy
        Mesh newMesh = new Mesh();
        CombineInstance[] combine = new CombineInstance[1];
        combine[0].mesh = mf.sharedMesh;
        combine[0].transform = Matrix4x4.identity;
        newMesh.CombineMeshes(combine, true, false);

        seg.deformMesh = newMesh;
        seg.deformMesh.MarkDynamic();
        mf.mesh = seg.deformMesh;

        // Capture Rest Pose relative to the Bike (this script's transform)
        Vector3 worldBoneVec = seg.tip.position - seg.anchor.position;
        seg.restLen = worldBoneVec.magnitude;
        if (seg.restLen < 0.001f) return;

        // Store direction as a LOCAL direction relative to the bike
        seg.localRestDir = transform.InverseTransformDirection(worldBoneVec.normalized);

        Vector3[] verts = seg.deformMesh.vertices;
        seg.vertT = new float[verts.Length];
        seg.localVertPerp = new Vector3[verts.Length];

        for (int i = 0; i < verts.Length; i++)
        {
            Vector3 worldV = seg.meshTarget.TransformPoint(verts[i]);
            Vector3 fromAnchor = worldV - seg.anchor.position;

            // T is a ratio, so it's space-independent
            float t = Vector3.Dot(fromAnchor, worldBoneVec.normalized) / seg.restLen;
            seg.vertT[i] = t;

            // Store perpendicular offset as a LOCAL direction relative to the bike
            Vector3 worldPerp = fromAnchor - (t * seg.restLen * worldBoneVec.normalized);
            seg.localVertPerp[i] = transform.InverseTransformDirection(worldPerp);
        }
        seg.ready = true;
    }

    void LateUpdate()
    {
        UpdateLeg(left);
        UpdateLeg(right);
    }

    void UpdateLeg(LegMember leg)
    {
        if (leg.legMesh == null) return;

        leg.legMesh.position = transform.TransformPoint(leg.localStartPos);
        leg.legMesh.rotation = transform.rotation * leg.localStartRot;

        if (leg.HasDeformation)
        {
            foreach (var seg in leg.deformSegments)
            {
                if (seg == null || !seg.ready) continue;
                seg.tip.position = seg.footJoint.position;
                UpdateSegmentDeformation(seg);
            }
        }
        else
        {
            float angleDiff = Mathf.DeltaAngle(leg.startPedalAngle, GetAngle(leg.pedalTarget));
            leg.legMesh.RotateAround(leg.hingePoint.position, transform.TransformDirection(rotationAxis), leg.reverse ? -angleDiff : angleDiff);
        }

        if (leg.footMesh != null)
        {
            leg.footMesh.position = leg.pedalTarget.TransformPoint(leg.footRelOffset);
            leg.footMesh.rotation = transform.rotation * leg.footRelRot;
        }
    }

    void UpdateSegmentDeformation(LegDeformSegment seg)
    {
        Vector3 currBoneVec = seg.tip.position - seg.anchor.position;
        float currLen = currBoneVec.magnitude;
        if (currLen < 0.001f) return;

        Vector3 currDir = currBoneVec / currLen;

        // Transform our stored local directions back into world space based on current bike rotation
        Vector3 worldRestDir = transform.TransformDirection(seg.localRestDir);
        Quaternion boneRot = Quaternion.FromToRotation(worldRestDir, currDir);

        Vector3[] verts = seg.deformMesh.vertices;

        for (int i = 0; i < verts.Length; i++)
        {
            // 1. Move along the bone axis
            Vector3 worldPos = seg.anchor.position + (seg.vertT[i] * currLen * currDir);

            // 2. Add thickness offset (re-oriented to world space first)
            Vector3 worldPerp = transform.TransformDirection(seg.localVertPerp[i]);
            worldPos += (boneRot * worldPerp);

            // 3. Back to local space of the mesh filter
            verts[i] = seg.meshTarget.InverseTransformPoint(worldPos);
        }

        seg.deformMesh.vertices = verts;
        seg.deformMesh.bounds = new Bounds(Vector3.zero, Vector3.one * 10f);

        if (seg.recalcNormals) seg.deformMesh.RecalculateNormals();
    }

    float GetAngle(Transform t)
    {
        Vector3 localPos = transform.InverseTransformPoint(t.position);
        return Mathf.Atan2(localPos.y, localPos.z) * Mathf.Rad2Deg;
    }
}