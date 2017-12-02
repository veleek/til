# Switch to a directory using a path from a file

I've recently been fiddling around with [Home Assistant](http://www.home-assistant.io) as a home automation platform, and as a result I've been working a little bit more with `bash` than I usually do through SSH on the Raspberry Pi 3.

As part of trying to get [my Z-Wave Garage Door Opener (GD00Z-4)](http://www.gocontrol.com/detail.php?productId=4) working with Home Assistant, I had to tweak the version of [OpenZWave](https://github.com/OpenZWave/open-zwave) that I was using.  The currently released version doesn't support "Covers" (e.g. controllable doors, electric blinds, etc.) so I had to pull in a commit or two from the dev branch.  And as part of THAT process I was trying to understand exactly how Python's [PIP 'editable' installs](https://pip.pypa.io/en/stable/reference/pip_install/#editable-installs) work.  

That's all a roundabout way of saying that I had an `.egg-link` file which contained a file path, and I wanted an easy way to navigate to that path without having to manually dump the value, highlight it using my mouse, and paste it back into a new command.

## Using bash variables in the command line

This part is relatively straight forward.  You can reference any variable using the `$variable` format in a command line

```bash
$ my_path=~/some/path
$ cd $my_path
```

This can be a little finiky though.  If you quote the variable assignment, you get an error which does little to help understand the problem:

```bash
$ my_path="~/some/path"
$ cd $my_path
-bash: cd: ~/src: No such file or directory
```

## Using bash command output in the command line

Using the output from another command in a bash command line is similar.  You can interpolate a command using `$(other --command)` or alternatively `` `other --command` ``

```bash
$ cd $(echo ~/some/path)
# or 
$ cd `echo ~/some/path`
```

## Navigating to a path from a file

Now I've got a `.egg-link` file which points to the folder that I want to navigate to.  I can easily use the `cat` command to dump the output.

```bash
(venv) pi@raspberrypi:~/src/home-assistant/venv/lib/python3.5/site-packages $ cat python-openzwave.egg-link
/home/pi/src/python-openzwave
.(venv) pi@raspberrypi:~/src/home-assistant/venv/lib/python3.5/site-packages $
```

You might have already spotted the problem that I'm going to run into based on this output, but lets step through the whole thing.  My first attempt was simple:

```bash
(venv) pi@raspberrypi:~/src/home-assistant/venv/lib/python3.5/site-packages $ cd $(cat python-openzwave.egg-link)
-bash: cd: too many arguments
```

Hmmm... it doesn't LOOK like too many arguments.  Oh wait, there's actually a second line that contains a single dot (`.`) and no newline at the end of the file.  Good thing I noticed that right away and definitely didn't spend 10 minutes trying to figure out what was wrong.  

Okay, so I need some way to pull out just the first line of the file and we can accomplish this pretty easily:

```bash
(venv) pi@raspberrypi:~/src/home-assistant/venv/lib/python3.5/site-packages $ head -1 python-openzwave.egg-link
/home/pi/src/python-openzwave
```

Now  we can add this to our command to get the result that we want:

```bash
(venv) pi@raspberrypi:~/src/home-assistant/venv/lib/python3.5/site-packages $ cd $(head -1 python-openzwave.egg-link)
(venv) pi@raspberrypi:~/src/python-openzwave $
```