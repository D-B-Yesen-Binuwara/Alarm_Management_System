.PHONY: up down rebuild seed demo

up:
	docker compose up -d

down:
	docker compose down

rebuild:
	docker compose up --build -d

seed:
	docker cp Backend/DB_Schema.sql inms-db:/tmp/DB_Schema.sql
	docker exec -i inms-db /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "Iac@4336" -C -i /tmp/DB_Schema.sql

demo:
	BASE_URL=http://localhost:5289 DB_CONTAINER=inms-db SQL_PASSWORD='Iac@4336' ./scripts/run-impact-tests.sh
