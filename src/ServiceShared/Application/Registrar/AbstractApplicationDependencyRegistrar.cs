﻿using Adnc.Shared.Application.Services.Trackers;
using Adnc.Shared.Remote;

namespace Adnc.Shared.Application.Registrar;

//public abstract partial class AbstractApplicationDependencyRegistrar : IDependencyRegistrar
public abstract partial class AbstractApplicationDependencyRegistrar
{
    public string Name => "application";
    public abstract Assembly ApplicationLayerAssembly { get; }
    public abstract Assembly ContractsLayerAssembly { get; }
    public abstract Assembly RepositoryOrDomainLayerAssembly { get; }
    protected SkyApmExtensions SkyApm { get; init; }
    protected RpcInfo? RpcInfoOption { get; init; }
    protected IServiceCollection Services { get; init; }
    protected IConfiguration Configuration { get; init; }
    protected IServiceInfo ServiceInfo { get; init; }
    protected IConfigurationSection RedisSection { get; init; }
    protected IConfigurationSection CachingSection { get; init; }
    protected IConfigurationSection MysqlSection { get; init; }
    protected IConfigurationSection MongoDbSection { get; init; }
    protected IConfigurationSection ConsulSection { get; init; }
    protected IConfigurationSection RabbitMqSection { get; init; }
    protected bool PollyStrategyEnable { get; init; }

    public AbstractApplicationDependencyRegistrar(IServiceCollection services, IServiceInfo serviceInfo)
    {
        Services = services ?? throw new ArgumentException("IServiceCollection is null.");
        ServiceInfo = serviceInfo ?? throw new ArgumentException("ServiceInfo is null.");
        Configuration = services.GetConfiguration() ?? throw new ArgumentException("Configuration is null.");
        RedisSection = Configuration.GetSection(NodeConsts.Redis);
        CachingSection = Configuration.GetSection(NodeConsts.Caching);
        MongoDbSection = Configuration.GetSection(NodeConsts.MongoDb);
        MysqlSection = Configuration.GetSection(NodeConsts.Mysql);
        ConsulSection = Configuration.GetSection(NodeConsts.Consul);
        RabbitMqSection = Configuration.GetSection(NodeConsts.RabbitMq);
        SkyApm = Services.AddSkyApmExtensions();
        RpcInfoOption = Configuration.GetSection(NodeConsts.RpcInfo).Get<RpcInfo>();
        PollyStrategyEnable = Configuration.GetValue("Polly:Enable", false);
    }

    /// <summary>
    /// 注册所有服务
    /// </summary>
    public abstract void AddApplicationServices();

    /// <summary>
    /// 注册adnc.application通用服务
    /// </summary>
    protected virtual void AddApplicaitonDefault()
    {
        Services
            .AddSingleton(typeof(Lazy<>))
            .AddScoped<UserContext>()
            .AddScoped<IMessageTracker, DbMessageTrackerService>()
            .AddScoped<IMessageTracker, RedisMessageTrackerService>()
            .AddScoped<MessageTrackerFactory>()
            .AddHostedService<Channels.LogConsumersHostedService>()
            .AddValidatorsFromAssembly(ContractsLayerAssembly, ServiceLifetime.Scoped)
            .AddAdncInfraAutoMapper(ApplicationLayerAssembly)
            .AddAdncInfraYitterIdGenerater(RedisSection, ServiceInfo.ShortName.Split('-')[0])
            .AddAdncInfraConsul(ConsulSection)
            .AddAdncInfraDapper();

        AddAppliactionSerivcesWithInterceptors();
        AddEfCoreContextWithRepositories();
        AddRedisCaching();
        AddBloomFilters();
    }
}