#!/bin/bash

if [ $(find ~/.mozilla -name cert?.db | wc -l) -ge 1 ]; then
    certutil -d $(find ~/.mozilla/ -name cert?.db | sed s/cert[0-9].db//g | sort | uniq) -D -n $2 2> /dev/null
    certutil -d $(find ~/.mozilla/ -name cert?.db | sed s/cert[0-9].db//g | sort | uniq) -A -n $2 -t "$3" -i $1/$2.crt
fi
