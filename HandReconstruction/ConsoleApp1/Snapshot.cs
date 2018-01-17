using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace ConsoleApp1
{
    public class Snapshot
    {
        public PXCMCapture.DeviceInfo DeviceInfo { get; set; }

        public PXCMCapture.Device.StreamProfileSet StreamProfileSet { get; set; }
        public PXCMCapture.StreamType MainPanel { get; set; }
        public PXCMCapture.StreamType PIPPanel { get; set; }

        public bool Synced { get; set; }


        PXCMSenseManager senseManager = PXCMSenseManager.CreateInstance();
        PXCMSession session;

        public Snapshot()
        {

           
            DeviceInfo = null;
            StreamProfileSet = null;
           
            MainPanel = PXCMCapture.StreamType.STREAM_TYPE_ANY;
            PIPPanel = PXCMCapture.StreamType.STREAM_TYPE_ANY;
            Synced = true;

            senseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_COLOR, 1920, 1080, 30);
            senseManager.EnableStream(PXCMCapture.StreamType.STREAM_TYPE_DEPTH, 640, 480, 60);
            Console.WriteLine(senseManager.captureManager.device);
            senseManager.Init();


            session = senseManager.QuerySession();
        }

        /// <summary>
        /// take a single color photo 
        /// </summary>
        /// <returns>returns a bitmap</returns>
        public Bitmap ColorSnapshot()
        {
            Bitmap bmp = new Bitmap(10, 10);
            for (; ; )
            {
                if (senseManager.AcquireFrame(true).IsError())
                {
                    throw new Exception("Failed to acquire frame");
                    break;
                }
                else
                {
                    // capture
                    PXCMCapture.Sample sample = senseManager.QuerySample();
                    PXCMImage MyImg = sample.color;

                    //test
                    PXCMImage.ImageData MyImgData;

                    MyImg.AcquireAccess(PXCMImage.Access.ACCESS_READ, out MyImgData);
                    bmp = MyImgData.ToBitmap(0, MyImg.info.width, MyImg.info.height);



                    // release
                    senseManager.ReleaseFrame();
                    break;
                }
            }

            ////should be used if we want to shut down the cam .
            //senseManager.Dispose();

            return bmp;
        }

        /// <summary>
        /// take a single depth grey color image shot
        /// </summary>
        /// <returns></returns>
        public WriteableBitmap DepthSnapshot()
        {
            for (; ; )
            {
                if (senseManager.AcquireFrame(true).IsError())
                {
                    throw new Exception("Failed to acquire frame");
                    break;
                }
                else
                {
                    // capture
                    PXCMCapture.Sample sample = senseManager.QuerySample();
                    PXCMImage MyImg = sample.depth;

                    //test
                    PXCMImage.ImageData MyImgData;

                    MyImg.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_DEPTH, out MyImgData);
                    WriteableBitmap WBmp = MyImgData.ToWritableBitmap(0, MyImg.info.width, MyImg.info.height, 96, 96);

                    // release
                    senseManager.ReleaseFrame();
                    return WBmp;
                    //break;
                }
            }

            ////should be used if we want to shut down the cam .
            //senseManager.Dispose();

            return null;
        }

        /// <summary>
        /// take a single depth color short array shot
        /// </summary>
        /// <returns></returns>
        public short[] DepthshortArray()
        {
            short[] depthPixel = new short[1];

            for (; ; )
            {
                if (senseManager.AcquireFrame(true).IsError())
                {
                    throw new Exception("Failed to acquire frame");
                }
                else
                {
                    // capture
                    PXCMCapture.Sample sample = senseManager.QuerySample();
                    PXCMImage depth = sample.depth;

                    //test
                    PXCMImage.ImageData DepthData;

                    depth.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_DEPTH_RAW, out DepthData);
                    int dwith = depth.info.width;
                    int dheight = depth.info.height;
                    depthPixel = DepthData.ToShortArray(0, dwith * dheight);

                    depth.Dispose();

                    // release
                    senseManager.ReleaseFrame();
                    break;
                }
            }

            //should be used if we want to shut down the cam .
            //senseManager.Dispose();
            return depthPixel;
        }

        /// <summary>
        /// take a single depth color int array shot
        /// </summary>
        /// <returns></returns>
        public int[] DepthintArray()
        {
            //short[] depthPixel = new short[1];

            for (; ; )
            {
                if (senseManager.AcquireFrame(true).IsError())
                {
                    throw new Exception("Failed to acquire frame");
                }
                else
                {
                    // capture
                    PXCMCapture.Sample sample = senseManager.QuerySample();
                    PXCMImage depth = sample.depth;

                    //test
                    PXCMImage.ImageData DepthData;

                    depth.AcquireAccess(PXCMImage.Access.ACCESS_READ, PXCMImage.PixelFormat.PIXEL_FORMAT_DEPTH_F32, out DepthData);
                    int dwith = depth.info.width;
                    int dheight = depth.info.height;
                    int[] depthPixel = DepthData.ToIntArray(0, dwith * dheight);

                    depth.Dispose();

                    // release
                    senseManager.ReleaseFrame();
                    return depthPixel;
                    //break;
                }
            }

            //should be used if we want to shut down the cam .
            //senseManager.Dispose();
            return null;
        }
    }
}