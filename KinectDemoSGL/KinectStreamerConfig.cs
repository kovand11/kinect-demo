﻿namespace KinectDemoSGL
{
    class KinectStreamerConfig
    {
        public bool ProvideDepthData { get; set; }
        public bool ProvideBodyData { get; set; }
        public bool ProvideColorData { get; set; }

        public KinectStreamerConfig()
        {
            ProvideDepthData = false;

            ProvideColorData = false;

            ProvideBodyData = false;
        }

    }
}
