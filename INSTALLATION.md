# GruntiMaps Installation

This is the code for the GruntiMaps application. It combines the functions of a
MapBox vector tile (MVT) server with a GeoJSON to MVT converter and also a relatively simple
MapBox style generator. The importer adds the new layer to the map server, and also produces
offline map packs.

It is written in C# on ASP.NET Core. It runs in the default ASP.NET Kestrel web server, with
an Apache HTTPd reverse proxy in front for security/reliability.

The complete system can only run on platforms where the MapBox Tippecanoe program can be executed
- however the services that execute Tippecanoe and GDAL could potentially be extracted from the 
main code and run as separate processes on other hosts. 

## Setting up the VMs

GruntiMaps is designed to be able to run from multiple redundant servers. The setup script used to establish a multi-server Development environment in Azure is as follows:

```bash
#!/bin/bash
GROUPNAME=maps-dev
LOCATION=australiaeast
VNETNAME=$(GROUPNAME)-vnet
SUBNETNAME=$(GROUPNAME)-subnet
PUBLICIPNAME=$(GROUPNAME)-publicip
LBNAME=$(GROUPNAME)-load-balancer
FRONTENDIPNAME=$(GROUPNAME)-frontend
BACKENDIPNAME=$(GROUPNAME)-backend
HEALTHPROBE=$(GROUPNAME)-healthprobe
LBRULE=$(GROUPNAME)-rule-web
FRONTPORT=80
BACKPORT=80
SSHPORT=22
EXTERNSSHPORTPREFIX=422
NETSECGRP=$(GROUPNAME)-network-security-group
NICPREFIX=$(GROUPNAME)-nic
AVAILSETNAME=$(GROUPNAME)-availability-set
INSTANCES=3
VMPREFIX=$(GROUPNAME)-vm

# Create a resource group.
az group create \ 
    --name $(GROUPNAME) \
    --location $(LOCATION)

# Create a virtual network.
az network vnet create \
    --resource-group $(GROUPNAME) \
    --location $(LOCATION) \
    --name $(VNETNAME) \
    --subnet-name $(SUBNETNAME)

# Create a public IP address.
az network public-ip create \
    --resource-group $(GROUPNAME) \
    --name $(PUBLICIPNAME)

# Create an Azure Load Balancer.
az network lb create \
    --resource-group $(GROUPNAME) \
    --name $(LBNAME) \
    --public-ip-address $(PUBLICIPNAME) \
    --frontend-ip-name $(FRONTENDIPNAME) \
    --backend-pool-name $(BACKENDIPNAME)

# Creates an LB probe on port 80.
az network lb probe create \
    --resource-group $(GROUPNAME) \
    --lb-name $(LBNAME) \
    --name $(HEALTHPROBE) (HTTP:$(FRONTPORT)/Home) \
    --protocol tcp \
    --port $(FRONTPORT)

# Creates an LB rule for port 80.
az network lb rule create \
    --resource-group $(GROUPNAME) \
    --lb-name $(LBNAME) \
    --name $(LBRULE) \
    --protocol tcp \
    --frontend-port $(FRONTPORT) \
    --backend-port $(BACKPORT) \
    --frontend-ip-name $(FRONTENDIPNAME) \
    --backend-pool-name $(BACKENDIPNAME) \
    --probe-name $(HEALTHPROBE) (HTTP:$(FRONTPORT)/Home)

# Create three NAT rules for SSH.
for i in `seq 1 $(INSTANCES)`; do
  az network lb inbound-nat-rule create \
    --resource-group $(GROUPNAME) \
    --lb-name $(LBNAME) \
    --name $(LBNAME)-rule-ssh-$i \
    --protocol tcp \
    --frontend-port $(EXTERNSSHPORTPREFIX)$i \
    --backend-port $(SSHPORT) \
    --frontend-ip-name $(FRONTENDIPNAME)
done

# Create a network security group
az network nsg create \
    --resource-group $(GROUPNAME) \
    --name $(NETSECGRP)

# Create a network security group rule for SSH.
az network nsg rule create \
    --resource-group $(GROUPNAME) \
    --nsg-name $(NETSECGRP) \
    --name $(NETSECGRP)-rule-ssh \
    --protocol tcp \
    --direction inbound \
    --source-address-prefix '*' \
    --source-port-range '*'  \
    --destination-address-prefix '*' \
    --destination-port-range $(SSHPORT) \
    --access allow \
    --priority 1000

# Create a network security group rule for port 80.
az network nsg rule create \
    --resource-group $(GROUPNAME) 
    --nsg-name $(NETSECGRP) 
    --name $(NETSECGRP)-rule-http \
    --protocol tcp \
    --direction inbound \
    --priority 1001 \
    --source-address-prefix '*' \
    --source-port-range '*' \
    --destination-address-prefix '*' \
    --destination-port-range $(BACKPORT) \
    --access allow \
    --priority 2000

# Create three virtual network cards and associate with public IP address and NSG.
for i in `seq 1 $(INSTANCES)`; do
  az network nic create \
    --resource-group $(GROUPNAME) \
    --name $(NICPREFIX)$i \
    --vnet-name $(VNETNAME) \
    --subnet $(SUBNETNAME) \
    --network-security-group $(NETSECGRP) \
    --lb-name $(LBNAME) \
    --lb-address-pools $(BACKENDIPNAME) \
    --lb-inbound-nat-rules $(LBNAME)-rule-ssh-$i
done

# Create an availability set.
az vm availability-set create \
    --resource-group $(GROUPNAME) \
    --name $(AVAILSETNAME) \
    --platform-fault-domain-count $(INSTANCES) \
    --platform-update-domain-count $(INSTANCES)

# Create three virtual machines, this creates SSH keys if not present.
for i in `seq 1 $(INSTANCES)`; do
  az vm create \
    --resource-group $(GROUPNAME) \
    --name $(VMPREFIX)$i \
    --availability-set $(AVAILSETNAME) \
    --nics $(NICPREFIX)$i \
    --image UbuntuLTS \
    --generate-ssh-keys \
    --no-wait
done
```
Choose a user to own the app. For the purposes of this document we will call that user `gruntimaps`.

Of course, you can simply set up a single instance server instead.

The configuration below assumes Ubuntu 18.04 LTS
(i.e. it expects a `systemd`-based install). If you are using a non-`systemd` Linux you will
need to use a different mechanism to configure and manage the Apache HTTPd and ASP.NET Core
services.

### Requirements

After the VMs are installed you will need to install various packages and compile applications.
The instructions for non-distribution software installs have come from their respective
websites, and are presented here for convenience only - they may change over time.

### Install required packages to the VMs

You will need to install some extra packages as below.

Start by connecting to each server via SSH (the creation of the VMs should have prompted you
to create public/private keys, or use password authentication).

Each VM should be accessible from the public IP address created in the script, on ports 422`x` where `x` is the VM number.

```bash
ssh gruntimaps@<the public IP address> -p 422[123]
```

#### Apache HTTPd (used as a reverse proxy for ASP.NET Kestrel server)

Install Apache HTTPd and dependencies.

```bash
sudo apt-get install apache2 apache2-bin apache2-utils apache2-data ssl-cert libapr1 libaprutil1 libaprutil1-dbd-sqlite3 libaprutil1-ldap liblua5.2-0
```

#### Tippecanoe

Install C++ compiler and tools, then download
[Tippecanoe](https://www.github.com/mapbox/tippecanoe) from GitHub, compile, check and install.

```bash
sudo apt-get install build-essential libsqlite3-dev zlib1g-dev
git clone https://github.com/mapbox/tippecanoe.git
cd tippecanoe
make
make test
sudo make install
cd ..
```

#### GDAL

Install GDAL to each server. GDAL is used to convert spatial file formats (other than GeoJSON) into GeoJSON, so that they can be converted into MapBox formats by Tippecanoe.

If you want the most up-to-date GDAL you might want to consider doing this first:

```bash
sudo add-apt-repository ppa:ubuntugis/ppa
sudo apt-get update
```

Either way, install GDAL with:

```bash
sudo apt-get install gdal-bin
```

#### ASP.NET Core

Configure `apt` to retrieve the ASP.NET Core distribution and install (see [DotNet Core](https://www.microsoft.com/net/core#linuxubuntu) for more information if needed).

```bash
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.1
```

### Configure packages

You will need to create and/or edit some configuration files.

```bash
sudo vi /etc/apache2/sites-available/gruntimaps.conf
```

Enter the following into the `gruntimaps.conf` file.

```apache
ServerName gruntimaps.com
<VirtualHost *:80>
  RewriteEngine On
  RewriteCond %{HTTPS} !=on
  RewriteRule ^/?(.*) https://%{SERVER_NAME}/ [R,L]
</VirtualHost>

<VirtualHost *:443>
  ProxyPreserveHost On
  ProxyPass / http://127.0.0.1:5000/
  ProxyPassReverse / http://127.0.0.1:5000/
  ErrorLog ${APACHE_LOG_DIR}/maps-error.log
  CustomLog ${APACHE_LOG_DIR}/maps-access.log common
  SSLEngine on
  SSLProtocol all -SSLv2
  SSLCipherSuite ALL:!ADH:!EXPORT:!SSLv2:!RC4+RSA:+HIGH:+MEDIUM:!LOW:!RC4
  SSLCertificateFile /etc/ssl/certs/gruntimaps.pem
  SSLCertificateKeyFile /etc/ssl/private/private.pem
</VirtualHost>

<Location "/sprites">
        Header set Access-Control-Allow-Origin "*"
        SetOutputFilter DEFLATE
        SetEnvIfNoCase Request_URI "\.(?:gif|jpe?g|png)$" no-gzip
</Location>

<Location "/api">
  Header set Access-Control-Allow-Origin "*"
  SetOutputFilter DEFLATE
  SetEnvIfNoCase Request_URI "\.(?:gif|jpe?g|png)$" no-gzip
</Location>

<Location "/">
  Header append X-FRAME-OPTIONS "SAMEORIGIN"
  Header set X-Content-Type-Options "nosniff"
</Location>
```

The `/sprites` and `/api` entries allows the sprite data to be served to other sites by setting the access control allow origin header.

```bash
sudo vi /etc/systemd/system/gruntimaps.service
```

Enter the following into the gruntimaps.service file.

```INI
[Unit]
Description=GruntiMaps

[Service]
WorkingDirectory=/home/gruntimaps/gruntimaps.server
ExecStart=/usr/bin/dotnet /home/gruntimaps/gruntimaps.server/GruntiMaps.WebAPI.dll
Restart=always
RestartSec=10
SyslogIdentifier=gruntimaps
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

#### Enable Apache mods and the site

Even though we haven't compiled or deployed the application itself yet, nor started
the ASP.NET Core web server, we can configure and start the Apache reverse proxy
without problems, so we will do that.

Firstly we need to get the SSL certificate and key, and put them where Apache will
find them. The certificate should be in `/etc/ssl/certs/gruntimaps.pem` and
the private key should be in `/etc/ssl/private/private.pem`.

Once they are in place we can enable the necessary mods, enable the site we specified
earlier, and start the service.

```bash
sudo a2enmod proxy proxy_http proxy_html xml2enc rewrite socache_shmcb ssl headers
sudo a2dissite 000-default
sudo a2ensite gruntimaps
sudo systemctl restart apache2
sudo systemctl enable apache2
```

Check that the Apache service is running

```bash
sudo systemctl status apache2
```

The result should be something like this

```bash
● apache2.service - LSB: Apache2 web server
   Loaded: loaded (/etc/init.d/apache2; bad; vendor preset: enabled)
  Drop-In: /lib/systemd/system/apache2.service.d
           └─apache2-systemd.conf
   Active: active (running) since Sat 2017-06-24 06:05:28 UTC; 33s ago
     Docs: man:systemd-sysv-generator(8)
   CGroup: /system.slice/apache2.service
           ├─14939 /usr/sbin/apache2 -k start
           ├─14943 /usr/sbin/apache2 -k start
           └─14944 /usr/sbin/apache2 -k start
```

### Compile and publish the application

Retrieve the repository, publish the code, and deploy it.

```bash
git clone https://github.com/gispeople/gruntimaps.git
cd gruntimaps/GruntiMaps.WebAPI
dotnet restore
dotnet publish -c Release -o ~/gruntimaps.server
```
### Edit configuration file

In the `gruntimaps.server` directory, edit `appsettings.json` to specify the location for your data, etc.

### Install existing map data

If this map server will provide access to a full set of custom maps and their
corresponding offline map packs, you will need to obtain these files and install them.

Even if you are going to only serve newly-created layers, you will need to install font
glyphs, otherwise symbol layers that use text will not render anything.

If a full set of fonts is desired:

```bash
cd ~
git clone https://github.com/lukasmartinelli/glfonts.git
cp -r glfonts/fonts/* <wherever_you_configured_your_font_directory>
```

#### Enable the ASP.NET server

Enable the ASP.NET server, start it, and check that it's running.

```bash
sudo systemctl enable gruntimaps.service
sudo systemctl start gruntimaps.service
sudo systemctl status gruntimaps.service
```

At this point, everything should be running and you should be able to access the
server successfully via HTTPS (and attempting to access via HTTP should redirect to HTTPS).
