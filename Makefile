PROJ_NAME := GameCloud
API_PROJ := ./Presentation/GameCloud.WebAPI
INFRA_PROJ := ./Infrastructure/GameCloud.Persistence
K8S_NAMESPACE := gamecloud
POD_NAME := gamecloud-main-576ccfc9d6-5g2hf    
DEPLOYMENT_NAME := gamecloud-main
IMAGE_TAG := latest

# Go-related variables
GO_RELAY_DIR := ./relay
GO_RELAY_BIN := relay-server
GO_OUTPUT_DIR := ./bin/relay

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

# Go-related tasks
.PHONY: go-init go-deps go-build go-run go-test go-clean go-proto go-test-ws cs-proto

go-init:
	@echo "Initializing Go module in $(GO_RELAY_DIR)..."
	@mkdir -p $(GO_RELAY_DIR)
	@cd $(GO_RELAY_DIR) && go mod init github.com/turanheydarli/gamecloud/relay

go-deps:
	@echo "Installing Go dependencies..."
	@cd $(GO_RELAY_DIR) && go mod tidy

go-build:
	@echo "Building Go relay components..."
	@mkdir -p $(GO_OUTPUT_DIR)
	@cd $(GO_RELAY_DIR) && go build -o ../$(GO_OUTPUT_DIR)/$(GO_RELAY_BIN) ./cmd/relay-server

go-run:
	@echo "Running Go relay server..."
	@cd $(GO_RELAY_DIR) && go run ./cmd/relay-server/main.go

go-test:
	@echo "Running Go tests..."
	@cd $(GO_RELAY_DIR) && go test ./...

go-clean:
	@echo "Cleaning Go build artifacts..."
	@rm -rf $(GO_OUTPUT_DIR)
	@find $(GO_RELAY_DIR) -type f -name "*.exe" -delete

go-proto:
	@echo "Generating Go code from protobuf definitions..."
	@mkdir -p $(GO_RELAY_DIR)/proto
	@echo "Installing specific versions of protoc plugins..."
	@cd $(GO_RELAY_DIR) && go install google.golang.org/protobuf/cmd/protoc-gen-go@v1.28.1
	@cd $(GO_RELAY_DIR) && go install google.golang.org/grpc/cmd/protoc-gen-go-grpc@v1.2.0
	@echo "Generating protobuf code..."
	@PATH=$$PATH:$(HOME)/go/bin protoc \
		--go_out=paths=source_relative:$(GO_RELAY_DIR)/proto \
		--go-grpc_out=paths=source_relative:$(GO_RELAY_DIR)/proto \
		--proto_path=./proto \
		./proto/*.proto
	@echo "Go protobuf files generated successfully in relay/proto"

go-test-ws:
	@echo "Running WebSocket test client..."
	@cd $(GO_RELAY_DIR) && go run ./cmd/test-client/main.go

# C#-related proto tasks
cs-proto:
	@echo "Generating C# code from protobuf definitions..."
	@dotnet build ./Infrastructure/GameCloud.Proto/GameCloud.Proto.csproj

# Combined tasks
.PHONY: build-all run-all clean-all init-all

build-all: build go-build cs-proto
	@echo "Built all components"

run-all:
	@echo "Starting all services..."
	@$(MAKE) run & $(MAKE) go-run

clean-all: clean go-clean
	@echo "Cleaned all components"

init-all: restore go-init go-deps cs-proto
	@echo "Initialized all components"

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

# Docker-related tasks for the Go relay
.PHONY: docker-build-relay docker-push-relay

docker-build-relay:
	@echo "Building Docker image for relay server..."
	docker build -t $(PROJ_NAME)/relay:$(IMAGE_TAG) -f relay/Dockerfile ./relay

docker-push-relay:
	@echo "Pushing relay Docker image..."
	docker push $(PROJ_NAME)/relay:$(IMAGE_TAG)

.PHONY: deploy rollout-status rollout-history rollout-undo scale get-deployments get-services get-nodes cluster-info

.PHONY: update-pod-name

update-pod-name:
	@echo "Updating POD_NAME in Makefile..."
	@POD=$$(kubectl get pods -n $(K8S_NAMESPACE) -l app=$(DEPLOYMENT_NAME) -o jsonpath='{.items[0].metadata.name}')

deploy:
	@echo "Deploying application to Kubernetes..."
	kubectl apply -k k8s/overlays/production -n $(K8S_NAMESPACE)
	@sleep 5
	@$(MAKE) update-pod-name

# Deploy relay server to Kubernetes
.PHONY: deploy-relay

deploy-relay:
	@echo "Deploying relay server to Kubernetes..."
	kubectl apply -k k8s/overlays/relay -n $(K8S_NAMESPACE)

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