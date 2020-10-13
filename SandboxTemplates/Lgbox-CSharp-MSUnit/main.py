import errno
from functools import wraps
import json
import os
import re
import signal
import subprocess
import sys

class TimeoutError(Exception):
    pass

def timeout(seconds=10, error_message=os.strerror(errno.ETIME)):
    def decorator(func):
        def _handle_timeout(signum, frame):
            raise TimeoutError(error_message)

        def wrapper(*args, **kwargs):
            signal.signal(signal.SIGALRM, _handle_timeout)
            signal.alarm(seconds)
            try:
                result = func(*args, **kwargs)
            finally:
                signal.alarm(0)
            return result

        return wraps(func)(wrapper)

    return decorator

def run(cmd):
    proc = subprocess.Popen(cmd,
        stdout = subprocess.PIPE,
        stderr = subprocess.PIPE,
    )
    stdout, stderr = proc.communicate()
 
    return proc.returncode, stdout, stderr

@timeout(10)
def main():
    # Run "dotnet build" to check, whether the project succeeds
    # to build itself.
    # 
    # If it doesn't, the code will prematurely return information 
    # about the unsuccessful build.
    code, out, err = run([
        "dotnet", "build",
        "--nologo"
    ])

    if code != 0:
        # String manipulation to remove unecessary information about 
        # the build failure & only provide necessary parts about it.
        out = out.split("Build FAILED.")[0].split("\n")[2:]
        out = '\n'.join(out).strip()
        print json.dumps({'type': 'build-failed', 'stdout': out, 'stderr': err})
        sys.exit(1)

    # Run "dotnet test" to check for test runs.
    #
    # This also outputs the contents of a test run into a file,  
    # which will be later used to parse the results of a test.
    code, out, err = run([
        "dotnet", "test", 
        "--logger", "trx;logfilename=testResult.trx", 
        "--results-directory", ".", 
        "--verbosity", "quiet", 
        "--nologo"
    ])

    if code != 0:
        print json.dumps({'type': 'test-failed', 'stdout': out, 'stderr': err})
        sys.exit(1)

    print json.dumps({'type': 'test-succeded', 'stdout': out})

if __name__ == "__main__":
    try:
        main()
    except TimeoutError:
        print json.dumps({'type': 'timeout'})
        sys.exit(1)