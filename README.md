# ResponseRequest
#para criar um container de rabbit use 
docker run -d --hostname rabbit-host --name rabbit-local -p 8089:15672 -p 5672:5672 --network bridge rabbitmq:3-management