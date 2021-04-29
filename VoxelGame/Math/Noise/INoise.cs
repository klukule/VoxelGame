﻿// SOURCE: https://github.com/ddevault/TrueCraft/blob/master/TrueCraft.API/World/INoise.cs

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VoxelGame.Noise
{
    public interface INoise
    {
        double Value2D(double x, double y);
        double Value3D(double x, double y, double z);
    }
}