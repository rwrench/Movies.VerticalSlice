using Microsoft.JSInterop;
using Movies.VerticalSlice.Api.Blazor.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Movies.VerticalSlice.Api.Blazor.Client.Tests.TestHelpers;

public class TestJwtAuthenticationStateProvider : JwtAuthenticationStateProvider
{
    public string? LastToken { get; private set; }
    public bool SetTokenCalled { get; private set; }

    public TestJwtAuthenticationStateProvider(IJSRuntime js) : base(js) { }

    public override async Task SetTokenAsync(string token)
    {
        SetTokenCalled = true;
        LastToken = token;
        await base.SetTokenAsync(token);
    }
}
