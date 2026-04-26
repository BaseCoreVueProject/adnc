# ADNC Quick Start Guide

[GitHub Repository](https://github.com/alphayu/adnc)

## 1. Configuration Changes

In the development environment, shared configurations for all `adnc` services are centralized in `adnc\src\Demo\Shared\resources\appsettings.shared.Development.json` (e.g., Redis, RabbitMQ, etc.).

Service-specific configurations are located in the `appsettings.Development.json` file of the corresponding API project (e.g., database connection strings, service ports, etc.).

### 1.1 Redis Configuration

```json
"Redis": {
    "Provider": "StackExchange",
    "EnableLogging": true,
    "SerializerName": "json",
    "EnableBloomFilter": false,
    "Dbconfig": {
        "ConnectionString": "SERVER_IP:PORT,password=PASSWORD,defaultDatabase=0,ssl=false,sslHost=null,connectTimeout=4000,allowAdmin=true"
    }
}
```

### 1.2 RabbitMQ Configuration

```json
"RabbitMq": {
    "HostName": "SERVER_IP",
    "Port": "PORT",
    "VirtualHost": "/",
    "UserName": "USERNAME",
    "Password": "PASSWORD"
}
```

### 1.3 SysLogDb Configuration

```json
"SysLogDb": {
    "DbType": "mysql",
    "ConnectionString": "Server=SERVER_IP;Port=PORT;database=adnc_syslog;uid=USERNAME;pwd=PASSWORD;connection timeout=30;"
}
```

### 1.4 MariaDB/MySQL Configuration

```json
"Mysql": {
    "ConnectionString": "Server=SERVER_IP;Port=PORT;database=adnc_admin;uid=USERNAME;pwd=PASSWORD;connection timeout=30;"
}
```

## 2. Import Database Data

Database scripts for all services are stored in `adnc\doc\dbsql\adnc.sql`, which can be imported all at once.

## 3. Start Backend Services

1. In `Visual Studio 2022`, right-click the solution → **Properties** → **Startup Project** → **Multiple startup projects**. Check the following 4 projects:

   - `Adnc.Gateway.Ocelot`
   - `Adnc.Demo.Admin.Api`
   - `Adnc.Demo.Maint.Api`
   - `Adnc.Demo.Cust.Api`

   **Note**: You don't need to start all services during actual development; this is just for a quick local experience.

2. In the `Visual Studio 2022` main interface, click the **Start** button to launch the 3 services and the gateway (4 projects in total).

3. If startup errors occur, check the **Console Window** for error messages. Common issues include:

   - **RabbitMQ Port Configuration**: RabbitMQ exposes two ports—one for the Web Management UI and one for data communication. Use the data port in the configuration.
   - **Port Conflicts**: Conflicts with applications like WeChat Work or other services.

| Project Name         | Description           | URL                      |
| -------------------- | --------------------- | ------------------------ |
| Adnc.Gateway.Ocelot | Gateway               | `http://localhost:5000`  |
| Adnc.Demo.Admin.Api | System Management     | `http://localhost:50010` |
| Adnc.Demo.Maint.Api | Operations Management | `http://localhost:50020` |
| Adnc.Demo.Cust.Api  | Customer Management   | `http://localhost:50030` |

## 4. Start Frontend

1. Open the frontend project `adnc-vue-elementplus` in `Visual Studio Code`. The frontend is built with `Vue 3` and requires dependency installation.
2. Run the following commands for environment setup:

```bash
# Install pnpm
npm install pnpm -g

# Optional: Set mirror registry
pnpm config set registry https://registry.npmmirror.com

# Install dependencies
pnpm install

# Start the frontend project
pnpm run dev
```

## 5. Conclusion

`adnc` should now be running locally.

If this project helps you, please `Star` & `Fork` to support us!

[ADNC](https://aspdotnetcore.net/) — A practical .NET microservices/distributed development framework.
