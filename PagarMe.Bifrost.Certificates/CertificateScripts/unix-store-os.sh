#!/bin/bash

if [ -f "/etc/centos-release" ]; then
    update-ca-trust force-enable
    update-ca-trust extract
else
    certutil -d sql:$HOME/.pki/nssdb -D -n Bifrost 2> /dev/null
    certutil -d sql:$HOME/.pki/nssdb -A -t "P,," -n Bifrost -i $1/$2.crt
fi