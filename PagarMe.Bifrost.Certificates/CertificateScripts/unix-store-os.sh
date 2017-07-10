#!/bin/bash

certutil -d sql:$HOME/.pki/nssdb -D -n Bifrost 2> /dev/null
certutil -d sql:$HOME/.pki/nssdb -A -t "P,," -n Bifrost -i $1/$2.crt
