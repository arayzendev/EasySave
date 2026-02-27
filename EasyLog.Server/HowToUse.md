# Comment déployer un conteneur Docker pour EasyLog.Server

## Build l'image et son conteneur du dockerfile

docker build -t easylog .

## Lancer le conteneur

docker run -d -p 5000:5000 -v ${PWD}/logs:/logs --name easylog easylog

## Accéder à l'interface de suivi

docker logs -f easylog

## Consulter les logs

docker exec -it easylog sh -c "cd /logs && exec sh"

cat logs_yyyy-mm-dd.json ou xml