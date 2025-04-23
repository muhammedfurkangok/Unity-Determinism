using SoftFloat;

namespace PhysicSystem
{
    public class BallData
    {
        public sfloat PositionX;
        public sfloat PositionY;
        public sfloat VelocityX;
        public sfloat VelocityY;
        public sfloat Radius;

        public sfloat JumpForce;
        public sfloat MoveSpeed;



        public void UpdatePosition(sfloat deltaTime, sfloat gravity, sfloat leftBoundary, sfloat rightBoundary,
            sfloat bottomBoundary, sfloat topBoundary)
        {
            VelocityY = VelocityY + (gravity * deltaTime);
            PositionX = PositionX + (VelocityX * deltaTime);
            PositionY = PositionY + (VelocityY * deltaTime);

            CheckCollisions(leftBoundary, rightBoundary, bottomBoundary, topBoundary);
        }

        private void CheckCollisions(sfloat leftBoundary, sfloat rightBoundary, sfloat bottomBoundary, sfloat topBoundary)
        {
            if ((PositionX - Radius) < leftBoundary)
            {
                PositionX = leftBoundary + Radius;
                VelocityX = (sfloat)0;
            }
            else if ((PositionX + Radius) > rightBoundary)
            {
                PositionX = rightBoundary - Radius;
                VelocityX = (sfloat)0;
            }

            if ((PositionY + Radius) > topBoundary)
            {
                PositionY = topBoundary - Radius;
                VelocityY = (sfloat)0;
            }
            else if ((PositionY - Radius) < bottomBoundary)
            {
                PositionY = bottomBoundary + Radius;
                VelocityY = (sfloat)0;
            }
        }

        public void ApplyHorizontalInput(sfloat horizontalInput)
        {
            VelocityX = horizontalInput * MoveSpeed;
        }

        public void ApplyJump(bool jump, sfloat bottomBoundary)
        {
            if (jump && IsOnGround(bottomBoundary))
            {
                VelocityY = JumpForce;
            }
        }

        public bool IsOnGround(sfloat bottomBoundary)
        {
            return (PositionY - Radius) <= bottomBoundary;
        }
    }
}