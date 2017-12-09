# `pip install` missing SSL module when inside a virtual environment

If you're attempting to use Python's virtual environments on windows there's a chance that you may run into this error 

 pip is configured with locations that require TLS/SSL, however the ssl module in Python is not available.
 
while trying to install additional modules using `pip` and then the installation will fail.  In general `pip` should work perfectly fine with a default python installation, so there's likely something wrong with the virtual environment configuration.  You can confirm this by launching python from inside and outside you virtual environment and running the following command:

```python
import ssl
```

With my configuration, when running inside the venv, I would get an error about something for TLS 1.3 missing but not in the regular environment.  So there are a few things we can do to figure out what's wrong.  Again, run the following commands from inside and outside of your venv:

```python
import sys
import _ssl

print(sys.modules["_ssl"])
```

Take note of both of the paths (which should point to `_ssl.pyd` files).  Obviously inside of the venv it likely points to a different location.  In my case the files were different so I had *somehow* managed to get multiple version installed and venv had managed to pickup and older one.  I tried manually copying the one from the python install directory into the venv directory and I was able to run `pip install` properly with no warnings afterwards.