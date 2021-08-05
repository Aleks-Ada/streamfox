# Streamfox
Streamfox video hosting website.

## Tutorials
Tutorials referenced in the demo video:

Services:
- DigitalOcean: https://www.digitalocean.com/community/tutorials
- Docker: https://docs.docker.com/get-started/

Development tools:
- ASP.NET Core tutorial: https://docs.microsoft.com/en-us/aspnet/core/?view=aspnetcore-5.0
- Ffmpeg documentation: https://www.ffmpeg.org/documentation.html 
- Vue.js tutorial: https://v3.vuejs.org/guide/introduction.html
- npm docs: https://docs.npmjs.com/about-npm

I also found this link during the video to install Docker on Ubuntu:
https://docs.docker.com/engine/install/ubuntu/

## Instructions to run
To run a production container on your machine, run one of the following commands depending on your operating system.

Before running the container, make sure that no other application is running on port 80.

After running the container, it will available to access on your machine over HTTP on any IP address at port 80.

Therefore, you can navigate to `http://localhost` in your browser to use the website.

### Windows
Open CMD in the base project directory and run
```cmd
run-docker.bat
```

### Linux
Open a terminal in the base project directory and run
```sh
sh run-docker.sh
```

or just
```sh
./run-docker.sh
```
