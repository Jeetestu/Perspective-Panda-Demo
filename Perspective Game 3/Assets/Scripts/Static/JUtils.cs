using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace JUtils
{
    public static class JUtilsClass
    {

        public static bool isAlongAxis(Vector3 pos, Vector3Int axis, Vector3 targetPoint)
        {
            for (int i = 0; i < 3; i++)
            {
                if (axis[i] == 1 || axis[i] == -1)
                {
                    pos[i] = 0;
                    targetPoint[i] = 0;
                    return pos == targetPoint;
                }
            }
            return false;
        }
        public static Vector3Int getClosestHorizontalDirection(Vector3 forward, Vector3Int up)
        {
            List<Vector3Int> horizontalDirections = getAllHorizontalDirections(up);

            Vector3Int newForward = Vector3Int.zero;
            float smallestAngle = 999f;

            foreach (Vector3Int dir in horizontalDirections)
            {
                if (Vector3.Angle(forward, dir) < smallestAngle)
                {
                    newForward = dir;
                    smallestAngle = Vector3.Angle(forward, dir);
                }
            }
            return newForward;
        }

        public static Vector3Int[] getDirections()
        {
            Vector3Int[] output = new Vector3Int[6];

            output[0] = new Vector3Int(1, 0, 0);
            output[1] = new Vector3Int(-1, 0, 0);
            output[2] = new Vector3Int(0, 1, 0);
            output[3] = new Vector3Int(0, -1, 0);
            output[4] = new Vector3Int(0, 0, 1);
            output[5] = new Vector3Int(0, 0, -1);
            return output;
        }

        public static List<Vector3Int> getAllHorizontalDirections(Vector3Int up)
        {

            List<Vector3Int> horizontalDirections = new List<Vector3Int>();
            horizontalDirections.Add(new Vector3Int(1, 0, 0));
            horizontalDirections.Add(new Vector3Int(-1, 0, 0));
            horizontalDirections.Add(new Vector3Int(0, 1, 0));
            horizontalDirections.Add(new Vector3Int(0, -1, 0));
            horizontalDirections.Add(new Vector3Int(0, 0, 1));
            horizontalDirections.Add(new Vector3Int(0, 0, -1));

            horizontalDirections.RemoveAll(item => Vector3Int.Equals(item, up) || Vector3Int.Equals(item, -up));

            return horizontalDirections;
        }

        public static void colorBlock(GameObject block)
        {
            if (block != null)
            {
                block.GetComponent<Renderer>().material.color = Color.red;
            }
        }

        public static bool alignedAxis(Vector3 axisOne, Vector3 axisTwo)
        {
            return Vector3.Cross(axisOne, axisTwo) == Vector3.zero;
        }

        //returns true if the first vector is closer than the second vector along the provided axis
        public static bool closerAlongAxis(Vector3 first, Vector3 second, Vector3 axis)
        {
            for (int i = 0; i < 3; i++)
            {
                if (axis[i] == 1) return first[i] < second[i];
                if (axis[i] == -1) return first[i] > second[i];
            }
            return false;
        }

        //rounds vector to nearest X decimal points
        public static Vector3 roundVector(Vector3 value, int precision)
        {
            int precisionValue = Mathf.RoundToInt(10 ^ precision);

            return new Vector3(Mathf.Round(value.x * precisionValue) / precisionValue, Mathf.Round(value.y * precisionValue) / precisionValue, Mathf.Round(value.z * precisionValue) / precisionValue);
        }

        //like lerp, but with a spring effect
        public static float Spring(float from, float to, float time)
        {
            time = Mathf.Clamp01(time);
            time = (Mathf.Sin(time * Mathf.PI * (.2f + 2.5f * time * time * time)) * Mathf.Pow(1f - time, 2.2f) + time) * (1f + (1.2f * (1f - time)));
            return from + (to - from) * time;
        }

        public static Vector3Int vecAbs(Vector3Int input)
        {
            return new Vector3Int(Mathf.Abs(input.x), Mathf.Abs(input.y), Mathf.Abs(input.z));
        }

        public static Vector3 smoothStep(Vector3 startPos, Vector3 endPos, float progress)
        {
            return new Vector3(Mathf.SmoothStep(startPos.x, endPos.x, progress),
                    Mathf.SmoothStep(startPos.y, endPos.y, progress), Mathf.SmoothStep(startPos.z, endPos.z, progress));
        }

        public static string formatTime(float val)
        {
            int hours = (int)val / 3600;
            int minutes = (int)(val - (hours * 3600)) / 60;
            int seconds = (int)(val - (hours * 3600) - (minutes * 60));
            return string.Format("{0:00}:{1:00}:{2:00}", hours, minutes, seconds);
        }

        public static bool isImpassableGround(GroundType groundType)
        {
            return groundType.typeOfGround == GroundType.Type.impassable || groundType.typeOfGround == GroundType.Type.flatImpassable;
        }

        /// <summary>
        /// Given the perspectiveVector and gravityUp, what type of view is appropriate?
        /// </summary>
        /// <param name="perspectiveVector">zero = 3D view, aligned with gravity = vertical, otherwise it indicates a horizontal angle</param>
        /// <param name="gravityUp">The 'upward' direction according to gravity</param>
        /// <returns></returns>
        public static ViewType getView(Vector3Int perspectiveVector, Vector3Int gravityUp)
        {
            if (perspectiveVector == Vector3Int.zero)
            {
                return ViewType.normal;
            }
            else if (perspectiveVector == gravityUp || perspectiveVector == -gravityUp)
            {
                return ViewType.vertical;
            }
            else
            {
                return ViewType.horizontal;
            }
        }

        public static bool isIceBlock(GameObject block)
        {
            GroundType gt = block.GetComponent<GroundType>();
            return gt.typeOfGround == GroundType.Type.ice || gt.typeOfGround == GroundType.Type.flatIce;
        }

        /// <summary>
        /// Based on the given unit direction, returns the necessary position and rotation for a plane to sit on the face of the block
        /// </summary>
        /// <param name="direction">Unit vector representing the direction of the face from the centre of the block</param>
        /// <param name="position">localPosition of the plane</param>
        /// <param name="rotation">rotation of the plane</param>
        public static void getFaceCoordinates (Vector3Int direction, out Vector3 position, out Vector3 rotation)
        {

            //the coordinates are not symetrical because the child of a cube has a slight y-offset of 0.5
            if (direction.y == 1)
            {
                position = new Vector3(0, 1, 0);
                rotation = new Vector3(0, 0, 0);
            }
            else if (direction.y == -1)
            {
                position = new Vector3(0, 0, 0);
                rotation = new Vector3(0, 0, -180);
            }
            else if (direction.x == 1)
            {
                position = new Vector3(0.5f,0.5f,0);
                rotation = new Vector3(0,0,-90);
            }
            else if (direction.x == -1)
            {
                position = new Vector3(-0.5f, 0.5f, 0);
                rotation = new Vector3(0, 0, 90);
            }
            else if (direction.z == 1)
            {
                position = new Vector3(0, 0.5f, 0.5f);
                rotation = new Vector3(90, 0, 0);
            }
            else //(direction.z == -1)
            {
                position = new Vector3(0, 0.5f, -0.5f);
                rotation = new Vector3(-90, 0, 0);
            }
        }
    }
}
