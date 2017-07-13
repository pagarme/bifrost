#!/bin/bash

if [ -f "/etc/centos-release" ]; then
    yum install ca-certificates
elif [ -f "/etc/redhat-release" ]; then
    yum install -y nss-tools
elif [ -f "/etc/debian_version" ]; then
    apt-get update
    apt-get install libnss3-tools -y
elif [ -f "/etc/arch-release"]; then
    pacman -S --noconfirm --force nss
fi