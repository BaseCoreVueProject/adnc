﻿using Adnc.Shared.WebApi.Registrar;

namespace Adnc.Whse.WebApi.Registrar;

public sealed class WhseWebApiDependencyRegistrar : AbstractWebApiDependencyRegistrar
{
    public WhseWebApiDependencyRegistrar(IServiceCollection services)
        : base(services, typeof(WhseWebApiDependencyRegistrar).Assembly)
    {
    }

    public override void AddAdnc()
    {
        AddWebApiDefault();
        Services.AddGrpc();
    }
}