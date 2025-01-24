PROJ_NAME := GameCloud
API_PROJ := ./Presentation/GameCloud.WebAPI
INFRA_PROJ := ./Infrastructure/GameCloud.Persistence
K8S_NAMESPACE := gamecloud
POD_NAME := gamecloud-main-65cd865ff9-f9g6c

.PHONY: run run-debug run-release clean restore build test

run:
	@echo "Starting application in default mode..."
	dotnet run --project $(API_PROJ)

run-debug:
	@echo "Starting application in debug mode..."
	dotnet run --project $(API_PROJ) --configuration Debug

run-release:
	@echo "Starting application in release mode..."
	dotnet run --project $(API_PROJ) --configuration Release

.PHONY: migration remove-migration database-update database-drop

migration:
	@echo "Creating new migration: $(word 2,$(MAKECMDGOALS))"
	dotnet ef migrations add $(word 2,$(MAKECMDGOALS)) --project $(INFRA_PROJ) --startup-project $(API_PROJ)

remove-migration:
	@echo "Removing last migration..."
	dotnet ef migrations remove --project $(INFRA_PROJ) --startup-project $(API_PROJ)

database-update:
	@echo "Updating database to latest migration..."
	dotnet ef database update --project $(INFRA_PROJ) --startup-project $(API_PROJ)

database-drop:
	@echo "Dropping database..."
	dotnet ef database drop --project $(INFRA_PROJ) --startup-project $(API_PROJ) --force

.PHONY: describe logs pod-shell pod-status

describe:
	@echo "Describing pod in $(K8S_NAMESPACE) namespace..."
	kubectl describe pod $(POD_NAME) -n $(K8S_NAMESPACE)

logs:
	@echo "Fetching pod logs..."
	kubectl logs $(POD_NAME) -n $(K8S_NAMESPACE)

pod-shell:
	@echo "Opening shell in pod..."
	kubectl exec -it $(POD_NAME) -n $(K8S_NAMESPACE) -- /bin/bash

pod-status:
	@echo "Checking pod status..."
	kubectl get pod $(POD_NAME) -n $(K8S_NAMESPACE)

clean:
	@echo "Cleaning solution..."
	dotnet clean

restore:
	@echo "Restoring dependencies..."
	dotnet restore

build:
	@echo "Building solution..."
	dotnet build --configuration Release

test:
	@echo "Running tests..."
	dotnet test
