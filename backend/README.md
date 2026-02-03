## MEMOTOPIA-API backend API

Made with Nest.js.
Deplyed on Linode - https://cloud.linode.com/

**Dev Stage**

Development stage contains backend and database in the same server, they run by docker compose.

**Prod Stage**

Production runs on two servers, one for backend and another for the database. Backend runs on docker, while databse is with docker-compose.


## Installation & running the app

Please double check the package.json and docker-compose before running localy.
```bash
$ yarn db:start ## Starts a docker container with a local database for testing, see docker-compose.yml for details
$ yarn
$ yarn start
```

Note: there are multiple ways of running this setup, see package.json scripts for details

## Migrations

Use those commands to handle migrations

```bash
yarn typeorm:cli migration:generate -n "NAME"
yarn typeorm:cli migration:run
```

```bash
yarn typeorm:cli migration:generate -n "NAME"
yarn typeorm:cli migration:create -N "NAME"
yarn typeorm:cli migration:run
yarn typeorm:cli migration:revert
```

## Nestjs model creation

```bash
nest generate module ./modules/user --no-spec
nest generate service ./modules/user --no-spec
nest generate controller ./modules/user --no-spec
```

## Before Commit
   run: yarn format


## SQL Scripts to run for adding games
In case you remove the db for any reason (i.e. in beta), the following queries need to be run first otherwise you will have errors:
``` bash
INSERT INTO public.game(
	id, "gameName")
	VALUES (1, 'Pairs');
	
INSERT INTO public.game(
	id, "gameName")
	VALUES (2, 'Boxes');
	
INSERT INTO public.game(
	id, "gameName")
	VALUES (3, 'Flash');
```


## Run localy
> yarn start

Than visit `http://localhost:3001/api/` where you can see the endpoints on swagger. You need to make sure to have the `.env.local` file with the needed variables. You can connect to db in dev or if you have localy.

## Deployment dev
- Need to login though terminal on the linode instance
- Pull the code from github
- Take care of migrations if any
- Run the following commands in order to have the latest changes on the backend code. In docker-compose is running the node backend and the posgress db.

```
$ docker-compose stop -t 1 nodeapp
$ docker-compose build nodeapp
$ docker-compose up --no-start nodeapp
$ docker-compose start nodeapp
```


## Deployment prod
- Need to login though terminal on the linode instance
- Pull the code from github
- Take care of migrations if any
- Code of backend is running on a docker container.

For the first time you can use the following:
```bash
$ docker build . -t memotopia
$ docker run -dp 80:3001 memotopia
```

In order to make the changes there are three steps:
1) Build the new code with a new tag
2) Stop and remove the old running docker
3) Run the new build

<br>
- Build the new code with a new tag

```
$ docker build . -t new-tag
```

<br>
- Stop and remove the old running docker

```
$ docker ps
$ docker stop the-container-id
$ docker rm the-container-id
```

<br>
- Run the new build

```
$ docker run -dp 80:3001 new-tag
```
