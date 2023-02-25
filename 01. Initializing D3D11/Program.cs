
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Windowing;
using System.Runtime.CompilerServices;

float[] rgba = { 0, 100, 100, 0 };

/* Defining the variables that later will be initialized at OnLoad */
ComPtr<IDXGIFactory2> factory = default;
ComPtr<IDXGISwapChain1> swapchain = default;
ComPtr<ID3D11Device> device = default;
ComPtr<ID3D11DeviceContext> deviceContext = default;
ComPtr<ID3D11RenderTargetView> d3d11RenderTargetView = default;

/* Load the APIs for later use */
var dgxi = DXGI.GetApi();
var d3d11 = D3D11.GetApi();
var compilers = D3DCompiler.GetApi();

/* Initializing the window */
var options = WindowOptions.Default with { API = GraphicsAPI.None };
var window = Window.Create(options);

/* Assign the events */
window.Load += OnLoad;
window.Render += OnRender;

/* Show the window  */ 
window.Run();

/* Dispose the resources */
factory.Dispose();
swapchain.Dispose();
device.Dispose();
deviceContext.Dispose();
d3d11RenderTargetView.Dispose();

unsafe void OnLoad()
{
    /*
     * Create virtual device
     */
    SilkMarshal.ThrowHResult
    (
        d3d11.CreateDevice
        (
            pAdapter:           default(ComPtr<IDXGIAdapter>),
            DriverType:         D3DDriverType.Hardware,
            Software:           default,
            Flags:              (uint)(CreateDeviceFlag.Debug | CreateDeviceFlag.BgraSupport),
            pFeatureLevels:     default,
            FeatureLevels:      0,
            SDKVersion:         D3D11.SdkVersion,
            ppDevice:           ref device,
            pFeatureLevel:      default,
            ppImmediateContext: ref deviceContext
        )
    );

#if DEBUG
    /*
     * Set debug callback
     */
    device.SetInfoQueueCallback(message => Console.WriteLine(SilkMarshal.PtrToString((nint)message.PDescription)));
#endif

    /*
     * Create dgxi factory for later use
     */
    factory = dgxi.CreateDXGIFactory<IDXGIFactory2>();

    /*
     * The swap chain properties
     */
    var swapChainDescription = new SwapChainDesc1
    {
        BufferCount = 2,
        Format = Format.FormatB8G8R8A8Unorm,
        BufferUsage = DXGI.UsageRenderTargetOutput,
        SwapEffect = SwapEffect.Discard,
        SampleDesc = new SampleDesc(1, 0),
        Width = 0,
        Height = 0
    };

    /* Create the swap chain */
    SilkMarshal.ThrowHResult
    (
        factory.CreateSwapChainForHwnd
        (
            device,
            window.Native!.DXHandle!.Value,
            in swapChainDescription,
            null,
            ref Unsafe.NullRef<IDXGIOutput>(),
            ref swapchain
        )
    );

    /* Create the frame buffer */
    ComPtr<ID3D11Texture2D> d3d11FrameBuffer = default;
    fixed (Guid* guid = &ID3D11Texture2D.Guid)
    {
        SilkMarshal.ThrowHResult
        (
            swapchain.GetBuffer(0, guid, (void**)&d3d11FrameBuffer)
        );
    }

    /* Create the render target */
    SilkMarshal.ThrowHResult
    (
        device.CreateRenderTargetView(d3d11FrameBuffer, null, ref d3d11RenderTargetView)
    );

    /* Delete the frame buffer */
    d3d11FrameBuffer.Release();
}

/* Present the render */
unsafe void OnRender(double obj)
{
    Span<float> color = rgba;
    deviceContext.ClearRenderTargetView(d3d11RenderTargetView, color);
    swapchain.Present(1, 0);
}