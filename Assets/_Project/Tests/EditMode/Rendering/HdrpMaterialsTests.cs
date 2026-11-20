using System;
using NUnit.Framework;
using Stilargo.Editor.Rendering;
using UnityEngine;

namespace Stilargo.Tests.EditMode.Rendering
{
    public sealed class HdrpMaterialsTests
    {
        [Test]
        public void CreateLit_ImplausibleAlbedo_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => HdrpMaterials.CreateLit(Color.white));
        }

        [Test]
        public void CreateLit_PlausibleAlbedo_UsesHdrpLit()
        {
            Material material = HdrpMaterials.CreateLit(new Color(0.5f, 0.5f, 0.5f));
            try
            {
                Assert.AreEqual(HdrpMaterials.LitShaderName, material.shader.name);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(material);
            }
        }
    }
}
