VERSION=$$(grep -oP "AssemblyInformationalVersion\(\"\K[^\"]+" Properties/AssemblyInfo.cs)
TAG=registry.digitalocean.com/unisave/unisave-compiler:$(VERSION)

.PHONY: dev prod

dev:
	@eval $$(minikube docker-env) ;\
		export DOCKER_BUILDKIT=1 ;\
		docker image build -t $(TAG) -f Dockerfile .
	kubectl set image deployment/compiler compiler=$(TAG)
	kubectl scale --replicas=0 deployment/compiler
	kubectl scale --replicas=1 deployment/compiler

prod:
	export DOCKER_BUILDKIT=1 ;\
		docker image build -t $(TAG) -f Dockerfile .
	docker push $(TAG)
