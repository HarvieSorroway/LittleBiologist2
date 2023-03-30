using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace LittleBiologist
{
    public class RenderScreenBlur : MonoBehaviour
    {
        public static Shader gaussianBlurShader;
        public static bool supported;
        public Material mat;

        public static RenderTexture renderTexture;

        public int DownSampleNum = 2;
        public float BlurSpreadSize = 1.5f;
        public int BlurIterations = 2;
        
        public void Start()
        {
            try
            {
                mat = new Material(gaussianBlurShader);
                supported = true;
            }
            catch(Exception e)
            {
                Debug.LogException(e);
                supported = false;
            }
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            if (supported)
            {
                if (renderTexture == null || renderTexture.width != src.width || renderTexture.height != src.height)
                {
                    if (renderTexture != null) DestroyImmediate(renderTexture);
                    renderTexture = new RenderTexture(src.width, src.height, 0);
                    renderTexture.hideFlags = HideFlags.HideAndDontSave;

                    Shader.SetGlobalTexture("_LBioGaussainBlurTexture", renderTexture);
                }

                //根据向下采样的次数确定宽度系数。用于控制降采样后相邻像素的间隔
                float widthMod = 1.0f / (1.0f * (1 << DownSampleNum));
                //Shader的降采样参数赋值
                mat.SetFloat("_DownSampleValue", BlurSpreadSize * widthMod);
                //设置渲染模式：双线性
                //通过右移，准备长、宽参数值
                int renderWidth = src.width >> DownSampleNum;
                int renderHeight = src.height >> DownSampleNum;

                RenderTexture renderBuffer = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, src.format);
                //拷贝sourceTexture中的渲染数据到renderBuffer,并仅绘制指定的pass0的纹理数据
                Graphics.Blit(src, renderBuffer, mat, 0);

                //根据BlurIterations（迭代次数），来进行指定次数的迭代操作
                for (int i = 0; i < BlurIterations; i++)
                {
                    //【2.1】Shader参数赋值
                    //迭代偏移量参数
                    float iterationOffs = (i * 1.0f);
                    //Shader的降采样参数赋值
                    mat.SetFloat("_DownSampleValue", BlurSpreadSize * widthMod + iterationOffs);

                    // 【2.2】处理Shader的通道1，垂直方向模糊处理 || Pass1,for vertical blur
                    // 定义一个临时渲染的缓存tempBuffer
                    RenderTexture tempBuffer = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, src.format);
                    // 拷贝renderBuffer中的渲染数据到tempBuffer,并仅绘制指定的pass1的纹理数据
                    Graphics.Blit(renderBuffer, tempBuffer, mat, 1);
                    //  清空renderBuffer
                    RenderTexture.ReleaseTemporary(renderBuffer);
                    // 将tempBuffer赋给renderBuffer，此时renderBuffer里面pass0和pass1的数据已经准备好
                    renderBuffer = tempBuffer;

                    // 【2.3】处理Shader的通道2，竖直方向模糊处理 || Pass2,for horizontal blur
                    // 获取临时渲染纹理
                    tempBuffer = RenderTexture.GetTemporary(renderWidth, renderHeight, 0, src.format);
                    // 拷贝renderBuffer中的渲染数据到tempBuffer,并仅绘制指定的pass2的纹理数据
                    Graphics.Blit(renderBuffer, tempBuffer, mat, 2);

                    //【2.4】得到pass0、pass1和pass2的数据都已经准备好的renderBuffer
                    // 再次清空renderBuffer
                    RenderTexture.ReleaseTemporary(renderBuffer);
                    // 再次将tempBuffer赋给renderBuffer，此时renderBuffer里面pass0、pass1和pass2的数据都已经准备好
                    renderBuffer = tempBuffer;
                }

                Graphics.Blit(renderBuffer, renderTexture);
                RenderTexture.ReleaseTemporary(renderBuffer);
            }
            Graphics.Blit(src, dest);
        }
    }
}
