PROJ_NAME := GameCloud
API_PROJ := ./Presentation/GameCloud.WebAPI
INFRA_PROJ := ./Infrastructure/GameCloud.Persistence
K8S_NAMESPACE := gamecloud
POD_NAME := gamecloud-main-6bcd8476d-bcv2x
DEPLOYMENT_NAME := gamecloud-main
IMAGE_TAG := latest

.PHONY: run run-debug run-release clean restore build test

run:
	@echo "Starting application in default mode..."
	ASPNETCORE_ENVIRONMENT=Development dotnet run --project $(API_PROJ) --launch-profile https

run-debug:
	@echo "Starting application in debug mode..."
	dotnet run --project $(API_PROJ) --configuration Debug

run-release:
	@echo "Starting application in release modes..."
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

.PHONY: deploy rollout-status rollout-history rollout-undo scale get-deployments get-services get-nodes cluster-info

.PHONY: update-pod-name

update-pod-name:
	@echo "Updating POD_NAME in Makefile..."
	@POD=$$(kubectl get pods -n $(K8S_NAMESPACE) -l app=$(DEPLOYMENT_NAME) -o jsonpath='{.items[0].metadata.name}') && \
	sed -i '' 's/POD_NAME := gamecloud-main-6bcd8476d-bcv2x

# Add to deploy target
deploy:
	@echo "Deploying application to Kubernetes..."
	kubectl apply -k k8s/overlays/production -n $(K8S_NAMESPACE)
	@sleep 5
	@$(MAKE) update-pod-name

rollout-undo:
	@echo "Rolling back to previous version..."
	kubectl rollout undo deployment/$(DEPLOYMENT_NAME) -n $(K8S_NAMESPACE)
	@sleep 5
	@$(MAKE) update-pod-name

scale:
	@echo "Scaling deployment to $(word 2,$(MAKECMDGOALS)) replicas..."
	kubectl scale deployment/$(DEPLOYMENT_NAME) --replicas=$(word 2,$(MAKECMDGOALS)) -n $(K8S_NAMESPACE)

get-deployments:
	@echo "Listing all deployments..."
	kubectl get deployments -n $(K8S_NAMESPACE)

get-services:
	@echo "Listing all services..."
	kubectl get services -n $(K8S_NAMESPACE)

get-nodes:
	@echo "Listing all nodes..."
	kubectl get nodes

cluster-info:
	@echo "Displaying cluster information..."
	kubectl cluster-info

# Usage examples as comments:
# make scale 3        # Scale to 3 replicas
# make deploy        # Deploy the application
# make rollout-undo # Rollback to previous version
