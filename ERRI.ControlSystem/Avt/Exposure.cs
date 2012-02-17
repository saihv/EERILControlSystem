﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EERIL.ControlSystem.Avt
{
    public enum ExposureAlgorithm : uint
    {
        Mean = 0,
        FitRange = 1
    };

    public enum ExposureMode : uint
    {
        Manual = 0,
        Auto = 1,
        AutoOnce = 2,
        External = 3
    };

    public class Exposure
    {
        public ExposureAlgorithm algorithm;
        public ExposureMode mode;
        public uint tolerance;
        public uint max;
        public uint min;
        public uint outliers;
        public uint rate;
        public uint target;
        public uint value;

        public Exposure() // TODO: Need to change default values to match camera defaults
        {
            algorithm = ExposureAlgorithm.Mean;
            mode = ExposureMode.Manual;
            tolerance = 0;
            max = 0;
            min = 0;
            outliers = 0;
            rate = 0;
            target = 0;
            value = 0;
        }
    }
}
