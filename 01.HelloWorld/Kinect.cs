using CCT.NUI.Core;
using CCT.NUI.HandTracking;
using CCT.NUI.KinectSDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace _01.HelloWorld
{
    class Kinect
    {
        const double pinchFingerDist = 0.003d;
        const double openHandDist = 0.0075d;
        //const double closedHandInterval = .0015d;
        const double angleRotationThreshhold = .250d;
        const double translationThreshhold = 10;
        const double extractionThreshold = 2.0d;
        const double zoomThreshold = 20;
        const double morphThreshold = 20;

        bool leftFistRightOpen = false;
        bool leftOpenRightFist = false;
        bool leftFistRightL = false;
        bool leftLRightL = false;
        bool justEnabledPotterWheel = false;

        public bool isRotating = false;
        public bool isTranslating = false;
        public bool isZoom = false;
        public bool isMorphing = false;
        public bool potterWheelMode = false;

        double initAngle = 0;
        double initDistLR = 0.0d;
        Point initPos;

        int graceTimer = 0;
        int maxGracePeriod = 5;

        //Variables public for Irrlicht reading:
        public double rotation = 0;
        public Point translation;
        public Point position;
        public double zoomDist = 0.0d;
        public double morphDist = 0.0d;

        public Kinect()
        {
            IDataSourceFactory dataSourceFactory = new SDKDataSourceFactory(true);
            var handDataSource = new HandDataSource(dataSourceFactory.CreateShapeDataSource());

            handDataSource.NewDataAvailable += new NewDataHandler<HandCollection>(handDataSource_NewDataAvailable);
            handDataSource.Start();
        }

        private double getDistance(Point loc1, Point loc2)
        {
            return Math.Sqrt((loc1.X - loc2.X) * (loc1.X - loc2.X) + (loc1.Y - loc2.Y) * (loc1.Y - loc2.Y) + (loc1.Z - loc2.Z) * (loc1.Z - loc2.Z));
        }
        private double getDistFromOrigin(Point loc1)
        {
            return Math.Sqrt(loc1.X * loc1.X + loc1.Y * loc1.Y + loc1.Z * loc1.Z );
        }

        private double getDistanceXY(Point loc1, Point loc2)
        {
            return Math.Sqrt((loc1.X - loc2.X) * (loc1.X - loc2.X) + (loc1.Y - loc2.Y) * (loc1.Y - loc2.Y));
        }

        private double getFingerAngle(IList<FingerPoint> fingers)
        {
            if (fingers.Count < 2) { return 0; }
            int leftMost = 0, rightMost = 1;

            for (int i = 2; i < fingers.Count; i++)
            {
                if(getDistance(fingers[leftMost].Location, fingers[i].Location) > getDistance(fingers[leftMost].Location, fingers[rightMost].Location))
                {
                    if(getDistance(fingers[leftMost].Location, fingers[i].Location) < getDistance(fingers[rightMost].Location, fingers[i].Location))
                    {
                        leftMost = i;
                    }
                    else
                    {
                        rightMost = i;
                    }
                }

                else if(getDistance(fingers[rightMost].Location, fingers[i].Location) > getDistance(fingers[leftMost].Location, fingers[rightMost].Location))
                {
                    leftMost = i;
                }
            }

            return getAngle(fingers[leftMost].Location, fingers[rightMost].Location);
        }
        private double getAngle(Point loc1, Point loc2)
        {
            if (loc1.X == loc2.X)
            {
                return 0;
            }
            return Math.Atan((loc1.Y - loc2.Y) / (loc1.X - loc2.X))
                + ((loc1.X < loc2.X) ? Math.PI : 0);
        }
        private bool isHandPinched(HandData hand)
        {
            //Console.WriteLine(hand.FingerCount);
            for (int i = 0; i < hand.FingerCount; i++)
            {
                //Console.WriteLine(getDistanceXY(hand.Fingers.ElementAt(i).Fingertip, hand.Location));
                if (getDistanceXY(hand.Fingers.ElementAt(i).Fingertip, hand.Location) > pinchFingerDist)
                {
                    return false;
                }
            }
            return true;
        }

        private bool isHandOpened(HandData hand)
        {
            bool onceIsOkay = true;
            for (int i = 0; i < hand.FingerCount; i++)
            {
                if (getDistance(hand.Fingers.ElementAt(i).Fingertip, hand.Location) < openHandDist)
                {
                    if (onceIsOkay)
                    {
                        onceIsOkay = false;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /*private bool isClosedFist(IList<FingerPoint> fingers)
        {
            return true;
            for (int i = 2; i < fingers.Count; i++)
            {
                
            }
        }*/
        /*private double averageHandAngle(HandData hand)
        {
            if (hand.FingerCount == 0) { return 0; }
            double totalAngle = 0;
            for (int i = 0; i < hand.FingerCount; i++)
            {
                if (hand.FingerPoints[i].Location.X == hand.Location.X)
                {
                    totalAngle += Math.PI / 2;
                }
                else
                {
                    totalAngle += (hand.FingerPoints[i].Location.X < hand.Location.X) ? Math.PI : 0;
                    totalAngle += Math.Atan((hand.FingerPoints[i].Location.Y - hand.Location.Y) / (hand.FingerPoints[i].Location.X - hand.Location.X));
                }
            }
            totalAngle /= hand.FingerCount;
            return totalAngle;
            /*
            int maxDist = 0;
            for (int i = 1; i < hand.FingerCount; i++)
            {
                if (getDistanceXY(hand.Fingers.ElementAt(i).Fingertip, hand.Location) > getDistanceXY(hand.Fingers.ElementAt(maxDist).Fingertip, hand.Location))
                {
                    maxDist = i;
                }
            }
            if(hand.FingerPoints[maxDist].Location.X == hand.Location.X)
            {
                return Math.PI/2;
            }
            return Math.Atan((hand.FingerPoints[maxDist].Location.Y - hand.Location.Y) / (hand.FingerPoints[maxDist].Location.X - hand.Location.X))
                + ((hand.FingerPoints[maxDist].Location.X < hand.Location.X) ? Math.PI : 0);*/
        /*}*/
        private void allFlagsDown()
        {
            leftFistRightOpen = false;
            leftFistRightL = false;
            leftLRightL = false;
            leftOpenRightFist = false;
            isRotating = false;
            isTranslating = false;
            isMorphing = false;
            isZoom = false;
        }

        private void handDataSource_NewDataAvailable(HandCollection data)
        {
            ///////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////Get Correct Hands ///////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////
            int left = 0;
            int right = 1;
            /*if (data.Hands.Count == 1)
            {
                Console.WriteLine("Num fingers: " + data.Hands[0].FingerCount);
            }*/
            if (data.Hands.Count < 2)
            {
                return;
            }


            if (data.Hands[right].Location.Z > data.Hands[left].Location.Z)
            {
                left = 1; right = 0;
            }

            for (int i = 2; i < data.Hands.Count; i++)
            {
                if (data.Hands[i].Location.Z < data.Hands[left].Location.Z)
                {
                    if (data.Hands[right].Location.Z > data.Hands[i].Location.Z)
                    {
                        left = right;
                        right = i;
                    }

                    else
                    {
                        left = i;
                    }
                }
            }

            if (data.Hands[right].Location.X < data.Hands[left].Location.X)
            {
                int temp = left;
                left = right;
                right = temp;
            }

            ///////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////End Get Correct Hands ///////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////



            ///////////////////////////////////////////////////////////////////////////////////////
            //////////////////////////////Check for Commands///////////////////////////////////////
            ///////////////////////////////////////////////////////////////////////////////////////

            Console.WriteLine(data.Hands[left].FingerCount + ", " + data.Hands[right].FingerCount);
            if (leftFistRightOpen)
            {
                if (graceTimer >= maxGracePeriod)
                {
                    if (!isRotating && Math.Abs(initAngle - getFingerAngle(data.Hands[right].FingerPoints)) > angleRotationThreshhold)
                    {
                        isRotating = true;
                        initAngle = getFingerAngle(data.Hands[right].FingerPoints);
                        isTranslating = false;
                    }
                    else if (isRotating)
                    {
                        rotation = getFingerAngle(data.Hands[right].FingerPoints) - initAngle;
                        Console.WriteLine("Rotating: " + rotation);
                    }
                    
                }
                else
                {
                    initAngle = getFingerAngle(data.Hands[right].FingerPoints);
                    initPos = data.Hands[right].Location;
                    graceTimer++;
                }
                
            }
            else if (leftFistRightL)
            {
                if (!isTranslating && getDistanceXY(initPos, data.Hands[right].Location) > translationThreshhold)
                {
                    isRotating = false;
                    initPos = data.Hands[right].Location;
                    isTranslating = true;
                    translation.X = translation.Y = translation.Z = 0;
                }
                else if (isTranslating)
                {
                    translation.X += initPos.X - data.Hands[right].Location.X;
                    translation.Y += initPos.Y - data.Hands[right].Location.Y;
                    translation.Z += initPos.Z - data.Hands[right].Location.Z;
                    initPos = data.Hands[right].Location;
                    Console.WriteLine("Translating. X: " + translation.X + ", Y: " + translation.Y + ", Z: " + translation.Z);
                }
            }
            else if (leftLRightL)
            {
                //Console.WriteLine("LLRL");
                if (graceTimer >= maxGracePeriod)
                {
                    //Console.WriteLine("GraceOver");
                    if (!isZoom && Math.Abs(initDistLR - getDistanceXY(data.Hands[left].Location, data.Hands[right].Location)) > zoomThreshold)
                    {
                        isZoom = true;
                    }
                    if (isZoom)
                    {
                        zoomDist = getDistanceXY(data.Hands[left].Location, data.Hands[right].Location) - initDistLR;
                        Console.WriteLine("Zoom Amt: " + zoomDist);
                    }
                }
                else
                {
                    graceTimer++;
                    initDistLR = getDistanceXY(data.Hands[left].Location, data.Hands[right].Location);
                }
            }
            else if (leftOpenRightFist)
            {
                if (graceTimer >= maxGracePeriod)
                {
                    if (!isMorphing && getDistance(initPos, data.Hands[right].Location) > morphThreshold)
                    {
                        isMorphing = true;
                    }
                    if (isMorphing)
                    {
                        double dir = getDistFromOrigin(data.Hands[right].Location) - getDistFromOrigin(initPos);
                        morphDist = dir/Math.Abs(dir) * getDistance(initPos, data.Hands[right].Location);
                        Console.WriteLine("Morph Amt:" + morphDist);
                    }
                }
                else
                {
                    graceTimer++;
                    initPos = data.Hands[right].Location;
                }
            }

            //Positives
            if (data.Hands[left].FingerCount <= 1 && data.Hands[right].FingerCount == 5)
            {
                if (!leftFistRightOpen)
                {
                    Console.WriteLine("leftFistRightOpen is being set.");
                    allFlagsDown();
                    leftFistRightOpen = true;
                    isRotating = true;
                    graceTimer = 0;
                    initAngle = getFingerAngle(data.Hands[right].FingerPoints);
                    initPos = data.Hands[left].Location;
                }
            }
            else if (data.Hands[left].FingerCount <= 1 && data.Hands[right].FingerCount == 2)
            {
                if (!leftFistRightL)
                {
                    Console.WriteLine("leftFistRightL is being set.");
                    allFlagsDown();
                    leftFistRightL = true;
                    isTranslating = true;
                    graceTimer = 0;
                    initAngle = getFingerAngle(data.Hands[right].FingerPoints);
                    initPos = data.Hands[left].Location;
                }
            }
            else if (data.Hands[left].FingerCount == 2 && data.Hands[right].FingerCount == 2)
            {
                if (!leftLRightL)
                {
                    initDistLR = getDistanceXY(data.Hands[left].Location, data.Hands[right].Location);
                    Console.WriteLine("leftLRightL is being set.");
                    allFlagsDown();
                    leftLRightL = true;
                    isZoom = true;
                    graceTimer = 0;
                }
            }
            else if (data.Hands[left].FingerCount == 5 && data.Hands[right].FingerCount <= 1)
            {
                if (!leftOpenRightFist)
                {
                    initDistLR = getDistanceXY(data.Hands[left].Location, data.Hands[right].Location);
                    Console.WriteLine("leftORightL is being set.");
                    allFlagsDown();
                    leftOpenRightFist = true;
                    isMorphing = true;
                    graceTimer = 0;
                    initPos = data.Hands[left].Location;
                }
            }
            //Negatives
            else if ((data.Hands[left].FingerCount != 2 && data.Hands[right].FingerCount != 2) ||
                    (data.Hands[left].FingerCount > 1 && data.Hands[right].FingerCount < 3) ||
                    (data.Hands[left].FingerCount > 1 && data.Hands[right].FingerCount > 3) ||
                    (data.Hands[left].FingerCount < 3 && data.Hands[right].FingerCount > 1))
            {
                allFlagsDown();
                graceTimer = -1;
            }
            position = data.Hands[right].Location;

            /*if (isHandPinched(data.Hands[right]))
            {
                //Console.WriteLine("Hand is Pinched");
            }*/

            /*if (isHandOpened(data.Hands[left]) || isHandOpened(data.Hands[right]))
            {
                //Console.WriteLine("Hand is Opened");
            }*/

        }
        public void resetTranslation()
        {
            translation.X = 0;
            translation.Y = 0;
            translation.Z = 0;
        }
    }
}
