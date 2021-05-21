# GRPCGreeterService

# LINK OF MICROSOFT DOCS TO CREATE CERTIFICATE SELF SICN BOTH ENVIROMENT
https://docs.microsoft.com/pt-br/dotnet/core/additional-tools/self-signed-certificates-guide#with-openssl

PARENT="contoso.com"
openssl req \
-x509 \
-newkey rsa:4096 \
-sha256 \
-days 365 \
-nodes \
-keyout $PARENT.key \
-out $PARENT.crt \
-subj "/CN=${PARENT}" \
-extensions v3_ca \
-extensions v3_req \
-config <( \
  echo '[req]'; \
  echo 'default_bits= 4096'; \
  echo 'distinguished_name=req'; \
  echo 'x509_extension = v3_ca'; \
  echo 'req_extensions = v3_req'; \
  echo '[v3_req]'; \
  echo 'basicConstraints = CA:FALSE'; \
  echo 'keyUsage = nonRepudiation, digitalSignature, keyEncipherment'; \
  echo 'subjectAltName = @alt_names'; \
  echo '[ alt_names ]'; \
  echo "DNS.1 = www.${PARENT}"; \
  echo "DNS.2 = ${PARENT}"; \
  echo '[ v3_ca ]'; \
  echo 'subjectKeyIdentifier=hash'; \
  echo 'authorityKeyIdentifier=keyid:always,issuer'; \
  echo 'basicConstraints = critical, CA:TRUE, pathlen:0'; \
  echo 'keyUsage = critical, cRLSign, keyCertSign'; \
  echo 'extendedKeyUsage = serverAuth, clientAuth')

openssl x509 -noout -text -in $PARENT.crt



# COMMAND TO CREATE CONTAINER WITH CERTIFICATE SELF SICN
docker run --rm -it -p 8000:80 -p 8001:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.crt -e ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/https/localhost.key -v /mnt/c/Users/frmau/estudo/certificados:/https/ grpcproductsservice

docker run --rm -d -p 8000:80 -p 8001:443 -e ASPNETCORE_URLS="https://+;http://+" -e ASPNETCORE_HTTPS_PORT=8001 -e ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/localhost.crt -e ASPNETCORE_Kestrel__Certificates__Default__KeyPath=/https/localhost.key -v /mnt/c/Users/frmau/estudo/certificados:/https/ grpcproductsservice


# COMMAND MORE EFICIENTILY TO CREATE CONTAINER GRPC SERVER WITH INSECURITY CERTIFICATE 
docker run -p 127.0.0.1:8080:80 -p 127.0.0.1:8081:8080 --env ASPNETCORE_ENVIRONMENT=Development -d grpcproductsservice -e ASPNETCORE_ENVIRONMENT=Development -e ASPNETCORE_URLS=http://+




