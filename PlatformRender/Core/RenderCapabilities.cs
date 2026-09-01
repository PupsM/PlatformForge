namespace PlatformRender.Core;

/// <summary>
/// Информация о возможностях GPU
/// </summary>
public sealed class RenderCapabilities
{
    public string Renderer { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string GLSLVersion { get; set; } = string.Empty;

    public int MaxTextureSize { get; set; }
    public int MaxVertexAttributes { get; set; }
    public int MaxUniformBufferSize { get; set; }
    public int MaxShaderStorageBufferSize { get; set; }

    public bool SupportsShaderStorageBuffer { get; set; }
    public bool SupportsComputeShaders { get; set; }
    public bool SupportsGeometryShaders { get; set; }
    public bool SupportsTessellation { get; set; }
    public bool SupportsMultiDrawIndirect { get; set; }

    public override string ToString()
    {
        return $@"=== Render Capabilities ===
            Renderer: {Renderer}
            Vendor: {Vendor}
            Version: {Version}
            GLSL Version: {GLSLVersion}
            Max Texture Size: {MaxTextureSize}
            Max Vertex Attributes: {MaxVertexAttributes}
            Max Uniform Buffer Size: {MaxUniformBufferSize} bytes
            Max Shader Storage Buffer Size: {MaxShaderStorageBufferSize} bytes
            Extensions:
            SSBO: {SupportsShaderStorageBuffer}
            Compute: {SupportsComputeShaders}
            Geometry: {SupportsGeometryShaders}
            Tessellation: {SupportsTessellation}
            MultiDrawIndirect: {SupportsMultiDrawIndirect}";
    }
}