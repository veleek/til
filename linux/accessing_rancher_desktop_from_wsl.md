# Accessing Rancher Desktop from WSL

Wow, it has been a LOOOONG time since [the last TIL I wrote about HomeAssistant](linux/switch_to_a_directory_in_bash_using_a_path_from_a_file.md) (and a long time since I wrote a TIL at all).  A lot has changed in Home Assistant, including the best way to do development with it.  [DevContainers](https://code.visualstudio.com/docs/remote/containers) provides the best way to get started as you can use a full Linux based development environment while still running VS Code seamlessly in Windows.  

You can use either Docker or Rancher to run your containers on Windows, which both use WSL as the backend.  The devcontainer will get created within the WSL sandbox you're using and there are plenty of guides about getting started.  In my case, I'm using Rancher Desktop, so I followed [the instructions they provide](https://docs.rancherdesktop.io/how-to-guides/vs-code-remote-containers/) to get started.

## Very slow git performance in Dev Container

WSL2 has [a bunch of advantages](https://docs.microsoft.com/en-us/windows/wsl/compare-versions) over WSL, but it has one unfortunate drawback.  While it's possible to access files in the Windows file system (using paths like `/mnt/c/users/...`) this is significantly slower than accessing files in the "native" file system.  So, if you follow all the regular setup guides, you'll end up cloning a repository in Windows, and then opening it in a Linux container in WSL.  Many of the common `git` commands make lots of little calls to the filesystem which significantly slows down performance.  For a large git repository, it can make it very difficult to do things like checkin files and push branches. 

The solution to this problem is thankfully pretty easy.  Instead of cloning the repository in Windows, clone it from within WSL.  

1. Open a WSL distro (not the `docker-desktop` or `rancher-desktop` one)
2. Clone the repository from within Linux.
3. Launch vscode from within WSL using `vscode <repo_path>`.
4. It should prompt you to reopen the folder in a container because the repo has a `devcontainer.json` configuration.  

You can also still access the source from within Windows using the WSL path (e.g. `\\wsl.localhost\Ubuntu\home\veleek\src\sample-repo`) and it should work fine.

## Unable to access the docker daemon when using Rancher Desktop.

If you're using Docker Desktop, everything may just work fine.  Docker Desktop has some automatic configuration to expose it's docker socket to WSL distributions.  When creating a Dev Container in VSCode, it'll use that docker socket and create a container in Docker Desktop (accessible as normal from Windows).

However, if you're using Rancher Desktop you may run into some problems.  Even though Rancher exposes some configuration to enable "WSL Integration" it doesn't seem to automatically update the WSL enironment to use it by defaut and I ran into errors stating that the docker daemon was unaccessible when creating dev containers.  

The fix to this is to manually update the `DOCKER_HOST` in the WSL container to point to the Rancher Desktop `docker.sock`

```bash
sudo chown root:docker /mnt/wsl/rancher-desktop/run/docker.sock
export DOCKER_HOST=unix:///mnt/wsl/rancher-desktop/run/docker.sock
```

If the path doesn't exist, you may need to re-enable the integration in Rancher Desktop and restart rancher and/or WSL.  Additionally, you may need to manually install the docker client so that you have access to the `docker` command from within WSL.  After that's done, you should be good to go.  Dev containers should open using the WSL distro and create containers in the Windows based docker client of your choice so you don't need to switch back and forth to manage a docker daemon, and you should get the performance you expect out of all the git commands

## Additional resources

* [rancher/desktop#640](https://github.com/rancher-sandbox/rancher-desktop/issues/640) was the final source of the fix.
* https://benc.dev/blog/devcontainers-wsl2 was a good starting point for trying to figure out what I needed to do, but it's focused on running docker within Linux to avoid needing to install Docker Desktop or Rancher Desktop.
* https://stuartleeks.com/posts/vscode-devcontainers-wsl/ is referenced by the above article, and is mostly just a duplicate of the VS Code documentation for getting started, but was helpful none the less.
