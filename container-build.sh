# Setup scripts
chmod +x *.sh

# Install required packages
apt update
apt install curl -y

curl -fsSL https://deb.nodesource.com/setup_14.x | bash -
curl https://packages.microsoft.com/config/ubuntu/21.04/packages-microsoft-prod.deb -o packages-microsoft-prod.deb
dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

apt update
apt install -y apt-transport-https nodejs dotnet-sdk-3.1 aspnetcore-runtime-3.1

apt install -y ffmpeg

# Build Server
mkdir /deploy

(cd /streamfox/Streamfox.Server && dotnet publish -c Release)
cp -r /streamfox/Streamfox.Server/bin/Release/netcoreapp3.1/publish/* /deploy

# Build Client
mkdir /deploy/client
cd /streamfox/client

npm i
npm run build

cp -r dist/* /deploy/client