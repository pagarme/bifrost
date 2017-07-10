#!/bin/bash

openssl pkcs12 -export -out $1/$2.pfx -inkey $1/$2.key -in $1/$2.crt -passin pass:"" -passout pass:""
