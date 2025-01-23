using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Rendering;
using Unity.Netcode;
using Unity.Netcode.Components;

namespace Fragsurf.Movement
{
    /// <summary>
    /// Easily add a surfable character to the scene
    /// Now modified to use Unity Netcode for GameObjects (server-authoritative).
    /// </summary>
    [AddComponentMenu("Fragsurf/Surf Character")]
    [RequireComponent(typeof(NetworkTransform))]
    public class SurfCharacter : NetworkBehaviour, ISurfControllable
    {
        public enum ColliderType
        {
            Capsule,
            Box
        }

        ///// Fields /////

        [Header("Physics Settings")]
        public Vector3 colliderSize = new Vector3(1f, 2f, 1f);
        [HideInInspector] public ColliderType collisionType { get { return ColliderType.Capsule; } }
        public float weight = 75f;
        public float rigidbodyPushForce = 2f;
        public bool solidCollider = false;

        [Header("View Settings")]
        public Transform viewTransform;
        public Transform playerRotationTransform;

        [Header("Crouching setup")]
        public float crouchingHeightMultiplier = 0.5f;
        public float crouchingSpeed = 10f;
        float defaultHeight;
        bool allowCrouch = true; // separate toggling

        [Header("Features")]
        public bool crouchingEnabled = true;
        public bool slidingEnabled = false;
        public bool laddersEnabled = true;
        public bool supportAngledLadders = true;

        [Header("Step offset (can be buggy)")]
        public bool useStepOffset = false;
        public float stepOffset = 0.35f;

        [Header("Movement Config")]
        [SerializeField]
        public MovementConfig movementConfig;

        private GameObject _groundObject;
        private Vector3 _baseVelocity;
        private Collider _collider;
        private Vector3 _angles;
        private Vector3 _startPosition;
        private GameObject _colliderObject;
        private GameObject _cameraWaterCheckObject;
        private CameraWaterCheck _cameraWaterCheck;

        // Make the MoveData a server-writable NetworkVariable
        // so only the server can update it, but all clients can read it.
        private NetworkVariable<MoveData> _moveData = new NetworkVariable<MoveData>(
            new MoveData(),
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        private SurfController _controller = new SurfController();
        private Rigidbody rb;
        private List<Collider> triggers = new List<Collider>();
        private int numberOfTriggers = 0;
        private bool underwater = false;

        ///// Properties /////

        public MoveType moveType { get { return MoveType.Walk; } }
        public MovementConfig moveConfig { get { return movementConfig; } }
        public MoveData moveData { get { return _moveData.Value; } }
        public new Collider collider { get { return _collider; } }

        public GameObject groundObject
        {
            get { return _groundObject; }
            set { _groundObject = value; }
        }

        public Vector3 baseVelocity { get { return _baseVelocity; } }

        public Vector3 forward { get { return viewTransform != null ? viewTransform.forward : Vector3.forward; } }
        public Vector3 right { get { return viewTransform != null ? viewTransform.right : Vector3.right; } }
        public Vector3 up { get { return viewTransform != null ? viewTransform.up : Vector3.up; } }

        Vector3 prevPosition;

        ///// Methods /////

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, colliderSize);
        }

        private void Awake()
        {
            // Assign references
            _controller.playerTransform = playerRotationTransform;
            if (viewTransform != null)
            {
                _controller.camera = viewTransform;
                _controller.cameraYPos = viewTransform.localPosition.y;
            }
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            // If *this* is not your character, you might want to disable local cameras, etc.
            // Example: Only the owner should have the camera active
            if (!IsOwner && viewTransform != null)
            {
                // Could disable camera or remove audio listener, etc.
                // viewTransform.GetComponent<Camera>().enabled = false; // Example
            }
        }

        private void Start()
        {
            // Create a separate object for the collider
            _colliderObject = new GameObject("PlayerCollider");
            _colliderObject.layer = gameObject.layer;
            _colliderObject.transform.SetParent(transform);
            _colliderObject.transform.rotation = Quaternion.identity;
            _colliderObject.transform.localPosition = Vector3.zero;
            _colliderObject.transform.SetSiblingIndex(0);

            // Water check object
            _cameraWaterCheckObject = new GameObject("Camera water check");
            _cameraWaterCheckObject.layer = gameObject.layer;
            if (viewTransform != null)
            {
                _cameraWaterCheckObject.transform.position = viewTransform.position;
            }

            SphereCollider _cameraWaterCheckSphere = _cameraWaterCheckObject.AddComponent<SphereCollider>();
            _cameraWaterCheckSphere.radius = 0.1f;
            _cameraWaterCheckSphere.isTrigger = true;

            Rigidbody _cameraWaterCheckRb = _cameraWaterCheckObject.AddComponent<Rigidbody>();
            _cameraWaterCheckRb.useGravity = false;
            _cameraWaterCheckRb.isKinematic = true;

            _cameraWaterCheck = _cameraWaterCheckObject.AddComponent<CameraWaterCheck>();

            prevPosition = transform.position;

            if (viewTransform == null)
            {
                // fallback to main camera if no transform specified
                viewTransform = Camera.main != null ? Camera.main.transform : null;
            }
            if (playerRotationTransform == null && transform.childCount > 0)
            {
                playerRotationTransform = transform.GetChild(0);
            }

            // Remove any existing collider on main object
            _collider = gameObject.GetComponent<Collider>();
            if (_collider != null)
            {
                Destroy(_collider);
            }

            // RigidBody required for triggers
            rb = gameObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.angularDamping = 0f;
            rb.linearDamping = 0f;
            rb.mass = weight;

            allowCrouch = crouchingEnabled;

            // Setup collider
            switch (collisionType)
            {
                // Box collider
                case ColliderType.Box:
                    _collider = _colliderObject.AddComponent<BoxCollider>();
                    var boxc = (BoxCollider)_collider;
                    boxc.size = colliderSize;
                    defaultHeight = boxc.size.y;
                    break;

                // Capsule collider
                case ColliderType.Capsule:
                    _collider = _colliderObject.AddComponent<CapsuleCollider>();
                    var capc = (CapsuleCollider)_collider;
                    capc.height = colliderSize.y;
                    capc.radius = colliderSize.x / 2f;
                    defaultHeight = capc.height;
                    break;
            }

            // Initialize movement data
            MoveData startData = _moveData.Value;
            startData.slopeLimit = movementConfig.slopeLimit;
            startData.rigidbodyPushForce = rigidbodyPushForce;
            startData.slidingEnabled = slidingEnabled;
            startData.laddersEnabled = laddersEnabled;
            startData.angledLaddersEnabled = supportAngledLadders;
            startData.playerTransform = transform;
            startData.viewTransform = viewTransform;
            if (viewTransform != null)
            {
                startData.viewTransformDefaultLocalPos = viewTransform.localPosition;
            }
            startData.defaultHeight = defaultHeight;
            startData.crouchingHeight = crouchingHeightMultiplier;
            startData.crouchingSpeed = crouchingSpeed;
            _collider.isTrigger = !solidCollider;
            startData.origin = transform.position;
            _startPosition = transform.position;
            startData.useStepOffset = useStepOffset;
            startData.stepOffset = stepOffset;

            _moveData.Value = startData;
        }

        private void Update()
        {
            // -------- CLIENT-SIDE INPUT COLLECTION (only the owner does this) --------
            if (!IsOwner) 
            {
                return; // not my player, don't read input
            }

            // Gather local input
            float vertical = Input.GetAxisRaw("Vertical");
            float horizontal = Input.GetAxisRaw("Horizontal");
            bool sprinting = Input.GetButton("Sprint");
            bool jumpPressed = Input.GetButton("Jump");
            bool crouchHeld = Input.GetButton("Crouch");

            // Because Input.GetButtonDown won't easily sync, we can pass booleans
            bool crouchDown = Input.GetButtonDown("Crouch");
            bool jumpDown = Input.GetButtonDown("Jump");

            // Send to server for authoritative movement
            SendInputToServerRpc(vertical, horizontal, jumpPressed, jumpDown, crouchHeld, crouchDown, sprinting);
        }

        /// <summary>
        /// ServerRpc to receive input from the owner client. 
        /// Only the server can write to _moveData. 
        /// </summary>
        [ServerRpc]
        private void SendInputToServerRpc(float vertical, float horizontal, bool jumpPressed, bool jumpDown, 
            bool crouchHeld, bool crouchDown, bool sprinting)
        {
            if (!IsServer) return;

            // Get current data
            MoveData data = _moveData.Value;

            // Basic axis
            data.verticalAxis = vertical;
            data.horizontalAxis = horizontal;

            // Wish jump can be set if jump is being pressed
            data.wishJump = jumpPressed;

            // If "crouchDown" or "crouchHeld", we can set data.crouching
            // or do your toggling logic here if needed
            data.crouching = crouchHeld;

            // If sprint is pressed
            data.sprinting = sprinting;

            // Convert axes to movement
            if (!Mathf.Approximately(horizontal, 0f))
            {
                data.sideMove = horizontal > 0f 
                    ? movementConfig.acceleration 
                    : -movementConfig.acceleration;
            }
            else
            {
                data.sideMove = 0f;
            }

            if (!Mathf.Approximately(vertical, 0f))
            {
                data.forwardMove = vertical > 0f
                    ? movementConfig.acceleration
                    : -movementConfig.acceleration;
            }
            else
            {
                data.forwardMove = 0f;
            }

            // Write back
            _moveData.Value = data;
        }

        /// <summary>
        /// The server handles the movement in FixedUpdate() for consistency.
        /// </summary>
        private void FixedUpdate()
        {
            // Only the server should run the movement logic
            if (!IsServer)
            {
                return;
            }

            // ---------------------------------------
            // Update environment checks (triggers/water)
            HandleWaterAndTriggers();
            // ---------------------------------------

            // Crouch
            if (allowCrouch)
            {
                _controller.Crouch(this, movementConfig, Time.fixedDeltaTime);
            }

            // Process movement
            _controller.ProcessMovement(this, movementConfig, Time.fixedDeltaTime);

            // Update the actual position from moveData.origin
            transform.position = _moveData.Value.origin;
            
            // If your character rotates in _controller.ProcessMovement, 
            // you may also update transform.rotation or playerRotationTransform.rotation here
            // transform.rotation = someRotation; 
        }

        /// <summary>
        /// Handles triggers (e.g., Water), updates "underwater" if needed.
        /// For a fully authoritative approach, keep triggers on the server side as well.
        /// </summary>
        private void HandleWaterAndTriggers()
        {
            // Re-check triggers
            if (numberOfTriggers != triggers.Count)
            {
                numberOfTriggers = triggers.Count;
                underwater = false;
                triggers.RemoveAll(item => item == null);

                foreach (Collider trigger in triggers)
                {
                    if (trigger == null)
                        continue;

                    if (trigger.GetComponentInParent<Water>())
                    {
                        underwater = true;
                    }
                }
            }

            // Update camera water check
            if (viewTransform != null)
            {
                _cameraWaterCheckObject.transform.position = viewTransform.position;
            }
            MoveData md = _moveData.Value;
            md.cameraUnderwater = _cameraWaterCheck.IsUnderwater();
            md.underwater = underwater;
            _moveData.Value = md;
        }

        private void OnTriggerEnter(Collider other)
        {
            // We can keep track of triggers on all instances or only on the server. 
            // For purely server-authoritative movement, consider limiting trigger detection to the server instance only.
            if (!triggers.Contains(other))
            {
                triggers.Add(other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (triggers.Contains(other))
            {
                triggers.Remove(other);
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            // Only do collision logic on server side if you want it fully authoritative.
            if (!IsServer)
            {
                return;
            }

            if (collision.rigidbody == null)
                return;

            Vector3 relativeVelocity = collision.relativeVelocity * collision.rigidbody.mass / 50f;
            Vector3 impactVelocity = new Vector3(
                relativeVelocity.x * 0.0025f,
                relativeVelocity.y * 0.00025f,
                relativeVelocity.z * 0.0025f);

            float maxYVel = Mathf.Max(_moveData.Value.velocity.y, 10f);
            Vector3 newVelocity = new Vector3(
                _moveData.Value.velocity.x + impactVelocity.x,
                Mathf.Clamp(
                    _moveData.Value.velocity.y + Mathf.Clamp(impactVelocity.y, -0.5f, 0.5f),
                    -maxYVel,
                    maxYVel),
                _moveData.Value.velocity.z + impactVelocity.z
            );

            newVelocity = Vector3.ClampMagnitude(newVelocity, Mathf.Max(_moveData.Value.velocity.magnitude, 30f));

            // Write back to the moveData
            MoveData md = _moveData.Value;
            md.velocity = newVelocity;
            _moveData.Value = md;
        }

        public void Reset()
        {
            if (!IsServer) return;

            // Reset movement data
            MoveData md = _moveData.Value;
            md.velocity = Vector3.zero;
            md.origin = _startPosition;
            md.viewAngles = Vector3.zero; // Add this field to MoveData if missing
            md.crouching = false;
            md.sprinting = false;
            _moveData.Value = md;

            // Reset transforms
            transform.position = _startPosition;
            transform.rotation = Quaternion.identity;
            
            if (playerRotationTransform != null)
                playerRotationTransform.localRotation = Quaternion.identity;
            
            if (viewTransform != null)
            {
                viewTransform.localPosition = _moveData.Value.viewTransformDefaultLocalPos;
                viewTransform.localRotation = Quaternion.identity;
            }

            // Force network sync
            GetComponent<NetworkTransform>().Teleport(
                _startPosition,
                Quaternion.identity,
                Vector3.one
            );
        }



        /// <summary>
        /// A utility function you still might use for angle clamping.
        /// </summary>
        public static float ClampAngle(float angle, float from, float to)
        {
            if (angle < 0f)
                angle = 360 + angle;
            if (angle > 180f)
                return Mathf.Max(angle, 360 + from);
            return Mathf.Min(angle, to);
        }
    }
}