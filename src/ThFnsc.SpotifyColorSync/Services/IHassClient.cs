﻿
using HADotNet.Core.Clients;

namespace ThFnsc.SpotifyColorSync.Services;

public interface IHassClient
{
    public EntityClient Entity { get; }
    public StatesClient States { get; }
    public ServiceClient Service { get; }
    public DiscoveryClient Discovery { get; }
    Task<Stream> GetImageAsync(string url);
}