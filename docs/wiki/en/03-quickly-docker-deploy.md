# ADNC Quick Docker Deployment Guide

[GitHub repository](https://github.com/alphayu/adnc)

## 1. System Requirements

1. Recommended server operating system: `Ubuntu 22.04`
2. `Docker` and `Docker Compose` must be installed on the server.

## 2. Create the Deployment Directory

```bash
mkdir -p /opt/adnc/src
```

## 3. Create a Custom Docker Network

```bash
docker network create \
  --driver=bridge \
  --subnet=172.80.0.0/16 \
  --ip-range=172.80.5.0/24 \
  --gateway=172.80.5.254 \
  adnc_network_main
```

## 4. Upload the Middleware Deployment YAML Files

Upload the local `adnc\deploy\staging` folder to `/opt/adnc` on the server.

After upload, the server directory structure should be:

```bash
opt
└── adnc
    ├── staging
    └── src
```

## 5. Start Middleware Containers

```bash
cd /opt/adnc/staging
docker compose up -d
```

The `docker-compose.yml` file performs the following tasks:

- Deploys the `Consul` cluster and initializes configuration.
- Deploys `MariaDB` and initializes the database.
- Deploys `Redis`.
- Deploys `RabbitMQ`.
- Deploys `Grafana` and `Loki`.
- Deploys `Nginx`.

After deployment, check container status with:

```bash
docker container ls
```

## 6. Install the .NET 8 SDK

```bash
apt-get update && \
apt-get install -y dotnet-sdk-8.0
```

## 7. Upload Microservice Code

> Before uploading, run `Delete-BIN-OBJ-Folders.bat` locally to clean all `bin` and `obj` directories.

Upload the following folders and files to `/opt/adnc/src` on the server:

- `Demo` directory
- `Gateways` directory
- `Directory.Build.props`
- `Directory.Packages.props`
- `deploy_demo.sh`
- `deploy_ocelot.sh`

After upload, the server directory structure should be:

```bash
adnc
├── src
│   ├── Demo
│   ├── Gateways
│   ├── deploy_demo.sh
│   ├── deploy_ocelot.sh
│   ├── Directory.Packages.props
│   └── Directory.Build.props
└── staging
```

## 8. Run Microservice Deployment Scripts

```bash
cd /opt/adnc/src
chmod +x deploy_demo.sh deploy_ocelot.sh
bash deploy_demo.sh
bash deploy_ocelot.sh
```

## 9. Verify the Gateway and Microservices

- Visit `http://{server-ip}:8590`, open the Consul UI, and check whether the `admin`, `maint`, and `cust` services are registered successfully.
- Visit `http://{server-ip}:5000` to check whether the gateway is working properly.

## 10. Deploy the Front End

```bash
pnpm run build
```

- After `build` succeeds, upload the files in the `dist` directory to `/opt/adnc/staging/adnc-nginx/html`.
- Visit `http://{server-ip}` and log in to check whether the system was deployed successfully.

## 11. Conclusion

At this point, `ADNC` has been deployed.

------
If you can help, welcome [star & fork](https://github.com/alphayu/adnc).
