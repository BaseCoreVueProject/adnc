# ADNC Quick Docker Deployment Guide

[GitHub Repository](https://github.com/alphayu/adnc)

## 1. System Requirements

1. Recommended OS: `Ubuntu 22.04`
2. `Docker` and `Docker Compose` must be pre-installed.

## 2. Create Deployment Directory

```bash
mkdir -p /opt/adnc/src
```

## 3. Create Custom Docker Network

```bash
docker network create \
  --driver=bridge \
  --subnet=172.80.0.0/16 \
  --ip-range=172.80.5.0/24 \
  --gateway=172.80.5.254 \
  adnc_network_main
```

## 4. Upload Middleware Deployment Files

Upload the `adnc\doc\devops-staging` folder from your local machine to the `/opt/adnc` directory on the server.

The server directory structure should look like this:

```bash
opt
└── adnc
    ├── devops-staging
    └── src
```

## 5. Start Middleware Containers

```bash
cd /opt/adnc/devops-staging
docker compose up -d
```

The `docker-compose.yml` file performs the following:

- Deploys a `Consul` cluster and initializes configuration.
- Deploys `MariaDB` and initializes the database.
- Deploys `Redis`.
- Deploys `RabbitMQ`.
- Deploys `Grafana` and `Loki`.
- Deploys `Nginx`.

Check container status after deployment:

```bash
docker container ls
```

## 6. Install .NET 8 SDK

```bash
apt-get update && \
apt-get install -y dotnet-sdk-8.0
```

## 7. Upload Microservices Code

> Before uploading, run `Delete-BIN-OBJ-Folders.bat` locally to clean all `bin` and `obj` directories.

Upload the following folders and files to the `/opt/adnc/src` directory on the server:

- `Demo` directory
- `Gateways` directory
- `Directory.Build.props`
- `Directory.Packages.props`
- `deploy_demo.sh`
- `deploy_ocelot.sh`

The server directory structure should look like this:

```bash
adnc
├── src
│   ├── Demo
│   ├── Gateways
│   ├── deploy_demo.sh
│   ├── deploy_ocelot.sh
│   ├── Directory.Packages.props
│   └── Directory.Build.props
└── devops-staging
```

## 8. Execute Deployment Scripts

```bash
cd /opt/adnc/src
chmod +x deploy_demo.sh deploy_ocelot.sh
bash deploy_demo.sh
bash deploy_ocelot.sh
```

## 9. Verify Gateway and Microservices

- Visit `http://{SERVER_IP}:8590` to open the Consul UI and check if `admin`, `maint`, and `cust` services are registered successfully.
- Visit `http://{SERVER_IP}:5000` to verify if the gateway is working.

## 10. Deploy Frontend

```bash
pnpm run build
```

- After a successful `build`, upload the files from the `dist` directory to `/opt/adnc/devops-staging/adnc-nginx/html`.
- Access `http://{SERVER_IP}` and log in to verify the system deployment.

------

## 11. Conclusion

The `ADNC` deployment is now complete.
