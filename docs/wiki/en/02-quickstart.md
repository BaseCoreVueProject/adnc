# ADNC Quick Start Guide

[GitHub repository](https://github.com/alphayu/adnc)

## 1. Modify Configuration Files

In the development environment, shared configuration for each `adnc` service is centralized in `adnc\src\Demo\Shared\resources\appsettings.shared.Development.json`, such as Redis and RabbitMQ settings.

Service-specific configuration is stored in each API project's `appsettings.Development.json`, such as database connection strings and service ports.

1. Redis configuration

```json
"Redis": {
    "Provider": "StackExchange",
    "EnableLogging": true,
    "SerializerName": "json",
    "EnableBloomFilter": false,
    "Dbconfig": {
        "ConnectionString": "server-ip:port,password=password,defaultDatabase=0,ssl=false,sslHost=null,connectTimeout=4000,allowAdmin=true"
    }
}
```

2. RabbitMQ configuration

```json
"RabbitMq": {
    "HostName": "server-ip",
    "Port": "port",
    "VirtualHost": "/",
    "UserName": "username",
    "Password": "password"
}
```

3. SysLogDb configuration

```json
"SysLogDb": {
    "DbType": "mysql",
    "ConnectionString": "Server=server-ip;Port=port;database=adnc_syslog;uid=username;pwd=password;connection timeout=30;"
}
```

4. MariaDB/MySQL configuration

```json
"MySQL": {
    "ConnectionString": "Server=server-ip;Port=port;database=adnc_admin;uid=username;pwd=password;connection timeout=30;"
}
```

## 2. Import Database Data

The database scripts for all services are stored in `adnc\database\mysql\adnc.sql`; you can import all data in one operation.

## 3. Start Backend Services

1. In `Visual Studio 2022`, right-click the solution, then choose **Properties** -> **Startup Project** -> **Multiple startup projects**, and select the following four projects:
   - `Adnc.Gateway.Ocelot`
   - `Adnc.Demo.Admin.Api`
   - `Adnc.Demo.Maint.Api`
   - `Adnc.Demo.Cust.Api`

**Tip**: During normal development, you do not need to start every service at the same time. This setup is only for a quick local experience.

2. In the main `Visual Studio 2022` window, click **Start** to start the three services and the gateway, four projects in total.

3. If startup fails, first check the error message in the **console window**. Common issues include:
   - **RabbitMQ port configuration error**: RabbitMQ exposes two ports, one for the web management page and one for data communication. Configure the data communication port in the configuration file.
   - **Service port conflict**: The configured port may already be used by another application.

| Project name | Description | URL |
| --- | --- | --- |
| Adnc.Gateway.Ocelot | Gateway | `http://localhost:5000` |
| Adnc.Demo.Admin.Api | System management | `http://localhost:50010` |
| Adnc.Demo.Maint.Api | Operations management | `http://localhost:50020` |
| Adnc.Demo.Cust.Api | Customer management | `http://localhost:50030` |

## 4. Start the Front End

1. Use `Visual Studio Code` to open the front-end project `adnc-vue-elementplus`. The front end is based on `Vue 3` and requires dependency installation.

2. Run the following commands to configure the environment:

```bash
# Install pnpm
npm install pnpm -g

# Optional: set a domestic mirror registry
pnpm config set registry https://registry.npmmirror.com

# Install dependencies
pnpm install

# Start the front-end project
pnpm run dev
```

## 5. Conclusion

At this point, `adnc` can run normally in your local environment.

----

If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
