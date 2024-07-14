using Cinemachine;
using MalbersAnimations.Events;
using MalbersAnimations.Scriptables;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MalbersAnimations
{
    [AddComponentMenu("Malbers/Camera/Third Person Follow Target (Cinemachine)")]
    [DefaultExecutionOrder(-500)]
    public class ThirdPersonFollowTarget : MonoBehaviour
    {
        [Tooltip("Cinemachine Brain Camera")]
        public CinemachineBrain Brain;

        [Tooltip("Update mode for the Aim Logic")]
        public UpdateType updateMode = UpdateType.FixedUpdate;

        [Tooltip("The Camera can rotate independent of the Game Time")]
        public BoolReference unscaledTime = new(true);

        /// <summary> List of all the scene Third Person Follow Cameras (using the same brain)! </summary>
        public static HashSet<ThirdPersonFollowTarget> TPFCameras;

        /// <summary>  Active Camera using the same Cinemachine Brain </summary>
        public ThirdPersonFollowTarget ActiveThirdPersonCamera { get; set; }

        private ICinemachineCamera ThisCamera;
        private Cinemachine3rdPersonFollow CM3PFollow;

        [Tooltip("Default Priority of this Cinemachine camera")]
        public int priority = 10;
        [Tooltip("Changes the Camera Side parameter on the Third Person Camera")]
        [Range(0f, 1f)]
        [SerializeField]
        private float cameraSide = 1f;

        [Tooltip("What object to follow")]
        public TransformReference Target;

        public Transform CamPivot;

        [Tooltip("Camera Input Values (Look X:Horizontal, Look Y: Vertical)")]
        public Vector2Reference look = new();

        [Tooltip("Align the Camera with the up vector")]
        public TransformReference upVector;


        [Tooltip("Invert X Axis of the Look Vector")]
        public BoolReference invertX = new();
        [Tooltip("Invert Y Axis of the Look Vector")]
        public BoolReference invertY = new();

        [Tooltip("Default Camera Distance set to the Third Person Cinemachine Camera")]
        public FloatReference CameraDistance = new(6);

        [Tooltip("Multiplier to rotate the X Axis")]
        public FloatReference XMultiplier = new(1);
        [Tooltip("Multiplier to rotate the Y Axis")]
        public FloatReference YMultiplier = new(1);

        [Tooltip("How far in degrees can you move the camera up")]
        public FloatReference TopClamp = new(70.0f);

        [Tooltip("How far in degrees can you move the camera down")]
        public FloatReference BottomClamp = new(-30.0f);

        [Tooltip("Lerp Rotation to smooth out the movement of the camera while rotating.")]
        public FloatReference LerpRotation = new(15f);


        [Tooltip("Lerp Position to smooth out the movement of the camera while following the target.")]
        public FloatReference lerpPosition = new(0);

        private float InvertX => invertX.Value ? -1 : 1;
        private float InvertY => invertY.Value ? 1 : -1;

        public float XSensibility { get => XMultiplier; set => XMultiplier.Value = value; }
        public float YSensibility { get => YMultiplier; set => YMultiplier.Value = value; }
        public float LerpPosition { get => lerpPosition; set => lerpPosition.Value = value; }
        public Transform UpVector { get => upVector; set => upVector.Value = value; }
        public bool UnScaledTime { get => unscaledTime; set => unscaledTime.Value = value; }
        public bool SetInvertX(bool value) => invertX.Value = value;
        public bool SetInvertY(bool value) => invertY.Value = value;


        public float CameraSide { get => cameraSide; set => cameraSide = value; }

        // cinemachine
        public float _cinemachineTargetYaw;
        public float _cinemachineTargetPitch;
        private const float _threshold = 0.00001f;
        public BoolEvent OnActiveCamera = new();


        readonly WaitForFixedUpdate mWaitForFixedUpdate = new();
        readonly WaitForEndOfFrame mWaitForLateUpdate = new();

        // Start is called before the first frame update
        void Awake()
        {
            if (Brain == null) Brain = FindObjectOfType<CinemachineBrain>();

            CM3PFollow = this.FindComponent<Cinemachine3rdPersonFollow>();

            if (CM3PFollow != null)
            {
                CM3PFollow.CameraDistance = CameraDistance;
                CM3PFollow.CameraSide = CameraSide;
            }
        }


        private void OnEnable()
        {
            //Brain.m_CameraActivatedEvent.AddListener(CameraChanged);
            TPFCameras ??= new(); //Initialize the Cameras
            TPFCameras.Add(this);


            //Search on the other TFP cameras to see if we are using the same Target...
            //if we are using the same Target use their Cam Pivot instead
            if (CamPivot == null)
            {
                foreach (var c in TPFCameras)
                {
                    if (c == this) continue; //Skip itself

                    //If another Camera is using the same 
                    if (c.Target.Value == Target.Value && c.CamPivot != null)
                    {
                        CamPivot = c.CamPivot; //Use the same Cam Pivot
                        break;
                    }
                }
            }

            if (CamPivot == null) //There's no CamPivot after searching in all other Cameras, let's create one
            {
                CamPivot = new GameObject($"CamPivot - [{(Target.Value != null ? Target.Value.name : name)}]").transform;
                CamPivot.parent = transform;
                CamPivot.ResetLocal();
                CamPivot.parent = null;
                //  CamPivot.hideFlags = HideFlags.HideInHierarchy; //Hide it we do not need to see it
            }


            //Find the Cinemachine camera target
            if (TryGetComponent(out ThisCamera) && ThisCamera.Follow == null)
                ThisCamera.Follow = CamPivot.transform;

            transform.position = CamPivot.position;

            CameraPosition(0, 0);
            StartCameraLogic();
        }

        private void OnDisable()
        {
            // Brain.m_CameraActivatedEvent.RemoveListener(CameraChanged);
            StopAllCoroutines();

            transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            TPFCameras.Remove(this);
        }

        private void StartCameraLogic()
        {
            if (updateMode == UpdateType.FixedUpdate)
            {
                StartCoroutine(AfterPhysics());
            }
            else
            {
                StartCoroutine(AfterLateUpdate());
            }
        }

        private IEnumerator AfterPhysics()
        {
            while (true)
            {
                CameraLogic(UnScaledTime ? Time.fixedUnscaledDeltaTime : Time.fixedDeltaTime);

                yield return mWaitForFixedUpdate;
            }
        }

        private IEnumerator AfterLateUpdate()
        {
            while (true)
            {
                CameraLogic(UnScaledTime ? Time.unscaledDeltaTime : Time.deltaTime);
                yield return mWaitForLateUpdate;
            }
        }


        private bool active;

        private void CameraLogic(float deltaTime)
        {
            //Update the Active Camera if we are the active camera
            if (ThisCamera == Brain.ActiveVirtualCamera)
            {
                if (!active)
                {
                    if (ActiveThirdPersonCamera != null)
                        ActiveThirdPersonCamera.active = false; //Old Camera set it to false

                    ActiveThirdPersonCamera = this;
                    active = true;
                    OnActiveCamera.Invoke(active);

                    // UpdateTPFCameras(); //Update all the TPF Cameras with this component
                    CameraPosition(LerpPosition, deltaTime);
                    //CheckRotation(); //Update the Rotation with the Camera Brain
                    return;     //Skip this cycle
                }
            }
            else
            {
                //Make sure this one is disabled
                if (active)
                {
                    active = false;
                    OnActiveCamera.Invoke(active);
                }
            }

            if (!active) return;


            //Skip if the TimeScale is zero
            if (Time.timeScale == 0)
            {
                look.Value = Vector2.zero;
                return;
            }

            if (ActiveThirdPersonCamera == this)
            {
                CameraPosition(LerpPosition, deltaTime);
                CameraRotation(deltaTime);
                SetCameraSide(CameraSide);
            }
        }

        private void CheckRotation()
        {
            var EulerAngles = Brain.transform.eulerAngles; //Get the Brain Rotation to save the movement 

            _cinemachineTargetYaw = ClampAngle(EulerAngles.y, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = EulerAngles.x > 180 ? EulerAngles.x - 360 : EulerAngles.x; //HACK!!!
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            CamPivot.rotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);

            UpdateAllCamerasYawPitch();
        }

        private void UpdateAllCamerasYawPitch()
        {
            foreach (var c in TPFCameras)
            {
                if (c.Target.Value != Target.Value) continue; //Skip if the camera is using different pivots

                //Update Rotation Values to all other cameras
                c._cinemachineTargetYaw = _cinemachineTargetYaw;
                c._cinemachineTargetPitch = _cinemachineTargetPitch;
            }
        }

        public void SetLookX(float x) => look.x = x;
        public void SetLookY(float y) => look.y = y;
        public void SetLook(Vector2 look) => this.look.Value = look;

        private void CameraPosition(float lerp, float deltatime)
        {
            if (Target.Value)
            {
                if (lerp == 0)
                {
                    CamPivot.transform.position = Target.position;
                }
                else
                {
                    CamPivot.transform.position = Vector3.Lerp(CamPivot.transform.position, Target.position, lerp * deltatime);
                }
            }
        }

        private void CameraRotation(float deltaTime)
        {
            // if there is an input and camera position is not fixed
            if (look.Value.sqrMagnitude >= _threshold)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = 100 * deltaTime;

                _cinemachineTargetYaw += look.x * deltaTimeMultiplier * InvertX * XMultiplier;
                _cinemachineTargetPitch += look.y * deltaTimeMultiplier * InvertY * YMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Cinemachine will follow this target
            var TargetRotation = Quaternion.Euler(_cinemachineTargetPitch, _cinemachineTargetYaw, 0.0f);

            if (UpVector) TargetRotation = Quaternion.FromToRotation(Vector3.up, UpVector.up) * TargetRotation;

            CamPivot.rotation = Quaternion.Lerp(CamPivot.rotation, TargetRotation, deltaTime * LerpRotation); //NEEDED FOR SMOOTH CAMERA MOVEMENT

            UpdateAllCamerasYawPitch();
        }

        public void SetTarget(Transform target) => Target.Value = target;

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }


        public void SetPriority(bool value)
        {
            ThisCamera.Priority = value ? priority : -1;
        }

        public void SetCameraSide(bool value) => SetCameraSide(value ? 1 : 0);

        public void SetCameraSide(float value)
        {
            if (CameraSide != value)
            {
                CameraSide = value;
                CM3PFollow.CameraSide = CameraSide;
            }
        }

        private void OnValidate()
        {
            if (Application.isPlaying && CM3PFollow != null)
                CM3PFollow.CameraSide = CameraSide;
        }


#if UNITY_EDITOR
        private void Reset()
        {
            Target.UseConstant = false;
            Target.Variable = MTools.GetInstance<TransformVar>("Camera Target");


            if (CamPivot == null)
            {
                CamPivot = new GameObject("Pivot").transform;
                CamPivot.parent = transform;
                CamPivot.ResetLocal();
            }
        }
#endif
    }
}
