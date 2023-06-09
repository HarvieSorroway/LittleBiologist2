﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace LittleBiologist
{
    public class BlurCofSetup
    {
        static float[] GaussianMatrix = new float[49]{
                0.00000067f,  0.00002292f,  0.00019117f,  0.00038771f,  0.00019117f,  0.00002292f,  0.00000067f,
                0.00002292f,  0.00078634f,  0.00655965f,  0.01330373f,  0.00655965f,  0.00078633f,  0.00002292f,
                0.00019117f,  0.00655965f,  0.05472157f,  0.11098164f,  0.05472157f,  0.00655965f,  0.00019117f,
                0.00038771f,  0.01330373f,  0.11098164f,  0.22508352f,  0.11098164f,  0.01330373f,  0.00038771f,
                0.00019117f,  0.00655965f,  0.05472157f,  0.11098164f,  0.05472157f,  0.00655965f,  0.00019117f,
                0.00002292f,  0.00078633f,  0.00655965f,  0.01330373f,  0.00655965f,  0.00078633f,  0.00002292f,
                0.00000067f,  0.00002292f,  0.00019117f,  0.00038771f,  0.00019117f,  0.00002292f,  0.00000067f
        };

        public static void SetUp()
        {
            CalculateGaussianMatrix(8f);
            Shader.SetGlobalFloatArray("_LBioBlurWeight", GaussianMatrix);
            Shader.SetGlobalFloat("_LBioBlurAmt", 1.4f);
            Shader.SetGlobalFloat("_LBioTintAmt", 0.1f);
            Shader.SetGlobalColor("_LBioTintColor", Color.white);
        }

        static void CalculateGaussianMatrix(float d)
        {
            int x = 0;
            int y = 0;

            float sum = 0.0f;
            for (x = -3; x <= 3; ++x)
            {
                for (y = -3; y <= 3; ++y)
                {
                    GaussianMatrix[y * 7 + x + 24] = Mathf.Exp(-(x * x + y * y) / (2.0f * d * d)) / (2.0f * Mathf.PI * d * d);
                    sum += GaussianMatrix[y * 7 + x + 24];
                }
            }

            //normalize
            sum = 1.0f / sum;
            for (int i = 0; i < GaussianMatrix.Length; i++)
            {
                GaussianMatrix[i] *= sum;
            }
        }
    }
}
