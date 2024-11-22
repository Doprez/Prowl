// This file is part of the Prowl Game Engine
// Licensed under the MIT License. See the LICENSE file in the project root for details.

using Veldrid;

namespace Prowl.Runtime.Rendering;

public interface IGeometryDrawData
{
    public void SetDrawData(CommandList commandList, ShaderPipeline pipeline);

    public int IndexCount { get; }

    public IndexFormat IndexFormat { get; }

    public PrimitiveTopology Topology { get; }
}
