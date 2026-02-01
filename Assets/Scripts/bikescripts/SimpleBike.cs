using UnityEngine;
using ProjectileCurveVisualizerSystem;

namespace KikiNgao.SimpleBikeControl
{
    public class SimpleBike : MonoBehaviour
    {
        [Tooltip("Control without biker")]
        public bool noBikerCtrl;

        private InputManager inputManager;
        public Transform bikerHolder;

        public WheelCollider frontWheelCollider;
        public WheelCollider rearWheelCollider;
        public GameObject frontWheel;
        public GameObject rearWheel;

        public Transform handlerBar;
        public Transform cranksetTransform;

        [SerializeField] public float legPower = 10;
        [SerializeField] private float powerUpMax = 2;
        [SerializeField] private float powerUpSpeed = .5f;
        [SerializeField] private float airResistance = 6;
        [SerializeField] private float turningSmooth = .8f;
        [SerializeField] private float restDrag = 2f;
        [SerializeField] private float restAngularDrag = .2f;
        [SerializeField] private float forceRatio = 2f;
        [SerializeField] public float bikeHealth = 100f;

        [SerializeField]
        private AnimationCurve frontWheelRestrictCurve =
            new AnimationCurve(new Keyframe(0f, 35f), new Keyframe(50f, 1f));

        public Transform leftHandTarget, rightHandTarget;
        public Transform leftPendalTarget, rightPendalTarget;
        public Transform leftStandTarget, rightStandTarget;

        private Transform centerOfMass;
        private Rigidbody m_Rigidbody;
        public Rigidbody GetRigidbody() => m_Rigidbody;

        [HideInInspector] public bool falling;
        private float fallingDrag = 1;
        private float fallingAngurlarDrag = 0.01f;

        private float temporaryFrontWheelAngle;
        private float handlerBarYLastAngle;
        private float currentLegPower;
        private float reversePower;
        private EventManager eventManager;

        public bool IsReverse() => inputManager.vertical < 0;
        public bool IsMovingToward => inputManager.vertical > 0;
        public bool IsMoving() => inputManager.vertical != 0; 
        private bool IsRest() => (inputManager.vertical == 0 && inputManager.horizontal == 0) ||
                                 (inputManager.vertical == 0 && inputManager.horizontal != 0);
        private bool IsTurning() => inputManager.horizontal != 0;
        private bool IsSpeedUp() => inputManager.speedUp;

        public float GetBikeSpeedKm() => GetBikeSpeedMs() * 3.6f;
        private float GetBikeSpeedMs() => m_Rigidbody.linearVelocity.magnitude;
        private float GetBikeAngle() => WrapAngle(transform.eulerAngles.z);
        public bool TiltToRight() => WrapAngle(transform.eulerAngles.z) <= 0;

        public bool Freeze { get => m_Rigidbody.isKinematic; set => m_Rigidbody.isKinematic = value; }
        public bool FreezeCrankset { get; set; }
        public bool ReadyToRide()
        {
            if (noBikerCtrl) return true;
            if (bikerHolder.childCount == 0) return false;
            if (bikerHolder.GetChild(0).CompareTag("Player")) return true;
            return false;
        }

        
        public ProjectileCurveVisualizer projectileCurveVisualizer;
        public GameObject projectileGameObject;
        private bool inProjectileMode = false;
        private float launchSpeed = 10.0f;
        private Vector3 launchVelocity;
        private Vector3 updatedProjectileStartPosition;
        private RaycastHit hit;

        void Start()
        {
            inputManager = GameManager.Instance.GetInputManager;
            eventManager = GameManager.Instance.GetEventManager;

            CreateCenterOfMass();
            SettingRigidbody();

            currentLegPower = legPower * 10;
            reversePower = legPower * 3;

            Freeze = true;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null && bikerHolder != null)
            {
                player.transform.SetParent(bikerHolder);
                player.transform.localPosition = Vector3.zero;
                player.transform.localRotation = Quaternion.identity;
            }
        }

        void Update()
        {
            if (!ReadyToRide()) return;

            if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Joystick2Button2))
            {
                inProjectileMode = !inProjectileMode;
                if (!inProjectileMode) projectileCurveVisualizer.HideProjectileCurve();
            }

            if (inProjectileMode)
            {
                //float scroll = Input.GetAxis("Mouse ScrollWheel");
                //float stickY = Input.GetAxis("Joystick2Axis5"); 

                //float combined = scroll + (stickY * 0.02f);

                //launchSpeed = Mathf.Clamp(launchSpeed + combined * 200f * Time.deltaTime, 2f, 50f);

                //launchVelocity = transform.forward + Vector3.up * 0.3f;
                //launchVelocity = launchVelocity.normalized * launchSpeed;

                //projectileCurveVisualizer.VisualizeProjectileCurve(
                  //  transform.position, 1.0f, launchVelocity, 0.25f, 0.1f, true,
                    //out updatedProjectileStartPosition, out hit);

                //if (Input.GetMouseButtonUp(0) || Input.GetButtonUp("Joystick2Fire1"))
                //{
                  //  inProjectileMode = false;
                    //projectileCurveVisualizer.HideProjectileCurve();

                    //Projectile projectile = Instantiate(projectileGameObject).GetComponent<Projectile>();
                    //projectile.transform.position = updatedProjectileStartPosition;
                    //projectile.Throw(launchVelocity);
                //}
            }

        }

        private void CreateCenterOfMass()
        {
            centerOfMass = new GameObject().transform;
            centerOfMass.name = "CenterOfMass";
            Vector3 center = new Vector3();
            Vector3 rearPosition = rearWheelCollider.transform.position;
            center.x = rearPosition.x;
            center.y = 0;
            center.z = rearPosition.z + (frontWheelCollider.transform.position.z - rearPosition.z) / 2;

            centerOfMass.transform.position = center;
            centerOfMass.parent = transform;
        }

        private void SettingRigidbody()
        {
            m_Rigidbody = transform.GetComponent<Rigidbody>();
            m_Rigidbody.centerOfMass = centerOfMass.transform.position;
        }

        float powerUp = 1f;
        private void FixedUpdate()
        {
            if (falling) { Falling(); return; }
            if (!ReadyToRide()) return;

            if (IsRest()) Rest();
            if (IsMoving()) MovingBike();
            if (IsTurning()) TurningBike();

            UpdateLegPower(IsSpeedUp());
            if (!FreezeCrankset) UpdateCranksetRotation();
            UpdateWheelDisplay();
        }

        private void UpdateLegPower(bool speedUp)
        {
            if (speedUp)
            {
                powerUp += powerUpSpeed * Time.deltaTime;
                if (powerUp >= powerUpMax) powerUp = powerUpMax;
                currentLegPower = legPower * 10 * powerUp;
                eventManager?.OnSpeedUp();
                return;
            }
            eventManager?.OnNormalSpeed();
            powerUp = 1f;
            currentLegPower = legPower * 10 * powerUp;
        }

        public void MovingBike()
        {
            Freeze = false;
            m_Rigidbody.linearDamping = GetBikeSpeedMs() / m_Rigidbody.mass * airResistance;
            m_Rigidbody.angularDamping = 5 + GetBikeSpeedMs() / (m_Rigidbody.mass / 10);

            frontWheelCollider.brakeTorque = 0;
            rearWheelCollider.motorTorque = !IsReverse() ? currentLegPower * inputManager.vertical : reversePower * inputManager.vertical;

            UpdateCenterOfMass();
        }

        private void TurningBike()
        {
            temporaryFrontWheelAngle = frontWheelRestrictCurve.Evaluate(GetBikeSpeedKm());
            float nextAngle = temporaryFrontWheelAngle * inputManager.horizontal;
            frontWheelCollider.steerAngle = nextAngle;
            Quaternion handlerBarLocalRotation = Quaternion.Euler(0, nextAngle - handlerBarYLastAngle, 0);
            handlerBar.rotation = Quaternion.Lerp(handlerBar.rotation, handlerBar.rotation * handlerBarLocalRotation, turningSmooth);
            handlerBarYLastAngle = nextAngle;
        }

        private void ResetWheelsCollider()
        {
            frontWheelCollider.steerAngle = 0f;
            frontWheelCollider.motorTorque = 0;
            rearWheelCollider.motorTorque = 0;
            rearWheelCollider.brakeTorque = 0;
            frontWheelCollider.brakeTorque = 0;
        }

        private void Rest()
        {
            m_Rigidbody.linearDamping = restDrag;
            m_Rigidbody.angularDamping = restAngularDrag;
            ResetWheelsCollider();
            UpdateCenterOfMass();
        }

        public void Falling()
        {
            falling = true;
            m_Rigidbody.linearDamping = fallingDrag;
            m_Rigidbody.angularDamping = fallingAngurlarDrag;

            UpdateCenterOfMass();
            UpdateWheelDisplay();
            ResetWheelsCollider();

            float angle = GetBikeAngle();
            if (angle < -75 || angle > 75) { Freeze = true; falling = false; }
        }

        private void UpdateCranksetRotation()
        {
            cranksetTransform.rotation *= Quaternion.Euler(GetBikeSpeedKm() / forceRatio, 0, 0);
            Quaternion ro = Quaternion.Euler(-GetBikeSpeedKm() / forceRatio, 0, 0);
            cranksetTransform.GetChild(0).rotation *= ro;
            cranksetTransform.GetChild(1).rotation *= ro;
        }

        private void UpdateWheelDisplay()
        {
            Vector3 temporaryVector;
            Quaternion temporaryQuaternion;

            rearWheelCollider.GetWorldPose(out temporaryVector, out temporaryQuaternion);
            rearWheel.transform.position = temporaryVector;

            Quaternion rearWheelRot = rearWheel.transform.rotation;
            rearWheel.transform.rotation = IsReverse()
                ? rearWheelRot * Quaternion.Euler(-GetBikeSpeedKm(), 0, 0)
                : rearWheelRot * Quaternion.Euler(GetBikeSpeedKm(), 0, 0);

            frontWheel.transform.localRotation = rearWheel.transform.localRotation;
        }

        private void UpdateCenterOfMass()
        {
            var centerLocal = centerOfMass.localPosition;

            if (!falling)
            {
                if (IsRest())
                {
                    centerLocal.y = 0;
                    centerLocal.x = TiltToRight() ? .01f : -0.1f;
                }
                else centerLocal.y = -0.8f;
            }
            else
            {
                centerLocal.y = 0;
                centerLocal.x = TiltToRight() ? .2f : -0.2f;
            }

            m_Rigidbody.centerOfMass = centerLocal;
        }

        private static float WrapAngle(float angle)
        {
            angle %= 360;
            if (angle > 180) return angle - 360;
            return angle;
        }

        
        private bool isSlowed = false;
        private float originalLegPower;
        private Coroutine slowRoutine;

        public void ApplyTemporarySlowdown(float multiplier, float duration)
        {
            if (isSlowed) return;

            isSlowed = true;
            originalLegPower = legPower;
            legPower *= multiplier;

            if (slowRoutine != null)
                StopCoroutine(slowRoutine);

            slowRoutine = StartCoroutine(RemoveSlowdownAfterDelay(duration));
        }

        private System.Collections.IEnumerator RemoveSlowdownAfterDelay(float duration)
        {
            yield return new WaitForSeconds(duration);
            legPower = originalLegPower;
            isSlowed = false;
        }
    }
}
