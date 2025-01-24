using System;
using Unity.Netcode;
using UnityEngine;

namespace Fragsurf.Movement {

    public enum MoveType {
        None,
        Walk,
        Noclip, // not implemented
        Ladder, // not implemented
    }

    public class MoveData: IEquatable<MoveData>, INetworkSerializable {

        ///// Fields /////
        
        public Transform playerTransform;
        public Transform viewTransform;
        public Vector3 viewTransformDefaultLocalPos;
        
        public Vector3 origin;
        public Vector3 viewAngles;
        public Vector3 velocity;
        public float forwardMove;
        public float sideMove;
        public float upMove;
        public float surfaceFriction = 1f;
        public float gravityFactor = 1f;
        public float walkFactor = 1f;
        public float verticalAxis = 0f;
        public float horizontalAxis = 0f;
        public bool wishJump = false;
        public bool crouching = false;
        public bool sprinting = false;

        public float slopeLimit = 45f;

        public float rigidbodyPushForce = 1f;

        public float defaultHeight = 2f;
        public float crouchingHeight = 1f;
        public float crouchingSpeed = 10f;
        public bool toggleCrouch = false;

        public bool slidingEnabled = false;
        public bool laddersEnabled = false;
        public bool angledLaddersEnabled = false;
        
        public bool climbingLadder = false;
        public Vector3 ladderNormal = Vector3.zero;
        public Vector3 ladderDirection = Vector3.forward;
        public Vector3 ladderClimbDir = Vector3.up;
        public Vector3 ladderVelocity = Vector3.zero;

        public bool underwater = false;
        public bool cameraUnderwater = false;

        public bool grounded = false;
        public bool groundedTemp = false;
        public float fallingVelocity = 0f;

        public bool useStepOffset = false;
        public float stepOffset = 0f;

        public bool Equals(MoveData other)
        {
            if (other == null) return false;

            return playerTransform == other.playerTransform &&
                viewTransform == other.viewTransform &&
                viewTransformDefaultLocalPos.Equals(other.viewTransformDefaultLocalPos) &&
                origin.Equals(other.origin) &&
                viewAngles.Equals(other.viewAngles) &&
                velocity.Equals(other.velocity) &&
                forwardMove == other.forwardMove &&
                sideMove == other.sideMove &&
                upMove == other.upMove &&
                surfaceFriction == other.surfaceFriction &&
                gravityFactor == other.gravityFactor &&
                walkFactor == other.walkFactor &&
                verticalAxis == other.verticalAxis &&
                horizontalAxis == other.horizontalAxis &&
                wishJump == other.wishJump &&
                crouching == other.crouching &&
                sprinting == other.sprinting &&
                slopeLimit == other.slopeLimit &&
                rigidbodyPushForce == other.rigidbodyPushForce &&
                defaultHeight == other.defaultHeight &&
                crouchingHeight == other.crouchingHeight &&
                crouchingSpeed == other.crouchingSpeed &&
                toggleCrouch == other.toggleCrouch &&
                slidingEnabled == other.slidingEnabled &&
                laddersEnabled == other.laddersEnabled &&
                angledLaddersEnabled == other.angledLaddersEnabled &&
                climbingLadder == other.climbingLadder &&
                ladderNormal.Equals(other.ladderNormal) &&
                ladderDirection.Equals(other.ladderDirection) &&
                ladderClimbDir.Equals(other.ladderClimbDir) &&
                ladderVelocity.Equals(other.ladderVelocity) &&
                underwater == other.underwater &&
                cameraUnderwater == other.cameraUnderwater &&
                grounded == other.grounded &&
                groundedTemp == other.groundedTemp &&
                fallingVelocity == other.fallingVelocity &&
                useStepOffset == other.useStepOffset &&
                stepOffset == other.stepOffset;
        }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref origin);
            serializer.SerializeValue(ref viewAngles);
            serializer.SerializeValue(ref velocity);
            serializer.SerializeValue(ref forwardMove);
            serializer.SerializeValue(ref sideMove);
            serializer.SerializeValue(ref upMove);
            serializer.SerializeValue(ref surfaceFriction);
            serializer.SerializeValue(ref gravityFactor);
            serializer.SerializeValue(ref walkFactor);
            serializer.SerializeValue(ref verticalAxis);
            serializer.SerializeValue(ref horizontalAxis);
            serializer.SerializeValue(ref wishJump);
            serializer.SerializeValue(ref crouching);
            serializer.SerializeValue(ref sprinting);
            serializer.SerializeValue(ref slopeLimit);
            serializer.SerializeValue(ref rigidbodyPushForce);
            serializer.SerializeValue(ref defaultHeight);
            serializer.SerializeValue(ref crouchingHeight);
            serializer.SerializeValue(ref crouchingSpeed);
            serializer.SerializeValue(ref toggleCrouch);
            serializer.SerializeValue(ref slidingEnabled);
            serializer.SerializeValue(ref laddersEnabled);
            serializer.SerializeValue(ref angledLaddersEnabled);
            serializer.SerializeValue(ref climbingLadder);
            serializer.SerializeValue(ref ladderNormal);
            serializer.SerializeValue(ref ladderDirection);
            serializer.SerializeValue(ref ladderClimbDir);
            serializer.SerializeValue(ref ladderVelocity);
            serializer.SerializeValue(ref underwater);
            serializer.SerializeValue(ref cameraUnderwater);
            serializer.SerializeValue(ref grounded);
            serializer.SerializeValue(ref groundedTemp);
            serializer.SerializeValue(ref fallingVelocity);
            serializer.SerializeValue(ref useStepOffset);
            serializer.SerializeValue(ref stepOffset);
        }
    }
}
