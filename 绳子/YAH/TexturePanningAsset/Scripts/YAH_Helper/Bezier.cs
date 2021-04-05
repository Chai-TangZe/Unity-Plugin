/* 
* Created by You Are Here LLC, 2020
* https://www.yahagency.com/
*
* For questions or assistance please contact
* pluginsupport@yahagency.com
*/

using UnityEngine;

namespace com.yah.LineRendererDemo
{
    /// <summary>
    /// Generic Bezier class capable of calculating Linear, Quadratic, and Cubic curves.
    /// </summary>
    public class Bezier
    {
        private int numPoints = 0;

        /// <summary>
        /// creates a new instance of the Bezier class clarifying the number of points on the curve.
        /// </summary>
        /// <param name="numberOfPoints"></param>
        public Bezier(int numberOfPoints)
        {
            numPoints = numberOfPoints;
        }

        public int NumberOfPoints
        {
            get { return numPoints; }
            set { numPoints = value; }
        }

        public Vector3[] GetLinearCurvePoints(Vector3 point0, Vector3 point1)
        {
            Vector3[] positions = new Vector3[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                float t = i / (float)numPoints;
                positions[i] = CalculateLinearCurvePoint(t, point0, point1);
            }

            positions[positions.Length - 1] = CalculateLinearCurvePoint(1, point0, point1);

            return positions;
        }

        public Vector3[] GetQuadraticCurvePoints(Vector3 point0, Vector3 point1, Vector3 point2)
        {
            Vector3[] positions = new Vector3[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                float t = i / (float)numPoints;
                positions[i] = CalculateQuadraticCurvePoint(t, point0, point1, point2);
            }

            positions[positions.Length - 1] = CalculateQuadraticCurvePoint(1, point0, point1, point2);

            return positions;
        }

        public Vector3[] GetCubicCurvePoints(Vector3 point0, Vector3 point1, Vector3 point2, Vector3 point3)
        {
            Vector3[] positions = new Vector3[numPoints];

            for (int i = 0; i < numPoints; i++)
            {
                float t = i / (float)numPoints;
                positions[i] = CalculateCubicCurvePoint(t, point0, point1, point2, point3);
            }

            positions[positions.Length - 1] = CalculateCubicCurvePoint(1, point0, point1, point2, point3);

            return positions;
        }

        private Vector3 CalculateLinearCurvePoint(float t, Vector3 point0, Vector3 point1)
        {
            Vector3 newPoint = point0 + t * (point1 - point0);

            return newPoint;
        }

        private Vector3 CalculateQuadraticCurvePoint(float t, Vector3 point0, Vector3 point1, Vector3 point2)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 newPoint = (uu * point0) + (2 * u * t * point1) + (tt * point2);

            return newPoint;
        }

        private Vector3 CalculateCubicCurvePoint(float t, Vector3 point0, Vector3 point1, Vector3 point2, Vector3 point3)
        {
            float u = 1 - t;
            float tt = t * t;
            float uu = u * u;
            float ttt = tt * t;
            float uuu = uu * u;

            Vector3 newPoint = (uuu * point0) + (3 * uu * t * point1) + (3 * u * tt * point2) + (ttt * point3);

            return newPoint;
        }
    }
}