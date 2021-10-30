using UnityEngine;
using UnityEngine.Rendering;

namespace TerraLand
{
    public static class MaterialManager
    {
        public enum PipelineType
        {
            Unsupported,
            BuiltInPipeline,
            UniversalPipeline,
            HDPipeline
        }

        private static Material _terrainMaterial = null;

        /// <summary>
        /// Returns the type of renderpipeline that is currently running
        /// </summary>
        /// <returns></returns>
        private static PipelineType DetectPipeline()
        {
#if UNITY_2019_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                // SRP
                string srpType = GraphicsSettings.renderPipelineAsset.GetType().ToString();

                if (srpType.Contains("HDRenderPipelineAsset"))
                    return PipelineType.HDPipeline;
                else if (srpType.Contains("UniversalRenderPipelineAsset") || srpType.Contains("LightweightRenderPipelineAsset"))
                    return PipelineType.UniversalPipeline;
                else
                    return PipelineType.Unsupported;
            }
#elif UNITY_2017_1_OR_NEWER
            if (GraphicsSettings.renderPipelineAsset != null)
            {
                // SRP not supported before 2019
                return PipelineType.Unsupported;
            }
#endif
            // no SRP
            return PipelineType.BuiltInPipeline;
        }

        public static Material GetTerrainMaterial()
        {
            if (_terrainMaterial == null)
            {
                PipelineType pipelineType = DetectPipeline();

                if (pipelineType == PipelineType.HDPipeline)
                    _terrainMaterial = new Material(Shader.Find("HDRP/TerrainLit"));
                else if (pipelineType == PipelineType.UniversalPipeline)
                    _terrainMaterial = new Material(Shader.Find("Universal Render Pipeline/Terrain/Lit"));
                else if (pipelineType == PipelineType.BuiltInPipeline || pipelineType == PipelineType.Unsupported)
                    _terrainMaterial = new Material(Shader.Find("Nature/Terrain/Standard"));
                else
                    _terrainMaterial = new Material(Shader.Find("Nature/Terrain/Standard"));
            }
        
            return _terrainMaterial;
        }
    }
}

