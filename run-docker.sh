#!/bin/sh

chmod +x *.sh
docker build -t streamfox .
docker run --name streamfox -p 0.0.0.0:80:5000 -itd streamfox
