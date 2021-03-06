﻿using VisualPinball.Engine.Test.Test;
using VisualPinball.Engine.VPT.Rubber;
using Xunit;

namespace VisualPinball.Engine.Test.VPT.Rubber
{
	public class RubberDataTest
	{
		[Fact]
		public void ShouldReadRubberData()
		{
			var table = Engine.VPT.Table.Table.Load(VpxPath.Rubber);
			ValidateRubberData(table.Rubbers["Rubber1"].Data);
		}

		[Fact]
		public void ShouldWriteRubberData()
		{
			const string tmpFileName = "ShouldWriteRubberData.vpx";
			var table = Engine.VPT.Table.Table.Load(VpxPath.Rubber);
			table.Save(tmpFileName);
			var writtenTable = Engine.VPT.Table.Table.Load(tmpFileName);
			ValidateRubberData(writtenTable.Rubbers["Rubber1"].Data);
		}

		private static void ValidateRubberData(RubberData data)
		{
			Assert.Equal(3, data.DragPoints.Length);
			Assert.Equal(0.832f, data.Elasticity);
			Assert.Equal(0.321f, data.ElasticityFalloff);
			Assert.Equal(0.685f, data.Friction);
			Assert.Equal(25.556f, data.Height);
			Assert.Equal(false, data.HitEvent);
			Assert.Equal(25.193f, data.HitHeight);
			Assert.Equal("test_pattern", data.Image);
			Assert.Equal(true, data.IsCollidable);
			Assert.Equal(true, data.IsReflectionEnabled);
			Assert.Equal(true, data.IsVisible);
			Assert.Equal("Playfield", data.Material);
			Assert.Equal(true, data.OverwritePhysics);
			Assert.Equal("", data.PhysicsMaterial);
			Assert.Equal(65.23f, data.RotX);
			Assert.Equal(75.273f, data.RotY);
			Assert.Equal(70.962f, data.RotZ);
			Assert.Equal(5.225f, data.Scatter);
			Assert.Equal(false, data.ShowInEditor);
			Assert.Equal(true, data.StaticRendering);
			Assert.Equal(12, data.Thickness);
		}
	}
}
