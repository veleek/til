# Accessing Rancher Desktop from WSL

Wow, it has been a LOOOONG time since [the last TIL I wrote about HomeAssistant](...tbd) (and a long time since I wrote a TIL at all)...

https://github.com/rancher-sandbox/rancher-desktop/issues/640 - The fix
https://benc.dev/blog/devcontainers-wsl2/ - The sorcue
https://stuartleeks.com/posts/vscode-devcontainers-wsl/ - The source of the source
https://docs.rancherdesktop.io/how-to-guides/vs-code-remote-containers/ - instructions on setting up dev containers.

## Do this
sudo chown root:docker /mnt/wsl/rancher-desktop/run/docker.sock
export DOCKER_HOST=unix:///mnt/wsl/rancher-desktop/run/docker.sock
export KUBECONFIG=/mnt/c/Users/<USER>/.kube/config
